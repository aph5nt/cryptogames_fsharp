namespace CryptoGames.Game.Mines

open Types
open CryptoGames.Player.Types
open Chessie.ErrorHandling

module TakeWinCommand = 
    module Validation = 
        let isInitialized(state : Option<State>) =
            match state with
            | None -> fail "The game has been not initialized"
            | _ -> ok(state.Value)

        let isAliveOrEscaped (state : State) =
            if state.UserState.Status = Alive || state.UserState.Status = Escaped then ok(state)
            else fail "isAliveOrEscaped"
            
        let madeAtLeastOneMove (state : State) =
            if state.UserState.Position.X >= 0 then ok(state)
            else fail "madeAtLeastOneMove"

    (* CUSTOM *)
    module Handlers =
        let handleTranferMoneyFromBankroll (args: Game.Args) (dependency : Game.Dependency) (state : State) =
            dependency.InternalApi.Transfer
            <| args.Network
            <| args.BankAddress.ToString()
            <| args.UserAddress.ToString()
            <| (state.UserState.Win - state.UserState.Bet)
            dependency.InternalApi.PublishState args.Network state.Settings.UserName
            ok(state)

        let handleReleaseLock (dependency : Game.Dependency) (state : State) =
            dependency.Database.Locks.DeleteBy
            <| state.Settings.Network
            <| state.Settings.Id 
            <| state.Settings.UserName

            ok(state)

    (* PIPELINED *)
    open Validation
    open Handlers
    
    let Execute (args: Game.Args) (dependency : Game.Dependency) (state : Option<State>) =
        ok (state)
        >>= isInitialized
        >>= isAliveOrEscaped
        >>= madeAtLeastOneMove
        >>= handleTranferMoneyFromBankroll args dependency
        >>= handleReleaseLock dependency
        >>= handleStatistics dependency
        |> ignore