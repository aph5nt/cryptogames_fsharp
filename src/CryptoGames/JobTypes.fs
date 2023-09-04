namespace CryptoGames
 
open CryptoGames
open CryptoGames.Data
open QBitNinja.Client

[<AutoOpen>]
module JobTypes =

    //[<RequireQualifiedAccess>]
    //module DividendPayer =
    //    type Job = {
    //        Run : unit -> unit
    //    }

    [<RequireQualifiedAccess>]
    module Withdrawer = 

        //type Dependency = {
        //    Database : DAO.Database
        //    QBitNinjaClient : QBitNinjaClient
        //    ExternalApiMap : Services.Payment.ExternalApiMap
        //}

        type Args = {
            BankAddress : BankAddress
            Network : Network
        }

    [<RequireQualifiedAccess>]
    module DepositForwarder =

        type Dependency = {
            ExternalApiMap : Services.Payment.ExternalApiMap
            Database : DAO.Database
        }

        type Args = {
            Network : Network
            BankAddress : BankAddress
            Interval : float
        } 
        with 
            static member intervalMsFor network =
                match network with
                | BTC     ->  1000. * 60. * 10.
                | BTCTEST ->  1000. * 60. * 1.
                | FREE    ->  1000. * 3.

        type Job = {
            Run : Quartz.IScheduler ->  Dependency -> Args -> unit
        }


module Dependency = 

    type T = {
        Database : Data.DAO.Database
        QBitNinjaClient : QBitNinjaClient
    }