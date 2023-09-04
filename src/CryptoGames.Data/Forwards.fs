namespace CryptoGames.Data

open FSharp.Data
open CryptoGames

module Forwards =
    module Sql =
        type InsertCmd = SqlCommandProvider<"""
            INSERT INTO dbo.Forwards(UserName, Network, Status, Address, Amount, TransactionHash)
            VALUES(@UserName, @Network, @Status, @Address, @Amount, @TransactionHash)
            """, DataBase.Connection, ResultType.Records, SingleRow = true>

        type UpdateCmd = SqlCommandProvider<"""
        UPDATE dbo.Forwards SET
            Status = @Status
            WHERE TransactionHash = @TransactionHash AND Network = @Network
        """, DataBase.Connection>
        type DeleteAllCmd = SqlCommandProvider<"""DELETE FROM dbo.Forwards""", DataBase.Connection>
        type Get = SqlCommandProvider<""" SELECT TOP(1) * FROM [dbo].[Forwards] WHERE Network = @Network AND TransactionHash = @TransactionHash""", DataBase.Connection, ResultType.Records, SingleRow = true>
        
    let private deleteAll() =
        Sql.DeleteAllCmd.Create().Execute() |> ignore

    let private update network transactionHash status =
        Sql.UpdateCmd.Create().Execute(status.ToString(), transactionHash, network.ToString()) |> ignore
 
    let private insert network userName status address amount transactionHash =
        Sql.InsertCmd.Create().Execute(userName, network.ToString(), status.ToString(), address, amount, transactionHash) |> ignore
    
    let private get network transactionHash =
        Sql.Get.Create().Execute(network.ToString(), transactionHash) 
        |> Option.map (fun r -> { DTO.Forward.Id = r.Id
                                  DTO.Forward.UserName = r.UserName
                                  DTO.Forward.Network = r.Network |> Network.Parse
                                  DTO.Forward.Amount = r.Amount
                                  DTO.Forward.Address  = r.Address
                                  DTO.Forward.Status = r.Status |> ConfirmationStatus.Parse
                                  DTO.Forward.CreatedAt = r.CreatedAt
                                  DTO.Forward.TransactionHash = r.TransactionHash } )

    let Create() =
        { DAO.ForwardDAO.Insert = insert
          DAO.ForwardDAO.Update = update 
          DAO.ForwardDAO.Get = get
          DAO.ForwardDAO.DeleteAll = deleteAll }
 