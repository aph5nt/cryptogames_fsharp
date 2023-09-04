namespace CryptoGames

open CryptoGames.Data
open Chessie.ErrorHandling

module Withdraw =
    module Types =
        
        type Args = {
            Network : Network
            UserName : UserName
            ToAddress : ToAddress
            Amount : Amount
        }

    open Types
    module Validation =

        let checkNetwork (args : Args) =
            if args.Network = BTC || args.Network = BTCTEST then ok(args)
            else fail "network unsupported"

        let checkAddress (args : Args) =
            try
                let btcNetwork = getNetwork args.Network
                NBitcoin.BitcoinAddress.Create(args.ToAddress, btcNetwork) |> ignore
                ok(args)
            with
            | _ -> fail "to address is invalid"

        let checkAmount (args : Args) =
            let fee = Fee.ForNetwork args.Network
            if args.Amount > fee.Amount then ok(args)
            else fail "unable to withdraw the amount which is less than the network fee"

        let checkAvailiableBalance (dependency : Dependency) (args : Args) =
            let network = args.Network
            let userName = args.UserName

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
            let available = balance - locks
             
            if(available - args.Amount >= 0m) then 
                ok(args, available)
            else 
                fail ("no enough funds")
    
    open Validation
    module Handlers =

        let updateBalance (dependency : Dependency) (args : Args, available : decimal) =
            let newBalance = available - args.Amount
            dependency.InternalApi.UpdateBalance
            <| args.Network
            <| args.UserName
            <| args.ToAddress
            <| newBalance
            ok(args)

        let insertWithdraw (dependency : Dependency) (args : Args) =
            dependency.Database.Withdraws.Insert
            <| args.Network
            <| args.UserName
            <| TranStatus.Pending
            <| TranVerifyStatus.Pending
            <| args.ToAddress
            <| args.Amount
            |> ignore
            ok(args)

    (* PIPELINE *)
    open Handlers
    let Execute(dependency : Dependency) (args : Args) =
        ok(args)
        >>= checkNetwork
        >>= checkAddress
        >>= checkAmount
        >>= checkAvailiableBalance dependency
        >>= updateBalance dependency
        >>= insertWithdraw dependency
        |> either (fun (_, _) -> true) (fun _ -> false)
