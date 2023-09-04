namespace CryptoGames

open System
open System.Threading
open System.Collections.Specialized
open Quartz
open Quartz.Impl
open CryptoGames.JobTypes 

 
module ForwarderJobs =
 
    [<Literal>] 
    let groupName = "deposits"

    [<Literal>] 
    let bank = "bank"

    [<Literal>] 
    let depositWatcher = "depositWatcher"

    [<Literal>] 
    let networkStr = "network"
    
    module UpdateBalanceJob =
        
        [<Literal>] 
        let retryCount = "retryCount"

        [<Literal>] 
        let maxRetryCount = "maxRetryCount"

        [<Literal>] 
        let transactionHash = "transactionHash"
 
        [<DisallowConcurrentExecution>]
        type Job(dependency : DepositForwarder.Dependency) =
            interface IJob with 
                member x.Execute context = 
                    let data = context.JobDetail.JobDataMap
                    let key = context.JobDetail.Key
                    let txhash = data.GetString transactionHash
                    let network = data.GetString networkStr |> Network.Parse
                   
                    let forward = dependency.Database.Forwards.Get network txhash
                    match forward with
                    | None ->
                        // log error!
                        context.Scheduler.UnscheduleJob(new TriggerKey(key.Name, key.Group)) |> ignore
                    | Some(forward) ->

                        if forward.CreatedAt.AddHours(3.) < DateTime.UtcNow then
                            dependency.Database.Forwards.Update network txhash Timeout
                            context.Scheduler.UnscheduleJob(new TriggerKey(key.Name, key.Group)) |> ignore
                        else
                            match forward.Status with
                            | Awaiting ->
                                try
                                    let client = new QBitNinja.Client.QBitNinjaClient(getNetwork network)
                                    let tx = client.GetTransaction (NBitcoin.uint256.Parse(txhash)) |> Async.AwaitTask |> Async.RunSynchronously
                                    if tx <> null then
                                        if tx.Block <> null then
                                            if tx.Block.Confirmations >= 1 then
                                                dependency.Database.Forwards.Update network txhash Confirmed
                                                dependency.InternalApi.UpdateBalance 
                                                <| forward.Network
                                                <| forward.UserName
                                                <| forward.Address
                                                <| forward.Amount
                                                context.Scheduler.UnscheduleJob(new TriggerKey(key.Name, key.Group)) |> ignore
                                with
                                | exn ->
                                    // log error here
                                    ()
                            | _ -> 
                                // nothing left to do...
                                context.Scheduler.UnscheduleJob(new TriggerKey(key.Name, key.Group)) |> ignore
 
        let Schedule (scheduler : IScheduler) (network : Network) userName  (txHash : string) =
            let startTime = DateBuilder.NextGivenSecondDate(new Nullable<DateTimeOffset>(), 15)
            let jobName = sprintf "updateBalance-@%s-%s" (network.ToString()) transactionHash
            let job = JobBuilder.Create<Job>().WithIdentity(jobName, groupName).Build()

            // init job data
            job.JobDataMap.Put(networkStr, network.ToString())
            job.JobDataMap.Put(transactionHash, txHash)
            
            let trigger = TriggerBuilder.Create().WithIdentity(jobName, groupName).StartAt(startTime).WithSimpleSchedule(fun x -> x.WithIntervalInSeconds(60).RepeatForever() |> ignore).Build()
            if scheduler.CheckExists(job.Key) = false then
                scheduler.ScheduleJob(job, trigger) |> ignore
            

    module WatchDepositsJob =
        [<DisallowConcurrentExecution>]
        type Job(dependency : DepositForwarder.Dependency) =
            interface IJob with 
                member x.Execute context = 
                    let data = context.JobDetail.JobDataMap
                    let network = data.GetString networkStr |> Network.Parse
                    let bankAddress = data.GetString bank
                    let externalApi = dependency.ExternalApiMap.[network]
                    dependency.ExternalApiMap.[network].GetBalances bankAddress
                    |> Array.filter(fun f -> f.Balance > 0m)
                    |> Array.iter(fun b -> 
                        Thread.Sleep 100
                        let fee = Services.Payment.Fee.ForNetwork network
                        let amountToForward = b.Balance - fee.Amount
                        if amountToForward > 0m then
                            let txHash = externalApi.DepositTransfer b.UserName b.Address bankAddress amountToForward
                            match txHash with
                            | None -> ()
                            | Some(txHash) ->
                                UpdateBalanceJob.Schedule 
                                <| context.Scheduler
                                <| network
                                <| b.UserName
                                <| txHash
                    )

        let Schedule (scheduler : IScheduler) (args : DepositForwarder.Args) =
            let startTime = DateBuilder.NextGivenSecondDate(new Nullable<DateTimeOffset>(), 10)
            let jobName = sprintf "%s-%s" depositWatcher (args.Network.ToString())
            let job = JobBuilder.Create<Job>().WithIdentity(jobName, groupName).Build()
            job.JobDataMap.Put(networkStr, args.Network.ToString())
            job.JobDataMap.Put(bank, args.BankAddress)
            
            let trigger = TriggerBuilder.Create().WithIdentity(jobName, groupName).StartAt(startTime).WithSimpleSchedule(fun x -> x.WithIntervalInMinutes(3).RepeatForever() |> ignore).Build()
            if scheduler.CheckExists(job.Key) = true then
                scheduler.UnscheduleJob(trigger.Key) |> ignore
            scheduler.ScheduleJob(job, trigger) |> ignore
    
    module WatchFreeDepositsJob =
        [<DisallowConcurrentExecution>]
        type Job(dependency : DepositForwarder.Dependency) =
            interface IJob with 
                member x.Execute context = 
                    let data = context.JobDetail.JobDataMap
                    let network = data.GetString networkStr |> Network.Parse
                    let bankAddress = data.GetString bank
                    let externalApi = dependency.ExternalApiMap.[FREE]
                    externalApi.GetBalances bankAddress
                    |> Array.iter(fun b -> 
                        let fee = Services.Payment.Fee.ForNetwork network
                        let amount = b.Balance - fee.Amount
                        if amount > 0m then
                            externalApi.DepositTransfer b.UserName b.Address bankAddress amount |> ignore
                            dependency.InternalApi.UpdateBalance network b.UserName b.Address amount
                    )
                    
        let Schedule (scheduler : IScheduler) (args : DepositForwarder.Args) =
            let startTime = DateBuilder.NextGivenSecondDate(new Nullable<DateTimeOffset>(), 10)
            let jobName = sprintf "%s-%s" depositWatcher (args.Network.ToString())
            let job = JobBuilder.Create<Job>().WithIdentity(jobName, groupName).Build()
            job.JobDataMap.Put(networkStr, args.Network.ToString())
            job.JobDataMap.Put(bank, args.BankAddress)
             
            let trigger = TriggerBuilder.Create().WithIdentity(jobName, groupName).StartAt(startTime).WithSimpleSchedule(fun x -> x.WithIntervalInMinutes(1).RepeatForever() |> ignore).Build()
            if scheduler.CheckExists(job.Key) = true then
                scheduler.UnscheduleJob(trigger.Key) |> ignore

            scheduler.ScheduleJob(job, trigger) |> ignore

    type JobFactory(dependency : DepositForwarder.Dependency) =
        interface Quartz.Spi.IJobFactory with 
            member x.ReturnJob job = ()
            member x.NewJob (bundle, scheduler) = 
                let jobType = bundle.JobDetail.JobType
                match jobType with
                | t when t = typeof<UpdateBalanceJob.Job> ->
                    new UpdateBalanceJob.Job(dependency) :> IJob
                | t when t = typeof<WatchDepositsJob.Job> ->
                    new WatchDepositsJob.Job(dependency) :> IJob
                | t when t =typeof<WatchFreeDepositsJob.Job> ->
                    new WatchFreeDepositsJob.Job(dependency) :> IJob
                | _ -> failwith "not supported"        
                        
module DepositForwarder =
    module Internal =
        let properties = new NameValueCollection()
        properties.["quartz.scheduler.instanceName"] <- "ServerAppScheduler"
        properties.["quartz.scheduler.instanceId"] <- "ServerAppScheduler"
        properties.["quartz.threadPool.type"] <- "Quartz.Simpl.SimpleThreadPool, Quartz"
        properties.["quartz.threadPool.threadCount"] <- "1"
        properties.["quartz.jobStore.misfireThreshold"] <- "60000"
        properties.["quartz.jobStore.type"] <- "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"
        properties.["quartz.jobStore.useProperties"] <- "false"
        properties.["quartz.jobStore.dataSource"] <- "default"
        properties.["quartz.jobStore.tablePrefix"] <- "QRTZ_"
        properties.["quartz.jobStore.clustered"] <- "true"
        properties.["quartz.jobStore.driverDelegateType"] <- "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz"
        properties.["quartz.dataSource.default.connectionString"] <- System.Configuration.ConfigurationManager.ConnectionStrings.["default"].ConnectionString
        properties.["quartz.dataSource.default.provider"] <- "SqlServer-20"
        let schedulerFactory = new StdSchedulerFactory(properties);
        let scheduler = schedulerFactory.GetScheduler();

    open CryptoGames.JobTypes.DepositForwarder

    let InitScheduler dependency = 
        Internal.scheduler.JobFactory <- new ForwarderJobs.JobFactory(dependency)
        Internal.scheduler.Start()
        Internal.scheduler

    let Create() =
        let run scheduler dependency args =
            if args.Network = FREE then 
                ForwarderJobs.WatchFreeDepositsJob.Schedule 
                <| Internal.scheduler
                <| args
            else
                ForwarderJobs.WatchDepositsJob.Schedule
                <| Internal.scheduler
                <| args
           
        { Run = run }