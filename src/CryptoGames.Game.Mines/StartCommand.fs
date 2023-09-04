namespace CryptoGames.Game.Mines

open Chessie.ErrorHandling
open Types
open CryptoGames.Data
open Akka.FSharp

module StartCommand =
    module Validation = 
        open CryptoGames.Core
        open CryptoGames.Player.Types

        let checkAvailiableBankBalance (dependency : Game.Dependency) (settings : GameSettings) =
            let bankBalance = dependency.Database.Balances.GetBankAll() |> Seq.find(fun x -> x.Network = settings.Network)
            let amount = bankBalance.Amount
            let maxMultiplicator = Types.State.GenerateMultiplicators(settings.Dimension.X, settings.Dimension.Y) |> List.max
            if (settings.Bet * maxMultiplicator < amount) then ok(settings)
            else fail (sprintf "no enough bank funds for %A" settings.Network)

        let checkAvailiableBalance (dependency : Game.Dependency) (settings : GameSettings) =
            let network = settings.Network
            let userName = settings.UserName

            let balanceQuery = 
                async {
                   let result = dependency.Database.Balances.Get network userName
                   match result with
                   | None -> return 0m
                   | Some(record) -> return record.Amount
                } |> Async.StartAsTask

            let locksQuery = 
                async {
                    return dependency.Database.Locks.GetBy network userName |> List.sumBy(fun r -> r.Amount)
                } |> Async.StartAsTask

            let balance = balanceQuery |> Async.AwaitTask |> Async.RunSynchronously
            let locks = locksQuery     |> Async.AwaitTask |> Async.RunSynchronously
            let bet = settings.Bet

            let total = balance - locks - bet
            if(total >= 0m) then ok(settings)
            else fail ("no enough funds")

        let checkIfCanStart (state : Option<State>)  (settings : GameSettings)  =
            match state with
            | None -> ok(settings)
            | Some(stateR) -> 
                match stateR.UserState.Status with
                | Alive -> fail "can not start game as its running"
                | _ -> ok(settings)

    (* CUSTOM *)
    module Handlers =
        open CryptoGames.Player.Types
         
        let createStateHandler (dependency : Game.Dependency) settings = 
            ok(dependency.Create settings)

        let putLock (dependency : Game.Dependency) (state : State) =
            dependency.Database.Locks.Insert
            <| state.Settings.Network
            <| state.Settings.Id
            <| state.Settings.UserName
            <| state.Settings.Bet

            ok(state)

        let insertLog (dependency : Game.Dependency)  (state : State ) =
            let settings = state.Settings
            dependency.Database.Logs.Insert
            <| settings.Id
            <| settings.Seed.Server
            <| settings.Seed.Client
            <| settings.Seed.Value
            <| (BoardToString <| state.GameState.Board)
            ok(state)

    (* PIPELINE *)
    open Validation
    open Handlers
    open Akka.Actor
    open CryptoGames.Player.Types
 
    let Execute (self : IActorRef, sender : IActorRef) (previousState : Option<State>) (settings : GameSettings) (dependency : Dependency) =
        ok (settings)
        >>= checkIfCanStart previousState
        >>= checkAvailiableBalance dependency
        >>= checkAvailiableBankBalance dependency
        >>= createStateHandler dependency
        >>= saveStateHandler dependency
        >>= putLock dependency
        >>= insertLog dependency
        |> eitherTee (fun (state, _) -> sender <! state.UserState) (fun mgs -> sender <! mgs)
        |> either (fun(s,_)->Some(s)) (fun(_)-> previousState)
