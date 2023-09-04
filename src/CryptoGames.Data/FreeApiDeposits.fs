namespace CryptoGames.Data

open FSharp.Data
open CryptoGames

module FreeApiDeposits =
    module Sql =
        type DB = SqlProgrammabilityProvider<DataBase.Connection> 
        type GetAllBalances = SqlCommandProvider<"SELECT * FROM freeApi.Deposits WHERE Balance > 0 AND Address <> @BankAddress",  DataBase.Connection, ResultType.Records, SingleRow = false>
        type GetBy = SqlCommandProvider<"SELECT TOP(1) * FROM freeApi.Deposits WHERE UserName = @UserName",  DataBase.Connection, ResultType.Records, SingleRow = true>
        type Insert = SqlCommandProvider<"INSERT INTO freeApi.Deposits(Address, UserName, Balance) VALUES (@Address, @UserName, 0); SELECT * FROM freeApi.Deposits where UserName = @UserName",  DataBase.Connection, ResultType.Records, SingleRow = true>
        type Transfer = SqlCommandProvider<"""
        BEGIN TRANSACTION
            DECLARE  @a decimal(18,8)
            set @a = @Amount
            UPDATE freeApi.Deposits SET 
                Balance = (Balance - @a)
            WHERE UserName = @UserName;
            
            UPDATE freeApi.Deposits SET 
                Balance = (Balance + @a)
            WHERE Address = @ToAddress
         COMMIT TRANSACTION""",  DataBase.Connection>
        type UpdateBalanceByLabelCmd = SqlCommandProvider<"UPDATE freeApi.Deposits SET Balance = @Amount WHERE UserName = @UserName",  DataBase.Connection>
        type DeleteWalletsCmd = SqlCommandProvider<"DELETE FROM freeApi.Deposits",  DataBase.Connection>
        type UpdateBalnaceFixed = SqlCommandProvider<"UPDATE freeApi.Deposits SET Balance = 1 WHERE UserName = @userName",  DataBase.Connection>
 
    let private update label amount =
        Sql.UpdateBalanceByLabelCmd.Create().Execute(amount, label) |> ignore

    let private transfer userName toAddress amount =
        Sql.Transfer.Create().Execute(amount, userName, toAddress) |> ignore

    let private insert address userName =
        let result = Sql.Insert.Create().Execute(address, userName) 
        let map = result |> Option.map(fun data ->
                                { DTO.FreeApiDeposit.Address = data.Address
                                  DTO.FreeApiDeposit.UserName = data.UserName
                                  DTO.FreeApiDeposit.Balance = data.Balance })
        map.Value

    let private deleteAll() = 
        Sql.DeleteWalletsCmd.Create().Execute() |> ignore
    
    let private updateFixed userName = 
        Sql.UpdateBalnaceFixed.Create().Execute(userName) |> ignore
 
    let private getBy userName =
        let result = Sql.GetBy.Create().Execute(userName)
        result |> Option.map(fun data ->  { DTO.FreeApiDeposit.Address = data.Address
                                            DTO.FreeApiDeposit.UserName = data.UserName
                                            DTO.FreeApiDeposit.Balance = data.Balance
                                           })
    let private getAll bankAddress =
        let result = Sql.GetAllBalances.Create().Execute(bankAddress)
        result |> Seq.map (fun data -> { DTO.FreeApiDeposit.Address = data.Address
                                         DTO.FreeApiDeposit.UserName = data.UserName
                                         DTO.FreeApiDeposit.Balance = data.Balance
                                       }) |> Seq.toList

    let Create() =
        { DAO.FreeApiDepositDAO.GetAll = getAll
          DAO.FreeApiDepositDAO.GetBy = getBy
          DAO.FreeApiDepositDAO.DeleteAll = deleteAll
          DAO.FreeApiDepositDAO.Insert = insert
          DAO.FreeApiDepositDAO.Transfer = transfer
          DAO.FreeApiDepositDAO.UpdateFixed = updateFixed
          DAO.FreeApiDepositDAO.Update = update }
 