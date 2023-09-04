namespace CryptoGames

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic
open CryptoGames.Player.Types

module PlayerManager =
    ()
    //type PlayerManagerActor(dependency : PlayerManager.Dependency, args : PlayerManager.Args) =
    //    inherit UntypedActor()

    //    let players = new Dictionary<string, IActorRef>()
    //    let context = UntypedActor.Context :> IActorContext
    //    let log = lazy (Akka.Event.Logging.GetLogger(context))

    //    override x.OnReceive msg = 
    //        match msg with
    //        | :? Commands.GetPlayers ->
    //            context.Sender.Tell((players.Keys |> Seq.toArray))
    //        | :? Commands.AttachPlayer as cmd ->
    //                if not (players.ContainsKey(cmd.UserName)) then
    //                    let playerArgs = { Player.Args.UserName = cmd.UserName; Player.Args.BankAddresses = args.BankAddresses }
    //                    let player = Player.PlayerActor.Create context (dependency.PlayerDependency, playerArgs)
    //                    player <? new Player.Commands.Start() |> Async.RunSynchronously |> ignore
    //                    players.[cmd.UserName] <- player
    //                    context.Sender <! "OK"
    //                else context.Sender <! "Failed" 
    //        | :? Commands.DetachPlayer as cmd ->
    //                if players.ContainsKey(cmd.UserName) then
    //                    try
    //                        players.[cmd.UserName].GracefulStop(TimeSpan.FromSeconds(10.))
    //                        |> Async.AwaitTask
    //                        |> Async.RunSynchronously
    //                        |> ignore

    //                        players.Remove(cmd.UserName) |> ignore
    //                        context.Sender <! "OK"
    //                    with exn ->
    //                        log.Value.Error("Failed to detach player {Name}, {Exception}", cmd.UserName, exn)
    //                        context.Sender <! "Failed"

    //        | _ -> x.Unhandled(msg)

    //    interface IDisposable with
    //        member x.Dispose() =
    //            GC.SuppressFinalize(x)

    //    static member Create (dependency : PlayerManager.Dependency) (args : PlayerManager.Args) =
    //        let name = "players"
    //        let props = Akka.Actor.Props.Create(typeof<PlayerManagerActor>, dependency, args)
    //        let actorRef = dependency.PlayerDependency.System.ActorOf(props, name);
    //        actorRef 