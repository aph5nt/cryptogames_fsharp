namespace CryptoGames

open CryptoGames.Data
open NBitcoin
open NBitcoin.Protocol
open QBitNinja.Client
open System.Collections.Generic
open System.Threading
open System.Linq
open Chessie.ErrorHandling
 
module WithdrawTransferCmd =
 
    module Types =
        type Dependency = {
            Database : DAO.Database
            QBitNinjaClient   : QBitNinjaClient
        }
        type Args = {
            Network : CryptoGames.Types.Network
            BankAddress : BankAddress
            Target : Withdraw list
        }

    open Types
    
    let getCoins(dependency, args) =
        let result = dependency.Database.Deposits.GetByAddress args.Network args.BankAddress 
        match result with 
        | None -> fail "bank deposit not found" 
        | Some(deposit) -> 
            let secret = new BitcoinSecret(deposit.PrivateKey, getNetwork args.Network);
            let key = Key.Parse(secret.ToWif(), getNetwork args.Network);
            let balance = 
                dependency.QBitNinjaClient.GetBalance(key, true)  
                |> Async.AwaitTask
                |> Async.RunSynchronously
            let coins = balance.Operations.SelectMany(fun f -> f.ReceivedCoins.AsEnumerable()).ToList()
            ok(dependency, args, coins, key)

    let buildTransaction (dependency, args : Args, coins : List<ICoin>, key : Key) =
        try
            let builder = new TransactionBuilder()
            builder.AddCoins(coins) |> ignore
            builder.AddKeys(key) |> ignore

            let fee = CryptoGames.Types.Services.Payment.Fee.ForNetwork args.Network

            args.Target 
            |> List.iter(fun (withdraw) -> 
                builder.Send(new BitcoinPubKeyAddress(withdraw.ToAddress), NBitcoin.Money.Coins(withdraw.Amount - fee.Amount)) |> ignore)
           
            builder.SetChange(key) |> ignore

            let feeAmount = (decimal)args.Target.Length * fee.Amount
            builder.SendFees(NBitcoin.Money.Coins(feeAmount))|> ignore
                         
            let transaction = builder.BuildTransaction(true)

            if builder.Verify(transaction) = false then
                fail "failed to verify the transaction"
            else
                ok(dependency, args, transaction)
        with
        | :? NBitcoin.NotEnoughFundsException ->
            fail "no founds"
        | _ -> fail "uncauth exception" 
    ()

    let canPublishTransaction (dependency : Dependency, args : Args, transaction : Transaction) =
        if args.Target |> List.exists(fun i -> i.Status <> Pending && i.VerifyStatus <> TranVerifyStatus.Pending) then 
            fail "one of the withdraw has invalid status"
        else 
            ok (dependency, args, transaction) 

    let sendTransaction (dependency, args, transaction : Transaction) =
        let transactionHash = transaction.GetHash()
        try
            dependency.Database.Withdraws.Update 
            <| (args.Target |> List.map(fun i -> i.Id))
            <| TranStatus.Processing
            <| transactionHash.ToString()
  
            use node = NBitcoin.Protocol.Node.ConnectToLocal(getNetwork args.Network)
            node.VersionHandshake()
            node.SendMessage(new InvPayload(InventoryType.MSG_TX, transactionHash));
            Thread.Sleep(1)
            node.SendMessage(new NBitcoin.Protocol.TxPayload(transaction))
            Thread.Sleep(1)

            ok(transactionHash.ToString())
        with
        | exn ->     
            dependency.Database.Withdraws.Update 
            <| (args.Target |> List.map(fun i -> i.Id))
            <| TranStatus.Failed
            <| transactionHash.ToString()
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