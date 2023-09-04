namespace CryptoGames.Data

open System
open CryptoGames
open CryptoGames.Game.Mines.Types

[<RequireQualifiedAccess>]
module DTO =
    type Lock = {
        Id : int64
        GameId : GameId
        Network : Network
        UserName : UserName
        Amount : Amount
    }
    type Log = {
        Id : int64
        GameId : GameId
        ServerGuid : Guid
        ClientGuid : Guid
        Seed : int
        CustomData : string
        CreatedAt : DateTime
    }
    type Statistic = {
         Id : int64
         GameId : GameId
         Network : Network
         Type : GameType
         UserName : UserName
         CreatedAt : DateTime
         Size : string
         Turn : int
         Bet : Amount
         Win : Amount
         Loss : Amount
    }
    type BestPlayer = {
        TotalWin : decimal
        MaxWin : decimal
        UserName : UserName
    }
    type UserAuth = { Name : string; Secret : string }
    type FreeApiDeposit = {
        Address : Address
        UserName : UserName
        Balance : Amount
    }
    type Balance = {
        Id : int64
        Network : Network
        UserName : UserName
        Address : Address
        Amount : Amount
        UpdatedAt : DateTime
    }
    type Deposit = {
        Id : int64
        Network : Network
        UserName : UserName
        Address : Address
        PrivateKey : PrivateKey
        PublicKey : PublicKey
    }
    type Forward = {
        Id : int64
        Network : Network
        UserName : string
        Address : Address
        Status : ConfirmationStatus
        Amount : Amount
        CreatedAt : DateTime
        TransactionHash : string
    }
    type Game = {
        Id : int64
        Type : GameType
        UserName : UserName
        Network : Network
        Status : Status
        Data : string
        UpdatedAt : DateTime
        Size : string
    }
    type Withdraw = {
        Id : int64
        Network : Network
        UserName : UserName
        Status : TranStatus
        VerifyStatus : TranVerifyStatus
        ToAddress : Address
        Amount : Amount
        TransactionHash : string
        CreatedAt : DateTime
        UpdatedAt : DateTime
    }

[<RequireQualifiedAccess>]
module DAO =
    type GameDAO = {
       NewId : unit -> GameId
       Get : GameType -> Network -> UserName -> Option<DTO.Game>
       Delete : GameType -> Network -> UserName -> unit
       DeleteAll : unit -> unit
       Upsert : int64 -> GameType -> UserName -> Network -> Status -> string -> string -> unit
    }
    type GameSettingsConverter = {
        Serialize : State -> string
        Deserialize : string -> State
    }
    type LockDAO = {
        GetBy : Network -> UserName -> DTO.Lock list
        DeleteBy : Network -> GameId -> UserName -> unit
        Insert : Network -> GameId -> UserName -> Amount -> unit
    }
    type LogDAO = {
        GetByGameId : GameId -> Option<DTO.Log>
        GetByUserName : UserName -> int -> int -> DTO.Log list
        Insert : GameId -> Guid -> Guid -> int -> string -> unit
    }
    type StatisticDAO = {
        GetBestPlayers : Network -> DTO.BestPlayer list
        GetLastGames : Network -> DTO.Statistic list
        GetByUserNameTotalPages : Network -> UserName -> int
        GetByUserName : Network -> UserName -> int -> int -> DTO.Statistic list
        Insert : Network -> GameId -> GameType -> UserName -> int -> Amount -> Amount -> Amount -> unit
    }
    type UserAuthDAO = {
        Create : unit -> DTO.UserAuth
        DeleteAll : unit -> unit
        Exists : UserName -> bool
        Validate : UserName -> string -> bool
    }
    type FreeApiDepositDAO = {
        GetAll : BankAddress -> DTO.FreeApiDeposit list
        GetBy : UserName -> Option<DTO.FreeApiDeposit>
        Insert : Address -> UserName -> DTO.FreeApiDeposit
        Update : UserName -> Amount -> unit
        UpdateFixed : UserName -> unit
        Transfer : UserName -> ToAddress -> Amount -> unit
        DeleteAll : unit -> unit
    }
    type BalanceDAO = {
        Get : Network -> UserName -> Option<DTO.Balance>
        GetAll : UserName -> DTO.Balance list
        GetBankAll : unit -> DTO.Balance list
        GetMaps : unit -> Services.BankAddressMap
        GetNewMap : int64 -> Services.BankAddressMap
        Reset : unit -> unit
        Delete : Network -> UserName -> unit
        DeleteAll : unit -> unit
        Transfer : Network -> FromAddress -> ToAddress -> Amount -> unit
        Update : Network -> UserName -> Address -> Amount -> DTO.Balance
        Insert : Network -> UserName -> Address -> Amount -> unit
    }
    type DepositDAO = {
        Insert : Network -> UserName -> Address -> PrivateKey -> PublicKey -> unit
        GetAllExcept :  Network -> BankAddress -> DTO.Deposit list
        GetBy :  Network -> UserName -> Option<DTO.Deposit>
        GetByAddress :  Network -> FromAddress -> Option<DTO.Deposit>
        DeleteAll : unit -> unit
    }
    type WithdrawDAO = {
        Insert : Network -> UserName -> TranStatus -> TranVerifyStatus -> ToAddress -> Amount -> int64
        Update : int64 list -> TranStatus -> TransactionHash -> unit
        GetAll : unit -> DTO.Withdraw list
        GetByIds : Network -> int64 list -> DTO.Withdraw list
        GetByStatus : Network -> TranStatus -> DTO.Withdraw list
        CountBy  : Network -> TransactionHash -> int
        DeleteAll : unit -> unit
    }
    type ForwardDAO = {
        Insert : Network -> UserName -> ConfirmationStatus -> Address -> Amount -> TransactionHash -> unit
        Update : Network -> TransactionHash -> ConfirmationStatus -> unit
        Get : Network -> TransactionHash -> Option<DTO.Forward>
        DeleteAll : unit -> unit
    }
    type QuartzDAO = {
        JobExist : string -> Option<string>
        FiredTrigger : string -> string list
    }
    type Database = {
        Forwards : ForwardDAO
        Withdraws : WithdrawDAO
        Deposits : DepositDAO
        Balances : BalanceDAO
        Statistics : StatisticDAO
        Games : GameDAO
        Locks : LockDAO
        Logs : LogDAO
        UserAuths : UserAuthDAO
        FreeApiDeposits : FreeApiDepositDAO
        GameSettings : GameSettingsConverter
    }


 