namespace CryptoGames.Game.Mines
    open Chessie.ErrorHandling
    open Types
    open Akka.FSharp

    module TakeAwayCommand =
        module Validation =
            
            let isAlive(state : State) =
                if state.UserState.Status = Alive then ok(state)
                else fail "isAlive"

            let isInitialized(state : Option<State>) =
                match state with
                | None -> fail "The game has been not initialized"
                | _ -> ok(state.Value)

         module Handlers = 
            open Akka.Actor
            let revealUserBoard (state : State) =
               for x in [0 .. (xMaxPosition state)] do
                   for y in [0 .. (yMaxPosition state)] do
                       state.UserState.Board.[x,y] <- state.GameState.Board.[x,y]
               state.UserState.Board

            let update (self : IActorRef) (state : State) =
                let userState = { state.UserState with Win = State.GetWin state; Status = Escaped; Board = revealUserBoard state }
                let stateR = { state  with UserState = userState }
                self <! new Commands.TakeWin()
                ok(stateR)
 
         open Validation
         open Handlers
         open Akka.Actor
         open CryptoGames.Player.Types

         let Execute (self : IActorRef, sender : IActorRef) (dependency : Game.Dependency) (state : Option<State>) = 
            ok (state)
            >>= isInitialized
            >>= isAlive
            >>= update self
            >>= saveStateHandler dependency
            |> eitherTee (fun (state, _) -> sender <! state.UserState) (fun mgs -> sender <! mgs)
            |> either (fun(s,_)->Some(s)) (fun(_)-> state)