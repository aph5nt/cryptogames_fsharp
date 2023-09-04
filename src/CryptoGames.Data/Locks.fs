namespace CryptoGames.Data

open FSharp.Data
open CryptoGames

module Locks = 
    module Sql =
        type DeleteCmd = SqlCommandProvider<"DELETE FROM [dbo].[Locks] WHERE UserName = @UserName AND Network = @Network AND GameId = @GameId", DataBase.Connection, ResultType.Records, SingleRow = true>
        type GetCmd = SqlCommandProvider<"SELECT * FROM [dbo].[Locks] WHERE UserName =  @UserName AND Network = @Network", DataBase.Connection, ResultType.Records, SingleRow = false>
        type InsertCmd = SqlCommandProvider<"""
            INSERT INTO dbo.Locks(GameId, Network, UserName, Amount)
            VALUES (@GameId, @Network, @UserName, @Amount)
            """, DataBase.Connection>
        
    let private mapLock (dbLock : Sql.GetCmd.Record) : DTO.Lock =
        {   Id = dbLock.Id
            GameId = dbLock.GameId
            Network = dbLock.Network |> Network.Parse
            UserName = dbLock.UserName
            Amount = dbLock.Amount }  

    let private getBy network userName =
        let result = Sql.GetCmd.Create().Execute(userName, network.ToString())
        result |> Seq.map mapLock |> Seq.toList
        
    let private insert network gameId userName amount =
        let cmd = Sql.InsertCmd.Create()
        cmd.Execute(gameId, network.ToString(), userName, amount) |> ignore
        
    let private delete network gameId userName =
        Sql.DeleteCmd.Create().Execute(userName, network.ToString(), gameId) |> ignore

    let Create() = 
        { DAO.LockDAO.GetBy = getBy
          DAO.LockDAO.DeleteBy = delete
          DAO.LockDAO.Insert = insert }
