namespace CryptoGames.Data

open FSharp.Data
 open CryptoGames
module Withdraws =
    module Sql =
        type InsertCmd = SqlCommandProvider<"""
            INSERT INTO dbo.Withdraws(Network, UserName, Status, VerifyStatus, ToAddress, Amount, TransactionHash)
            VALUES(@Network, @UserName, @Status, @VerifyStatus, @ToAddress, @Amount, '')
            SELECT Id FROM dbo.Withdraws WHERE Id = (SELECT SCOPE_IDENTITY())
            """, DataBase.Connection, ResultType.Records, SingleRow = true>

        type UpdateCmd = SqlCommandProvider<"""
        declare @idList IdList = @ids
        UPDATE dbo.Withdraws SET
            Status = @Status,
            TransactionHash = @TransactionHash,
            UpdatedAt = getutcdate()
            WHERE Id IN (SELECT Id FROM @idList)
        """, DataBase.Connection>
        type DeleteAllCmd = SqlCommandProvider<"""DELETE FROM dbo.Withdraws""", DataBase.Connection>
        type GetAll = SqlCommandProvider<""" SELECT * FROM [dbo].[Withdraws] """, DataBase.Connection, ResultType.Records>
        type GetByStatus  = SqlCommandProvider<"""  
            SELECT TOP(10) * FROM [dbo].[Withdraws] WHERE Network = @Network AND [Status] = @Status ORDER BY Id DESC
        """, DataBase.Connection, ResultType.Records>
        type GetByIds = SqlCommandProvider<""" 
            declare @idList IdList = @ids
            SELECT * FROM [dbo].[Withdraws] WHERE Network = @Network AND Id IN (SELECT Id FROM @idList) """, DataBase.Connection, ResultType.Records>
        type CountBy = SqlCommandProvider<""" SELECT count(Id) FROM [dbo].[Withdraws] WHERE Network = @Network AND TransactionHash = @TransactionHash """, DataBase.Connection, ResultType.Records, SingleRow = true>
       
    let private deleteAll() =
        Sql.DeleteAllCmd.Create().Execute() |> ignore

    let private update (ids : int64 list) status transactionHash =
        Sql.UpdateCmd.Create().Execute(ids |> List.map(fun id -> new Sql.UpdateCmd.IdList(id)), status.ToString(), transactionHash) |> ignore
 
    let private insert network userName status verifyStatus toAddress (amount : Amount) =
        Sql.InsertCmd.Create().Execute(network.ToString(), userName, status.ToString(), verifyStatus.ToString(), toAddress.ToString(), amount).Value
   
    let private countBy network transactionHash =
        Sql.CountBy.Create().Execute(network.ToString(), transactionHash).Value.Value
    
    let private getByStatus network status =
        let inline mapResult (result : Sql.GetByStatus.Record) =
            { Withdraw.Id = result.Id
              Withdraw.Network = result.Network |> Network.Parse
              Withdraw.UserName = result.UserName
              Withdraw.Status = result.Status |> TranStatus.Parse
              Withdraw.VerifyStatus = result.VerifyStatus |> TranVerifyStatus.Parse
              Withdraw.ToAddress = result.ToAddress
              Withdraw.Amount = result.Amount
              Withdraw.TransactionHash = result.TransactionHash
              Withdraw.CreatedAt = result.CreatedAt
              Withdraw.UpdatedAt = result.UpdatedAt }
        Sql.GetByStatus.Create().Execute(network.ToString(), status.ToString()) |> Seq.map mapResult |> Seq.toList

    let private getByIds network ids =
        let inline mapResult (result : Sql.GetByIds.Record) =
            { Withdraw.Id = result.Id
              Withdraw.Network = result.Network |> Network.Parse
              Withdraw.UserName = result.UserName
              Withdraw.Status = result.Status |> TranStatus.Parse
              Withdraw.VerifyStatus = result.VerifyStatus |> TranVerifyStatus.Parse
              Withdraw.ToAddress = result.ToAddress
              Withdraw.Amount = result.Amount
              Withdraw.TransactionHash = result.TransactionHash
              Withdraw.CreatedAt = result.CreatedAt
              Withdraw.UpdatedAt = result.UpdatedAt }
        Sql.GetByIds.Create().Execute(ids |> List.map(fun id -> new Sql.GetByIds.IdList(id)), network.ToString()) |> Seq.map mapResult |> Seq.toList

    let private getAll() =
        let inline mapResult (result : Sql.GetAll.Record) =
            { Withdraw.Id = result.Id
              Withdraw.Network = result.Network |> Network.Parse
              Withdraw.UserName = result.UserName
              Withdraw.Status = result.Status |> TranStatus.Parse
              Withdraw.VerifyStatus = result.VerifyStatus |> TranVerifyStatus.Parse
              Withdraw.ToAddress = result.ToAddress
              Withdraw.Amount = result.Amount
              Withdraw.TransactionHash = result.TransactionHash
              Withdraw.CreatedAt = result.CreatedAt
              Withdraw.UpdatedAt = result.UpdatedAt }

        Sql.GetAll.Create().Execute() |> Seq.map mapResult |> Seq.toList
        
    let Create() =
        { DAO.WithdrawDAO.Insert = insert
          DAO.WithdrawDAO.Update = update 
          DAO.WithdrawDAO.GetByIds = getByIds
          DAO.WithdrawDAO.GetByStatus = getByStatus
          DAO.WithdrawDAO.GetAll = getAll
          DAO.WithdrawDAO.CountBy  = countBy
          DAO.WithdrawDAO.DeleteAll = deleteAll }
 