namespace CryptoGames.Game.Mines

open System
open CryptoGames
 
module Types =

    (* MODEL *)
    type Field = 
        | Mined 
        | Safe 
        | Unknown
        override this.ToString() = this |> toString
        static member Parse input = input |> fromString
    and Board = Field [,]
    and Position = { X : int; Y : int }
    and Dimension  = { X : int; Y : int }
    and Status =
        | Alive 
        | Dead 
        | Escaped
        override this.ToString() = this |> toString
        static member Parse input = input |> fromString
    and Seed(client : Guid) =
        let server = Guid.NewGuid()
        let value = client.GetHashCode() + server.GetHashCode()
        member x.Client = client
        member x.Server = server
        member x.Value = value
     
    and GameSettings = {
        Id : int64
        Seed : Seed
        Bet : decimal
        Dimension : Dimension
        Type : GameType
        Network : Network
        UserName : string }
        
    and CreateState = GameSettings -> State
    and State = { 
        Settings : GameSettings
        GameState : GameState
        UserState : UserState }
            with 
                static member GenerateMultiplicators (xlen, ylen) =
                    match (xlen, ylen) with
                    | (3,2) -> [1.92m; 3.69m; 7.08m]
                    | (6,3) -> [1.44m;2.07m;2.99m;4.3m;6.19m;8.92m]
                    | (9,4) -> [1.28m; 1.64m; 2.1m; 2.68m; 3.44m; 4.4m; 5.63m; 7.21m; 9.22m]
                    | _ -> failwith "multiplicator not availiable"

                static member GetSize (board : Board) =
                    let xLen = Array2D.length1 board
                    let yLen = Array2D.length2 board
                    match (xLen, yLen) with
                    | (3,2) -> "3 x 2"
                    | (6,3) -> "6 x 3"
                    | (9,4) -> "9 x 4"
                    | _ -> failwith "size not availiable"

                static member GetWin (state : State) =
                    let xLen = Array2D.length1 state.GameState.Board
                    let yLen = Array2D.length2 state.GameState.Board
                    let multiplicators = State.GenerateMultiplicators (xLen, yLen)
                    let win = state.UserState.Bet * multiplicators.[state.UserState.Position.X]
                    win
 
                static member Create : CreateState =
                    fun settings ->
                        let xMaxPosition = settings.Dimension.X - 1
                        let board = Array2D.create settings.Dimension.X settings.Dimension.Y Safe
                        let userBoard = Array2D.create settings.Dimension.X settings.Dimension.Y Unknown
                        let rnd = new System.Random(settings.Seed.Value)
                        for i in [0 .. xMaxPosition] do
                            board.[i, rnd.Next(0, settings.Dimension.Y)] <- Mined
                        { Settings = settings
                          GameState = { GameState.Board = board; Size = State.GetSize board }
                          UserState = { Board = userBoard
                                        Position = { X = -1; Y = 0; }
                                        Status = Alive 
                                        Bet = settings.Bet
                                        Win = 0m
                                        Loss = 0m } }

                static member CreateMocked : CreateState =
                    fun settings ->
                        let xMaxPosition = settings.Dimension.X - 1
                        let board = Array2D.create settings.Dimension.X settings.Dimension.Y Safe
                        let userBoard = Array2D.create settings.Dimension.X settings.Dimension.Y Unknown
                        for i in [0 .. xMaxPosition] do board.[i, 0] <- Mined
                        { Settings = settings
                          GameState = { GameState.Board = board; Size = State.GetSize board }
                          UserState = { Board = userBoard
                                        Position = { X = -1; Y = 0; }
                                        Status = Alive 
                                        Bet = settings.Bet
                                        Win = 0m
                                        Loss = 0m } }

    and GameState = { Board : Board; Size : string }
    and UserState = {
        Board : Board
        Position : Position
        Status  : Status
        Bet : decimal
        Win : decimal
        Loss : decimal }
 
    (* HELPERS *)
    let xMaxPosition state = Array2D.length1 state.GameState.Board - 1
    let yMaxPosition state = Array2D.length2 state.GameState.Board - 1
 
    let BoardToString ( board : Board ) =
        let xLen = Array2D.length1 board
        let yLen = Array2D.length2 board
        let sb = new System.Text.StringBuilder()
        for x in [0 .. (xLen - 1)] do
            for y in [0 .. (yLen - 1)] do
                sb.AppendLine((sprintf "[%d,%d] = %s\n" <| x <| y <| (board.[x,y] |> toString) )) |> ignore
        sb.ToString()

