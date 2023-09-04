namespace CryptoGames.Game.Mines
    open System
    open Chessie.ErrorHandling
    open Types
    open Akka.FSharp.Actors

    module MoveCommand = 
        module Validation =
            
            let isInitialized(state : Option<State>, position : Position) =
                match state with
                | None -> fail "The game has been not initialized"
                | _ -> ok(state.Value, position)

            let isAlive (state : State, position : Position) =
                if state.UserState.Status = Alive then ok(state, position)
                else fail "isAlive"

            let isOutOfBoard (state : State, position : Position) =
                if (position.X <= (xMaxPosition state) && position.X >= 0) && (position.Y <= (yMaxPosition state) && position.Y >= 0) then 
                  ok(state, position)
                else fail "outOfBoard"

            //TODO: test it for moving backwards, notmoving
            let isMoveNotAllowed (state : State, position : Position) =
                if (position.X > state.UserState.Position.X) && (position.X - state.UserState.Position.X = 1) then
                    ok(state, position)
                else fail "moveNotAllowed"
            
 
        (* CUSTOM *)
        module Handlers =

            let private update (state : State) (position : Position)  =
                let updateUserBoard (state : State) (to' : Position) =
                    for x in [0 .. (xMaxPosition state)] do
                        for y in [0 .. (yMaxPosition state)] do
                            if x <= to'.X then
                                state.UserState.Board.[x,y] <- state.GameState.Board.[x,y]
                    state.UserState.Board

                let revealUserBoard (state : State) =
                    for x in [0 .. (xMaxPosition state)] do
                        for y in [0 .. (yMaxPosition state)] do
                            state.UserState.Board.[x,y] <- state.GameState.Board.[x,y]
                    state.UserState.Board
                 
                let userBoard stateArg (status : Status) =  
                    { stateArg with UserState = { stateArg.UserState with Board = updateUserBoard stateArg position; Position = position; Status = status } }

                let revealedBoard stateArg (status : Status) =
                    { stateArg with UserState = { stateArg.UserState with Board = revealUserBoard stateArg; Position = position; Status = status } }
                     
                match state.GameState.Board.[position.X, position.Y] with
                | Unknown -> state
                | Mined -> revealedBoard state Dead
                | Safe ->  if (xMaxPosition state) = position.X then
                                revealedBoard state Escaped
                           else userBoard state Alive

            let handleMove (state : State, position : Position)  =
                ok(update state position)
            
            open Akka.Actor

            let handleWin (self : IActorRef) (state : State) =
                if state.UserState.Status = Escaped then
                    let userState = { state.UserState with Win = State.GetWin state }
                    let stateR = { state  with UserState = userState }
                    self <! new Commands.TakeWin()
                    ok(stateR)
                else ok(state)

            let handleLoss (self : IActorRef) (state : State) =
                if state.UserState.Status = Dead then
                    let userState = { state.UserState with Loss = state.UserState.Bet }
                    let stateR = { state  with UserState = userState }
                    self <! new Commands.TakeLoss()
                    ok(stateR)
                else ok(state)
          
        (* PIPELINED *)
        open Akka.FSharp
        open Validation
        open Handlers
        open Akka.Actor
        open CryptoGames.Player.Types.Game

        let Execute (self : IActorRef, sender : IActorRef) (dependency : Dependency) (state : Option<State>, position : Position) =
            ok (state, position)
            >>= isInitialized
            >>= isOutOfBoard
            >>= isMoveNotAllowed
            >>= isAlive
            >>= handleMove
            >>= handleWin self
            >>= handleLoss self
            >>= saveStateHandler dependency
            |> eitherTee (fun (state, _) -> sender <! state.UserState) (fun mgs -> sender <! mgs)
            |> either (fun(s,_)->Some(s)) (fun(_)-> state)