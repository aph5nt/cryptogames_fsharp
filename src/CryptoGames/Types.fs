namespace CryptoGames

open System

[<AutoOpen>]
module CommonTypes =

    type UserName = string
    and Address = string
    and FromAddress = string
    and ToAddress = string
    and BankAddress = string
    and Amount = decimal
    and GameId = int64
    and PrivateKey = string
    and PublicKey = string
    and TransactionHash = string

    type Network = 
        | FREE 
        | BTC 
        | BTCTEST
        override this.ToString() = this |> toString
        static member Parse input = input |> fromString
         
    let Networks = [|FREE;BTCTEST; BTC|]

    let timeout = TimeSpan.FromSeconds(3.)

    type TranVerifyStatus = 
        | Pending 
        | Verified
        | Failed
        override this.ToString() = this |> toString
        static member Parse input =  input |> fromString
           
    type TranStatus = 
        | Pending
        | Processing
        | Succeed
        | Failed
        override this.ToString() = this |> toString
        static member Parse input = input |> fromString
 
    type ConfirmationStatus =
        | Awaiting
        | Confirmed
        | Timeout
        | Failed
        override this.ToString() = this |> toString
        static member Parse input = input |> fromString 
 
    //type GameType = 
    //    | Mines
    //    override this.ToString() = this |> toString
    //    static member Parse input = input |> fromString  
            
    //let Games = [|Mines|]
 
    module Services =
        
        type BankAddressMap = Map<Network, Address>
 
        [<RequireQualifiedAccess>]
        module Payment =
            open System.Collections.Generic

            type OnBalanceUpdated = {
                Network : Network
                UserName : string
                Address : string
                Amount : decimal 
            }
 
            //and Balance = {
            //    Balance : decimal
            //    Address : string
            //    Network : Network
            //} 
 
            and ExternalBalance = {
                Balance : decimal
                Address : string
                UserName : string
            }

            and Fee = { Amount : Amount }
            with
                static member ForNetwork network =
                    match network with
                    | BTCTEST -> { Amount = 0.0005m }
                    | BTC -> { Amount = 0.0005m }
                    | FREE -> { Amount = 0m }
                    | _ -> failwith "network not supported"
                
            //and InternalApi = {
            //    GetState      : Network -> UserName -> Option<Balance>
            //    TryCreateBalance : Network -> UserName -> Address -> Amount -> unit
            //    UpdateBalance : Network -> UserName -> Address -> Amount -> unit
            //    Transfer      : Network -> FromAddress -> ToAddress -> Amount -> unit  
            //    PublishState  : Network -> UserName -> unit
            //}

            and ExternalApi = {
                GetBalances   : BankAddress -> ExternalBalance[]
                GetBalance :  UserName -> Option<ExternalBalance>
                CreateAddress : UserName -> ExternalBalance
                DepositTransfer  : UserName -> Address -> BankAddress -> Amount -> Option<TransactionHash>
                WithdrawTransfer : BankAddress -> (ToAddress * Amount) list -> Option<TransactionHash>
            }

            and ExternalApiMap = Dictionary<Network, ExternalApi>

            type Api = {
                ExternalApis : ExternalApiMap
                //InternalApi : InternalApi
            }

            //type Dependency = {
            //    System : ActorSystem
            //    EventStream : Akka.Event.EventStream
            //}
             and Args = {
                Networks: Network[]
            }

            type Setup = {
                Run : Args -> Api
            }
        
        [<RequireQualifiedAccess>]
        module Bankroll = 
            
            type Dependency = {
                Api : Payment.Api
            }
            and Args = {
                Networks: Network[]
            }
            type Setup = {
                Run : Dependency -> Args -> BankAddressMap
            }

        //[<RequireQualifiedAccess>]
        //module UINotifier =
        //    type Task = {
        //        Run : ActorSystem -> unit
        //    }
        
        //[<RequireQualifiedAccess>]
        //module UINotfierProxy = 
        //    type Task = {
        //        Run : ActorSystem -> unit
        //    }
 
