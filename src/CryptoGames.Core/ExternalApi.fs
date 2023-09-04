namespace CryptoGames
 
open CryptoGames.Data
open System
 
module ExternalApi =
    module FreeApi =
        module Internal = 
            let getBalances (database : DAO.Database) bankAddress =
                let result = database.FreeApiDeposits.GetAll bankAddress
                result
                |> Seq.map(fun record -> { Services.Payment.ExternalBalance.Balance = record.Balance
                                           Services.Payment.ExternalBalance.Address = record.Address
                                           Services.Payment.ExternalBalance.UserName = record.UserName })
                |> Seq.toArray

            let getBalance (database : DAO.Database) userName =
                let result = database.FreeApiDeposits.GetBy userName
                match result with
                | Some(record) -> Some({ Services.Payment.ExternalBalance.Balance = record.Balance
                                         Services.Payment.ExternalBalance.Address = record.Address
                                         Services.Payment.ExternalBalance.UserName = record.UserName })
                | None -> None

            let createAddress (database : DAO.Database) userName =
                let data = database.FreeApiDeposits.Insert  (Guid.NewGuid().ToString()) userName
                { Services.Payment.ExternalBalance.Balance = data.Balance
                  Services.Payment.ExternalBalance.Address = data.Address
                  Services.Payment.ExternalBalance.UserName = data.UserName }

            let withdrawTransfer _ _ _ = 
                Some("transaction hash should be returned")

            let depositTransfer (database : DAO.Database) userName address bankAddress amount = 
                database.FreeApiDeposits.Transfer userName bankAddress amount
                Some("")
 
        let Create (database : DAO.Database) =
            { Services.Payment.ExternalApi.CreateAddress = Internal.createAddress database
              Services.Payment.ExternalApi.DepositTransfer = Internal.depositTransfer database
              Services.Payment.ExternalApi.WithdrawTransfer = Internal.withdrawTransfer database
              Services.Payment.ExternalApi.GetBalance =  Internal.getBalance database
              Services.Payment.ExternalApi.GetBalances = Internal.getBalances database}
    
    module NBitcoinApi = 
        open QBitNinja.Client
        
        let Create(network : Network,  database : DAO.Database) = 
 
            let createAddress userName =
                let key = new NBitcoin.Key()
                let secret = new NBitcoin.BitcoinSecret(key, getNetwork network)

                let privateKey = secret.ToString()
                let publicKey = secret.PubKey.ToString()
                let address = secret.PubKey.GetAddress(getNetwork network).ToString()

                database.Deposits.Insert network userName address privateKey publicKey

                { Services.Payment.ExternalBalance.UserName = userName
                  Services.Payment.ExternalBalance.Balance = 0m
                  Services.Payment.ExternalBalance.Address = address }
             
            let getBalance userName =
                let result = database.Deposits.GetBy network userName 
                match result with 
                | None -> None
                | Some(deposit) ->
                    let client = new QBitNinjaClient(getNetwork network)
                    let balance = 
                        client.GetBalance(NBitcoin.BitcoinAddress.Create(deposit.Address, getNetwork network), true) 
                        |> Async.AwaitTask
                        |> Async.RunSynchronously
                    
                    let spendableBalance = balance.Operations |> Seq.filter(fun f -> f.Confirmations >= 6) |> Seq.sumBy(fun f -> f.Amount)
                
                    Some({ Services.Payment.ExternalBalance.Balance = spendableBalance.ToDecimal(NBitcoin.MoneyUnit.BTC)
                           Services.Payment.ExternalBalance.Address = deposit.Address
                           Services.Payment.ExternalBalance.UserName = deposit.UserName })

            let getBalances (bankAddresses : BankAddress) =
                try
                    let client = new QBitNinja.Client.QBitNinjaClient(getNetwork network)
                    let result = database.Deposits.GetAllExcept network bankAddresses

                    let mapResult (deposit : DTO.Deposit) =
                        let balance = 
                            client.GetBalance(NBitcoin.BitcoinAddress.Create(deposit.Address, getNetwork network), true) 
                            |> Async.AwaitTask
                            |> Async.RunSynchronously

                        let spendableBalance = balance.Operations |> Seq.filter(fun f -> f.Confirmations >= 6) |> Seq.sumBy(fun f -> f.Amount)

                        { Services.Payment.ExternalBalance.Balance = spendableBalance.ToDecimal(NBitcoin.MoneyUnit.BTC)
                          Services.Payment.ExternalBalance.Address = deposit.Address
                          Services.Payment.ExternalBalance.UserName = deposit.UserName }

                    result |> Seq.map(mapResult) |> Seq.toArray
                with
                | exn ->
                    Serilog.Log.Error(exn, "getBalances")
                    failwith exn.Message
 
//            let withdrawTransfer fromAddress withdraws = 
//                let dependency = {
//                    WithdrawTransferCmd.Types.Dependency.Database = CryptoGames.Data.Database.Create()
//                    WithdrawTransferCmd.Types.Dependency.QBitNinjaClient = new QBitNinja.Client.QBitNinjaClient(getNetwork network)
//                }
//                let args = {
//                    WithdrawTransferCmd.Types.Args.BankAddress = fromAddress
//                    WithdrawTransferCmd.Types.Args.Network = network
//                    WithdrawTransferCmd.Types.Args.Target = withdraws
//                } 
//                Some("transaction hash should be returned")

            let withdrawTransfer fromAddress withdraws = 
                Some("")
            let depositTransfer userName address bankAddress amount = 
                let dependency = {
                    Dependency.Database = CryptoGames.Data.Database.Create()
                    Dependency.QBitNinjaClient = new QBitNinja.Client.QBitNinjaClient(getNetwork network)
                }
                let args = {
                    Args.UserName = userName
                    Args.Amount = amount
                    Args.Fee = Fee.ForNetwork network
                    Args.Address = address
                    Args.BankAddress = bankAddress
                    Args.Network = network
                }
                
                DepositTransferCmd.Execute (dependency, args)

            { Services.Payment.ExternalApi.CreateAddress = createAddress
              Services.Payment.ExternalApi.DepositTransfer = depositTransfer
              Services.Payment.ExternalApi.WithdrawTransfer = withdrawTransfer
              Services.Payment.ExternalApi.GetBalance =  getBalance
              Services.Payment.ExternalApi.GetBalances = getBalances }
 
open ExternalApi

module Payment =
    open CryptoGames
    
    let run dependency args =
        let internalApi =  InternalApi.Create(dependency.EventStream)
       
        let database = Data.Database.Create()
        let externalApiMap = new Services.Payment.ExternalApiMap()
        for network in args.Networks do
            if network = FREE then
                externalApiMap.Add(network, FreeApi.Create(database))
            else 
                externalApiMap.Add(network, NBitcoinApi.Create(network,  database))
            
        { Services.Payment.ExternalApis = externalApiMap
          Services.Payment.InternalApi = internalApi database}

    let Create() =
        { Services.Payment.Run = run }