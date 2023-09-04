namespace CryptoGames

open Akka.Actor
open Akka.FSharp
open Serilog
open Serilog.Sinks.RollingFileAlternate
open CryptoGames.Player.Types
open CryptoGames.Game.Mines


module WebModule =
    //open CryptoGames.Types.Services
    ()
//    let Run (system : ActorSystem) =
//        let localPath = System.Reflection.Assembly.GetExecutingAssembly().Location
//        let logPath = System.IO.Path.Combine(localPath, "..\\logs")
//        let loggerCfg = new LoggerConfiguration()
//        Serilog.Log.Logger <- loggerCfg.WriteTo.ColoredConsole().MinimumLevel.Debug().WriteTo.RollingFileAlternate(logPath).WriteTo.Trace().CreateLogger()
 
//        UINotifier.Create().Run system |> ignore
        
//        { new IModule with
//            member x.PlayerManager() = 
//                system.ActorSelection("akka.tcp://GreedyRunSever@localhost:8086/user/players").ResolveOne(timeout) |> Async.AwaitTask |> Async.RunSynchronously
//            member x.Player userName =
//                let player = system.ActorSelection(sprintf "akka.tcp://GreedyRunSever@localhost:8086/user/players/%s" userName)
//                player.ResolveOne(timeout) |> Async.AwaitTask |> Async.RunSynchronously
//            member x.Game userName network =
//                let game = system.ActorSelection(sprintf "akka.tcp://GreedyRunSever@localhost:8086/user/players/%s/%s" userName (Game.GameName network))
//                game.ResolveOne(timeout) |> Async.AwaitTask |> Async.RunSynchronously  
//        }
         

//module Module = 
//    open CryptoGames.Types.Services
//    open Quartz.Impl
//    open System.Collections.Specialized
//    open CryptoGames.JobTypes

//    let Run (system : ActorSystem) createFn =
//        let localPath = System.Reflection.Assembly.GetExecutingAssembly().Location
//        let logPath = System.IO.Path.Combine(localPath, "..\\logs")
//        let loggerCfg = new LoggerConfiguration()
//        Serilog.Log.Logger <- loggerCfg.WriteTo.ColoredConsole().MinimumLevel.Debug().WriteTo.RollingFileAlternate(logPath).WriteTo.Trace().CreateLogger()
 
//        let api = 
//            Payment.Create().Run
//            <| { Payment.Dependency.System = system
//                 Payment.Dependency.EventStream = system.EventStream }
//            <| { Payment.Args.Networks = Networks }

//        let bankAddressMap = 
//            Bankroll.Create(Data.Database.Create()).Run 
//            <| { Bankroll.Dependency.Api = api }
//            <| { Bankroll.Args.Games = Games
//                 Bankroll.Args.Networks = Networks }

//        let playerManager = 
//            PlayerManager.PlayerManagerActor.Create
//            <|  { PlayerManager.Dependency.PlayerDependency = 
//                    { Player.Dependency.System = system
//                      Player.Dependency.EventStream = system.EventStream
//                      Player.Dependency.Api = api
//                      Player.Dependency.Create = createFn
//                      Player.Dependency.Database = Data.Database.Create() } }
//            <| { PlayerManager.Args.BankAddresses = bankAddressMap
//                 PlayerManager.Args.Networks = Networks
//                 PlayerManager.Args.Games = Games }
        
//        (* setting up the deposit forwarding *)
//        let forwarderDependency = 
//            { DepositForwarder.Dependency.Database = Data.Database.Create()
//              DepositForwarder.Dependency.ExternalApiMap = api.ExternalApis
//              DepositForwarder.Dependency.InternalApi = api.InternalApi
//              DepositForwarder.Dependency.System = system }

//        let forwarderScheduler = DepositForwarder.InitScheduler forwarderDependency

//        for network in Networks do
//            DepositForwarder.Create().Run 
//            <| forwarderScheduler
//            <| forwarderDependency
//            <| { DepositForwarder.Args.BankAddress = bankAddressMap.[network]
//                 DepositForwarder.Args.Network = network
//                 DepositForwarder.Args.Interval = DepositForwarder.Args.intervalMsFor network }

//        UINotfierProxy.Create().Run system

//        { new IModule with
//            member x.PlayerManager() = playerManager
//            member x.Player userName =
//                let query = sprintf "/user/players/%s" userName
//                (select query system).ResolveOne(timeout) |> Async.AwaitTask |> Async.RunSynchronously
//            member x.Game userName network =
//                let query = sprintf "/user/players/%s/%s" userName (Game.GameName network)
//                (select query system).ResolveOne(timeout) |> Async.AwaitTask |> Async.RunSynchronously  
//            }
 
//    module Api =

//        let AttachPlayer playerManager userName =
//            async {
//                let! _ = playerManager <? { PlayerManager.Commands.AttachPlayer.UserName = userName }
//                return ()
//            }

//        let ActivateNetwork player network =
//            async {
//                let! _ = player <? { Player.Commands.ActivateNetwork.Network = network }
//                return ()
//            }

//        let Withdraw player network toAddress amount =
//            async {
//                let! result = player <? { Player.Commands.Withdraw.Amount = amount; Player.Commands.Withdraw.Network = network; Player.Commands.Withdraw.ToAddress = toAddress }
//                return result
//            }

//        let DetachPlayer playerManager userName =
//            async {
//                let! _ = playerManager <? { PlayerManager.Commands.DetachPlayer.UserName = userName }
//                return ()
//            }