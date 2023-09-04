namespace CryptoGames

open Akka.Actor
open Akka.FSharp
open System
open CryptoGames.Player.Types
open CryptoGames.Game.Mines

module Player =
    ()
    //type PlayerActor(dependency : Dependency, args : Args) =
    //    inherit UntypedActor()

    //    let games = new GameMap()
    //    let context = UntypedActor.Context :> IActorContext
    //    let log = lazy (Akka.Event.Logging.GetLogger(context))
        
    //    let createGame system network =
    //        let internalApi = dependency.Api.InternalApi
    //        let externalApi = dependency.Api.ExternalApis.[network]
            
    //        let gameDependency = {
    //            Game.Dependency.Create = dependency.Create
    //            Game.Dependency.Database = dependency.Database
    //            Game.Dependency.InternalApi = dependency.Api.InternalApi
    //            Game.Dependency.ExternalApi = dependency.Api.ExternalApis.[network]
    //        }

    //        let gameArgs address =
    //            {  Game.Args.Network = network
    //               Game.Args.UserAddress = address
    //               Game.Args.UserName = args.UserName
    //               Game.Args.BankAddress = args.BankAddresses.[network] }

    //        let data = internalApi.GetState network args.UserName
    //        match data with
    //        | None ->
    //            let result = externalApi.GetBalance args.UserName 
    //            match result with
    //            | Some(balance) ->
    //                // no internal, but exsiting external
    //                internalApi.UpdateBalance network args.UserName balance.Address balance.Balance
    //                Game.GameActor.Create context (gameDependency, gameArgs balance.Address)
    //            | None -> 
    //                // no internal and external
    //                let extWallet = externalApi.CreateAddress args.UserName
    //                internalApi.UpdateBalance network args.UserName extWallet.Address extWallet.Balance
    //                Game.GameActor.Create context (gameDependency, gameArgs extWallet.Address)
                
    //        | Some(balance) -> 
    //            Game.GameActor.Create context (gameDependency, gameArgs balance.Address)

    //    override x.OnReceive msg = 
    //        match msg with
    //        | :? Player.Commands.Start ->
    //                let networks = 
    //                    (dependency.Database.Balances.GetAll args.UserName)
    //                    |> Seq.toArray 
    //                    |> Array.map(fun i -> i.Network)
    //                    |> Array.append([|FREE|])
    //                    |> Array.distinct
    //                for network in networks do 
    //                    let game = createGame context.System network
    //                    games.Add(network, game)
    //                context.Sender.Tell("OK")
    //        | :? Player.Commands.ActivateNetwork as cmd -> 
    //            let balance = dependency.Database.Balances.Get cmd.Network args.UserName 
    //            match balance with
    //            | Some(_) -> 
    //                log.Value.Warning("tried to activate existing network for {UserName}", args.UserName)
    //                context.Sender.Tell("Failed")
    //            | None ->
    //                let game = createGame context.System cmd.Network
    //                games.Add(cmd.Network, game)
    //                context.Sender.Tell("OK")
    //        | :? Player.Commands.Withdraw as cmd ->
    //            let withdrawDependency = {
    //                WithdrawCmd.Types.Dependency.Database = dependency.Database
    //                WithdrawCmd.Types.Dependency.InternalApi = dependency.Api.InternalApi
    //            }

    //            let withdrawArgs = {
    //                WithdrawCmd.Types.Args.Network = cmd.Network
    //                WithdrawCmd.Types.Args.Amount = cmd.Amount
    //                WithdrawCmd.Types.Args.ToAddress = cmd.ToAddress
    //                WithdrawCmd.Types.Args.UserName = args.UserName
    //            }

    //            let result = WithdrawCmd.Execute withdrawDependency  withdrawArgs
    //            x.Sender <! result

    //        | _ -> x.Unhandled(msg)

    //    interface IDisposable with
    //        member x.Dispose() =
    //            GC.SuppressFinalize(x)

    //    static member Create (context : IActorContext) (dependencies : Dependency, args : Args) =
    //        let name = args.UserName
    //        let props = Akka.Actor.Props.Create(typeof<PlayerActor>, dependencies, args)
    //        let actorRef = context.ActorOf(props, name);
    //        actorRef