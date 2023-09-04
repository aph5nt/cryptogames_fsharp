namespace CryptoGames.Game.Mines

open System
open CryptoGames
//open CryptoGames.Game.Mines.Types
//open CryptoGames.Player.Types

module Game =
    ()
    //let GameName network = (sprintf "%s-%s" (Mines.ToString()) (network.ToString()))

    //type GameActor(dependency : Game.Dependency, args : Game.Args) =
    //    inherit UntypedActor()

    //    let mutable state : Option<State> = None
 
    //    override x.PreStart() = 
    //        let game = dependency.Database.Games.Get GameType.Mines args.Network args.UserName 
    //        state <- game |> Option.map (fun g -> dependency.Database.GameSettings.Deserialize g.Data)

    //    override x.OnReceive msg =
    //        match msg with
    //        | :? Commands.Start as cmd  -> 
    //            if cmd.GameSettings.Network <>  args.Network then 
    //                failwith "tried to start game for invalid network"
    //            state <- StartCommand.Execute (x.Self, x.Sender) state cmd.GameSettings dependency
    //        | :? Commands.Move as cmd -> state <- MoveCommand.Execute (x.Self, x.Sender) dependency (state, cmd.Position)
    //        | :? Commands.TakeAway  -> state <- TakeAwayCommand.Execute (x.Self, x.Sender) dependency state
    //        | :? Commands.TakeWin  -> TakeWinCommand.Execute  args dependency state
    //        | :? Commands.TakeLoss ->       TakeLossCommand.Execute args dependency state
    //        | :? Commands.Get ->
    //                    match state with
    //                    | Some(s) -> x.Sender <! s.UserState
    //                    | None -> x.Sender <! "Empty"
                
    //        | _ -> x.Unhandled(msg)

    //    interface IDisposable with
    //        member x.Dispose() =
    //            GC.SuppressFinalize(x)

    //    static member Create (context : IActorContext) (dependencies : Game.Dependency, args : Game.Args) =
    //        let name = GameName args.Network
    //        let props = Akka.Actor.Props.Create(typeof<GameActor>, dependencies, args)
    //        let actorRef = context.ActorOf(props, name);
    //        actorRef      