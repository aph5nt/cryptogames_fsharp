namespace CryptoGames.Core

open System
open Akka.Actor
open Akka.FSharp
open Akka.FSharp.Actors  
open FSharp.Data
open Chessie.ErrorHandling
open CryptoGames.Types
open CryptoGames.Data
open CryptoGames.Data.Types
open CryptoGames.Types.Services.Payment

module BlockIoApi =
    open CryptoGames

    type Command = 
        | CreateAddress       of network : Network * label : string
        | GetAddressBalance   of network : Network * label : string
        | GetAddressBalances  of network : Network
        | Send                of network : Network * localTxIds : int64 list * fromAddress : string list * toAddress : string list * amount : decimal list
        | GetFee              of network : Network * toAddress  : string list * amount : decimal list

    module ApiWrapper =
       
        let baseUrl endpoint = sprintf "https://block.io/api/v2/%s" endpoint
        
        type CreateNewAddressJson = FSharp.Data.JsonProvider<"""
            {
              "status" : "success",
              "data" : {
                "network" : "BTCTEST",
                "user_id" : 24,
                "address" : "2N1N3cFnfxcqHFm63bjZ5bWKGsh3rjQpihf",
                "label" : "moje"
              }
            }
            """>

        type GetAddressBalance = FSharp.Data.JsonProvider<"""
            {
              "status" : "success",
              "data" : {
                "network" : "BTCTEST",
                "available_balance" : "0.00000000",
                "pending_received_balance" : "0.00000000",
                "balances" : [
                  {
                    "user_id" : 24,
                    "label" : "moje",
                    "address" : "2N1N3cFnfxcqHFm63bjZ5bWKGsh3rjQpihf",
                    "available_balance" : "0.00000000",
                    "pending_received_balance" : "0.00000000"
                  }
                ]
              }
            }
            """>

        type EstimateNetworkFee = FSharp.Data.JsonProvider<"""
                {
          "status" : "success",
          "data" : {
            "network" : "BTCTEST",
            "estimated_network_fee" : "0.00020000"
          }
        }""">

        type WithdrawFromAddresses = FSharp.Data.JsonProvider<"""
            {
              "status" : "success",
              "data" : {
                "network" : "BTCTEST",
                "txid" : "01ca5593fc96de3760878074a6e9d1d30fc5f10d4248e24d36d240183b2ead41",
                "amount_withdrawn" : "0.00120000",
                "amount_sent" : "0.00100000",
                "network_fee" : "0.00020000",
                "blockio_fee" : "0.00000000"
              }
            }""">

        type GetMyAddresses = FSharp.Data.JsonProvider<"""
        {
          "status" : "success",
          "data" : {
            "network" : "BTCTEST",
            "addresses" : [
              {
                "user_id" : 0,
                "address" : "2NFgK7cKzjyfXaXwu7WbkAtTS5CSougJqY7",
                "label" : "default",
                "available_balance" : "0.00000000",
                "pending_received_balance" : "0.00000000"
              },
              {
                "user_id" : 1,
                "address" : "2N24jPDvWZQRVcs52tuWtoPnWcvWtKzhZs5",
                "label" : "second",
                "available_balance" : "0.00000000",
                "pending_received_balance" : "0.00000000"
              }
            ]
          }
        }
        """>

        let createAddress network label =
            async {
                let request = baseUrl "get_new_address"
                let query = [
                    ("api_key", ApiKey network);
                    ("label", label)
                ]
                
                let! response = Http.AsyncRequest(request, httpMethod = "POST", query = query)

                match response.StatusCode with
                | code when code >= 200 && code < 300 -> 
                    match response.Body with
                    | Text data ->
                        let parsed = CreateNewAddressJson.Parse(data).Data
                        return Ok({ ExternalBalance.Address = parsed.Address; ExternalBalance.Label = parsed.Label; ExternalBalance.Balance = 0m }, [])
                        | _ -> return Bad(["unsuported body format"])
                | code when code = 429 -> return Bad(["limit exceded"])
                | _ -> return Bad(["unsupported response"])
            }

        let getAddressBalance network label =
            async {
                let request = baseUrl "get_address_balance"
                let query = [
                    ("api_key", ApiKey network);
                    ("labels", label)
                ]

                let! response = Http.AsyncRequest(request, httpMethod = "POST", query = query)

                match response.StatusCode with
                | code when code >= 200 && code < 300 -> 
                    match response.Body with
                    | Text data ->
                        let parsed = GetAddressBalance.Parse(data).Data
                        let balance = parsed.Balances.[0]
                        return Ok({ ExternalBalance.Balance = balance.AvailableBalance; ExternalBalance.Address = balance.Address; ExternalBalance.Label = balance.Label }, [])
                        | _ -> return Bad(["unsuported body format"])
                | code when code = 429 -> return Bad(["limit exceded"])
                | _ -> return Bad(["unsupported response"])
            }

        let getMyAddresses network =
            async {
                let request = baseUrl "get_my_addresses"
                let query = [
                    ("api_key", ApiKey network);
                ]
                
                let response = Http.Request(request, httpMethod = "POST", query = query)
                match response.StatusCode with
                | code when code >= 200 && code < 300 -> 
                    match response.Body with
                    | Text data ->
                        let parsed = GetMyAddresses.Parse(data).Data.Addresses
                        let result = 
                            parsed 
                            |> Array.map(fun balance -> { ExternalBalance.Address = balance.Address; ExternalBalance.Label = balance.Label; ExternalBalance.Balance = balance.AvailableBalance })
                        return Ok(result, [])
                    | _ -> return Bad([ "unsuported body format" ])
                | code when code = 429 -> return Bad([ "limit exceded" ])
                | _ -> return Bad(["unsupported response"])
            }
            
        let getAddressBalanceByAddress network (addresses : string list) = 
            async {
                let request = baseUrl "get_address_balance"
                let query = [
                    ("api_key", ApiKey network);
                    ("addresses", addresses |> String.concat ",")
                ]
                
                    let! response = Http.AsyncRequest(request, httpMethod = "POST", query = query)

                match response.StatusCode with
                | code when code >= 200 && code < 300 -> 
                    match response.Body with
                    | Text data ->
                        let parsed = GetAddressBalance.Parse(data).Data
                        let result = parsed.Balances |> Array.map(fun balance -> { ExternalBalance.Balance = balance.AvailableBalance; ExternalBalance.Address = balance.Address; ExternalBalance.Label = balance.Label })
                        return Ok(result, [])
                        | _ -> return Bad(["unsuported body format"])
                | code when code = 429 -> return Bad(["limit exceded"])
                | _ -> return Bad(["unsupported response"])
            }
        let estimateNetworkFee network toAddresses (amounts : decimal list) =
            async {
                
                LocalSettings.SetGlobalizationSettings()
                
                let request = baseUrl "get_network_fee_estimate"
                let query = [
                    ("api_key", ApiKey network);
                    ("amounts", amounts |> List.map(fun x -> x.ToString()) |> String.concat(",") );
                    ("to_addresses", toAddresses |> String.concat(","))
                ]
                
                try
                    let! response = Http.AsyncRequest(request, httpMethod = "POST", query = query)

                    match response.StatusCode with
                    | code when code >= 200 && code < 300 -> 
                        match response.Body with
                        | Text data ->
                            let parsed = EstimateNetworkFee.Parse(data).Data
                            let estimate = parsed.EstimatedNetworkFee
                            return Ok({ Fee = estimate }, [])
                        | _ -> return Bad(["unsuported body format"])
                    | code when code = 429 -> return Bad(["limit exceded"])
                    | _ -> return Bad(["unsupported response"])
                  with exn ->
                    return Bad([exn.Message])
            }


        let withdrawFromAddresses network txids fromAddresses toAddresses (amounts : decimal list) =
            async {

                LocalSettings.SetGlobalizationSettings()

                let request = baseUrl "withdraw_from_addresses"
                let query = [
                    ("api_key", ApiKey network);
                    ("pin", Settings.BlockApiPin);
                    ("priority","low");
                    ("amounts", amounts |> List.map(fun x -> x.ToString()) |> String.concat(","));
                    ("from_addresses", fromAddresses |> String.concat(","));
                    ("to_addresses", toAddresses |> String.concat(","))
                ]
                try
                    let! response = Http.AsyncRequest(request, httpMethod = "POST", query = query)

                    match response.StatusCode with
                    | code when code >= 200 && code < 300 -> 
                        match response.Body with
                        | Text _ ->
                            return Ok(txids, [])
                        | _ -> 
                            Serilog.Log.Error("unsuported body format")
                            return Bad(txids)
                    | code when code = 429 -> 
                        Serilog.Log.Error("limit exceded")
                        return Bad(txids)
                    | _ -> 
                        Serilog.Log.Error("unsupported response")
                        return Bad(txids)
                with 
                | exn ->
                    Serilog.Log.Error(exn, "error occured")
                    return Bad(txids)
            }

    
    module Poller =

        let Run system (api : IActorRef, transactionRepository : TransactionRepository) =
            let name = "poller"
            let actorRef =
                spawn system name (fun m ->
                    let rec loop() = actor {
                        let! message = m.Receive()
                        match box message with
                        | :? string as s -> 
                            if s = "dequeue" then

                                for network in Networks do

                                    let transactions = transactionRepository.GetToProcess(network) |> Seq.toList
                                    if transactions.Length > 0 then
                                        let fromBankTransactions = transactions |> List.filter(fun t -> t.Type = FromBank)
                                        let fromAddresses = fromBankTransactions |> List.map(fun t -> t.FromAddress)
                                        let toAddresses = fromBankTransactions |> List.map(fun t -> t.ToAddress)
                                        let amounts = fromBankTransactions |> List.map(fun t -> t.Amount)
                                        let txIds = fromBankTransactions |> List.map(fun t -> t.Id)

                                        txIds |> List.iter(fun txid -> transactionRepository.UpdateTransaction txid TranStatus.Procesing)

                                        if txIds.Length > 0 then 
                                            
                                            api.Tell(Command.Send(network, txIds, fromAddresses, toAddresses, amounts))

                                        let fromPlayerstransactions = transactions |> List.filter(fun t -> t.Type = ToBank)
                                        fromPlayerstransactions |> List.iter(fun tran ->
                                             transactionRepository.UpdateTransaction tran.Id TranStatus.Procesing
                                             let cmd = Command.Send(network, [tran.Id], [tran.FromAddress], [tran.ToAddress], [tran.Amount])
                                             api <! cmd
                                    )
                        | :? Result<int64 list, int64> as result -> 
                            match result with
                            | Ok(txids,_) ->
                                txids |> List.iter(fun txid -> transactionRepository.UpdateTransaction txid TranStatus.Succeed)
                            | Bad(txids) ->
                                txids |> List.iter(fun txid -> transactionRepository.UpdateTransaction txid TranStatus.Failed)

                        | msg -> 
                            Serilog.Log.Error("unhalted message " + msg.ToString())
                        return! loop()
                    }
                    loop())
               
            
            schedule {
                return ((1000,1000), fun() -> actorRef <! "dequeue")
            } |> ignore
            

    let Create (system : IActorRefFactory) =

        let name = "blockio-api"
        let apiCallLimit = 5

        let actorRef =
            spawn system name (fun mailbox ->
                let rec loop apiCalls = actor {
                        
                        let left = (int)(1000 - DateTime.UtcNow.Millisecond)
                        if apiCalls <= apiCallLimit then
                            
                            let! message = mailbox.Receive()
                            match message with

                                | GetFee(network, toAddress, amounts) ->
                                    async {
                                        let! estimate = ApiWrapper.estimateNetworkFee network toAddress amounts
                                        return estimate
                                    } |!> mailbox.Context.Sender

                                    return! loop (apiCalls + 1)

                                | Send(network, txids, fromAddress, toAddress, amounts) ->
                                    async {
                                        let! withdraw = ApiWrapper.withdrawFromAddresses network txids fromAddress toAddress amounts
                                        return withdraw
                                    } |!> mailbox.Context.Sender

                                    return! loop (apiCalls + 1)

                                | GetAddressBalance(network, label) ->

                                    async {
                                        let! balance = ApiWrapper.getAddressBalance network label
                                        return balance
                                    } |!> mailbox.Context.Sender

                                    return! loop (apiCalls + 1)

                                 | GetAddressBalances(network) ->

                                    async {
                                        let! balances = ApiWrapper.getMyAddresses network
                                        return balances
                                    } |!> mailbox.Context.Sender

                                    return! loop (apiCalls + 1)

                                | CreateAddress(network, label) -> 
                                    async {
                                        let! address = ApiWrapper.createAddress network label
                                        return address
                                    } |!> mailbox.Context.Sender

                                    return! loop (apiCalls + 1)
                            
                        else 
                           Async.Sleep(left) |> Async.RunSynchronously
                           return! loop 0
                }

                loop 0)
             
        Poller.Run system (actorRef, (TransactionRepository.Create()))

        actorRef