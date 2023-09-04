namespace CryptoGames.Game.Mines

open Types
open CryptoGames.Player.Types
open Chessie.ErrorHandling

module TakeLossCommand = 
    module Validation =

        let isInitialized(state : Option<State>) =
            match state with
            | None -> fail "The game has been not initialized"
            | _ -> ok(state.Value)

        let isDead (state : State) =
            if state.UserState.Status = Dead then ok(state)
            else fail "isDead"
            
        let madeAtLeastOneMove (state : State) =
            if state.UserState.Position.X >= 0 then ok(state)
            else fail "madeAtLeastOneMove"  

    (* CUSTOM *)
    module Handlers =
           
        let handleTranferMoneyFromUserAddress (args: Game.Args) (dependency : Game.Dependency) (state : State) =
            dependency.InternalApi.Transfer
            <| args.Network
            <| args.UserAddress.ToString()
            <| args.BankAddress.ToString()
            <| (state.Settings.Bet)
            dependency.InternalApi.PublishState args.Network state.Settings.UserName
            ok(state)
            
        let handleReleaseLock (dependency : Game.Dependency) (state : State) =
            dependency.Database.Locks.DeleteBy
            <| state.Settings.Network
            <| state.Settings.Id 
            <| state.Settings.UserName
            ok(state)

    (* PILELINED *)
    open Validation
    open Handlers
 
    let Execute (args: Game.Args) (dependency : Game.Dependency) (state : Option<State>) =
        ok (state)
        >>= isInitialized
        >>= isDead
        >>= madeAtLeastOneMove
        >>= handleTranferMoneyFromUserAddress args dependency
        >>= handleReleaseLock dependency
        >>= handleStatistics dependency
        |> ignore // TODO: log the failure path

