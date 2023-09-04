namespace CryptoGames

open CryptoGames.Data


(*
    THIS SHOULD BE REMOVED !! there is no sense in duplication of the BalanceDAO !!

*)

module InternalApi =
    open Services
    
    module Internal =
        let getState (database : DAO.Database) (network : Network) (userName : UserName) =
            let data = database.Balances.Get network userName 
            match data with
            | None -> None
            | Some(record) ->
                Some({ Services.Payment.Balance.Balance = record.Amount
                       Payment.Balance.Balance.Address = record.Address
                       Payment.Balance.Balance.Network = record.Network })

        let update (database : DAO.Database) (eventBus : Akka.Event.EventStream) (network : Network) (userName : UserName) (address : Address) (amount :  Amount) =
            let result = 
                database.Balances.Update 
                    <| network
                    <| userName
                    <| address
                    <| amount  

            eventBus.Publish { OnBalanceUpdated.UserName = userName; OnBalanceUpdated.Network = network;  OnBalanceUpdated.Amount = result.Amount;  OnBalanceUpdated.Address = result.Address }    
          
        let tryCreateBalance (database : DAO.Database) (network : Network) (userName : UserName) (address : Address) (amount : Amount) =
            database.Balances.Insert network userName address amount

        let transfer (database : DAO.Database) (network : Network) (fromAddress : FromAddress) (toAddress : ToAddress) (amount : Amount) =
              database.Balances.Transfer network fromAddress toAddress amount

        let publishState (database : DAO.Database) (eventBus : Akka.Event.EventStream) (network : Network)  (userName : UserName) =
            let data = getState database network userName 
            match data with
            | None -> ()
            | Some(record) ->
                eventBus.Publish { OnBalanceUpdated.UserName = userName
                                   OnBalanceUpdated.Network = network
                                   OnBalanceUpdated.Amount = record.Balance
                                   OnBalanceUpdated.Address = record.Address }    

    let Create (eventBus : Akka.Event.EventStream) (database : DAO.Database) =
        { InternalApi.GetState = Internal.getState database
          InternalApi.TryCreateBalance = Internal.tryCreateBalance database
          InternalApi.UpdateBalance = Internal.update database eventBus 
          InternalApi.Transfer = Internal.transfer database
          InternalApi.PublishState = Internal.publishState database eventBus }
        
 