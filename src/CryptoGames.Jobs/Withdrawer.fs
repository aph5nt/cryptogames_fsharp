namespace CryptoGames

open System
open System.Threading
open System.Collections.Specialized
open Quartz
open Quartz.Impl
open CryptoGames.JobTypes 
 
module WithdrawerJobs =

    [<Literal>] 
    let groupName = "withdraws"

    [<Literal>] 
    let networkStr = "network"

    [<Literal>] 
    let bank = "bank"

    [<Literal>]
    let blockVersionStr = "blockVersion"

    [<Literal>] 
    let withdrawWatcher = "withdrawWatcher"

    module WithdrawVerifier =
        [<DisallowConcurrentExecution>]
        type Job() =
            interface IJob with 
                member x.Execute context = 
                    ()

        let Schedule (scheduler : IScheduler) (network : Network) (txHash : string) = 
            ()

    module WatchWithdrawsJob =
        open QBitNinja.Client
 
        [<DisallowConcurrentExecution>]
        [<PersistJobDataAfterExecution>]
        type Job(dependency : Withdrawer.Dependency) =
            interface IJob with 
                member x.Execute context = 
                    let data = context.JobDetail.JobDataMap
                    let network = data.GetString networkStr |> Network.Parse
                    let blockVersion = data.GetInt blockVersionStr
                    let bankAddress = data.GetString bank

                    let currentBlock = dependency.QBitNinjaClient.GetBlock(new Models.BlockFeature(), true) |> Async.AwaitTask |> Async.RunSynchronously
                    if blockVersion < currentBlock.Block.Header.Version then
                        let withdraws = dependency.Database.Withdraws.GetByStatus network TranStatus.Pending
                        let txHash = 
                            dependency.ExternalApiMap.[network].WithdrawTransfer
                            <| bankAddress
                            <| (withdraws |> List.map(fun w -> (w.ToAddress, w.Amount)))
                        match txHash with
                            | None -> ()
                            | Some(txHash) ->
                                context.Put(blockVersion, currentBlock.Block.Header.Version)
                                WithdrawVerifier.Schedule 
                                <| context.Scheduler
                                <| network
                                <| txHash

        let Schedule (scheduler : IScheduler) (args : Withdrawer.Args) =
            let startTime = DateBuilder.NextGivenSecondDate(new Nullable<DateTimeOffset>(), 10)
            let jobName = sprintf "%s-%s" withdrawWatcher (args.Network.ToString())
            let job = JobBuilder.Create<Job>().WithIdentity(jobName, groupName).Build()
            job.JobDataMap.Put(networkStr, args.Network.ToString())
            job.JobDataMap.Put(bank, args.BankAddress)
            
            let trigger = TriggerBuilder.Create().WithIdentity(jobName, groupName).StartAt(startTime).WithSimpleSchedule(fun x -> x.WithIntervalInMinutes(10).RepeatForever() |> ignore).Build()
            if scheduler.CheckExists(job.Key) = true then
                scheduler.UnscheduleJob(trigger.Key) |> ignore
            scheduler.ScheduleJob(job, trigger) |> ignore      


                   
