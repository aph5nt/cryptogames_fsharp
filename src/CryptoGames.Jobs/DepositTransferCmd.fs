namespace CryptoGames

open CryptoGames.Data
open Chessie.ErrorHandling
open QBitNinja.Client
open System.Collections.Generic
open System.Threading
open System.Linq

module DepositTransferCmd =
    module Types = 
        type Dependency = {
            Database : DAO.Database
            QBitNinjaClient   : QBitNinjaClient
        }
        type Args = {
            Network : Network
            UserName : UserName
            Address : Address
            BankAddress : Address
            Amount : Amount
            Fee : Services.Payment.Fee
        }

    open Types
    
    let getCoins (dependency, args) =
        let result = dependency.Database.Deposits.GetByAddress args.Network args.Address 
        match result with 
        | None -> fail "deposit not found" 
        | Some(deposit) -> 
            let secret = new NBitcoin.BitcoinSecret(deposit.PrivateKey, getNetwork args.Network);
            let key = NBitcoin.Key.Parse(secret.ToWif(), getNetwork args.Network);
            let balance = 
                dependency.QBitNinjaClient.GetBalance(key, true)  
                |> Async.AwaitTask
                |> Async.RunSynchronously
            let coins = balance.Operations.SelectMany(fun f -> f.ReceivedCoins.AsEnumerable()).ToList()
            ok(dependency, args, coins, key)
   
    let buildTransaction (dependency, args : Args, coins : List<NBitcoin.ICoin>, key : NBitcoin.Key) =
        try
            let destinationAddr = new NBitcoin.BitcoinPubKeyAddress(args.BankAddress)

            let builder = new NBitcoin.TransactionBuilder()
            builder.AddCoins(coins) |> ignore
            builder.AddKeys(key) |> ignore
            builder.Send(destinationAddr, NBitcoin.Money.Coins(args.Amount)) |> ignore
            builder.SetChange(key) |> ignore
            builder.SendFees(NBitcoin.Money.Coins(args.Fee.Amount))|> ignore
                        
            let transaction = builder.BuildTransaction(true)

            if builder.Verify(transaction) = false then
                fail "failed to verify the transaction"
            else
                ok(dependency, args, transaction)
        with
        | :? NBitcoin.NotEnoughFundsException ->
            fail "no founds"
        | _ -> fail "uncauth exception"
    
    let canPublishTransaction (dependency, args, transaction : NBitcoin.Transaction) =
        let data = dependency.Database.Forwards.Get args.Network (transaction.GetHash().ToString())
        match data with
        | None -> ok (dependency, args, transaction)
        | Some(forward) -> 
            match forward.Status with
            | Failed -> ok (dependency, args, transaction)
            | _ -> fail "transaction with given tx hash has been sent already"

    let sendTransaction (dependency, args, transaction : NBitcoin.Transaction) =
        let transactionHash = transaction.GetHash()
        try
            dependency.Database.Forwards.Insert 
                <| args.Network 
                <| args.UserName
                <| Awaiting 
                <| args.Address 
                <| args.Amount
                <| (transactionHash.ToString())

            use node = NBitcoin.Protocol.Node.ConnectToLocal(getNetwork args.Network)
            node.VersionHandshake()
            node.SendMessage(new NBitcoin.Protocol.InvPayload(NBitcoin.Protocol.InventoryType.MSG_TX, transactionHash));
            Thread.Sleep(1)
            node.SendMessage(new NBitcoin.Protocol.TxPayload(transaction))
            Thread.Sleep(1)

            ok(transactionHash.ToString())
        with
        | exn ->     
                dependency.Database.Forwards.Update
                <| args.Network
                <| transactionHash.ToString()
                <| ConfirmationStatus.Failed

                Serilog.Log.Error(exn.Message)   
                fail exn.Message     

    let Execute (dependency, args) =
        ok((dependency, args))
        >>= getCoins
        >>= buildTransaction
        >>= canPublishTransaction
        >>= sendTransaction
        |> either 
            (fun(transactionHash, _) -> Some(transactionHash))
            (fun(msgs) -> 
                let errorMsgs = String.concat "\n" msgs
                Serilog.Log.Error("Failed send transaction @{errorMsgs}", errorMsgs )
                None)