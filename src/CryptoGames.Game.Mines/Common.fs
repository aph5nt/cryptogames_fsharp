[<AutoOpen>]
module Common

open Chessie.ErrorHandling
open CryptoGames.Data
open CryptoGames.Game.Mines.Types
open CryptoGames.Player.Types

let saveStateHandler (dependency : Game.Dependency) (state : State) =
    dependency.Database.Games.Upsert
    <| state.Settings.Id
    <| GameType.Mines
    <| state.Settings.UserName
    <| state.Settings.Network
    <| state.UserState.Status
    <| dependency.Database.GameSettings.Serialize state
    <| state.GameState.Size
    |> ignore

    ok(state)

let handleStatistics (dependency : Game.Dependency) (state : State) =
    dependency.Database.Statistics.Insert
    <| state.Settings.Network
    <| state.Settings.Id
    <| state.Settings.Type
    <| state.Settings.UserName
    <| (state.UserState.Position.X + 1)
    <| state.Settings.Bet
    <| state.UserState.Win
    <| state.UserState.Loss
    ok(state)

 