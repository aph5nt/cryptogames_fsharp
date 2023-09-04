namespace CryptoGames.Data

open FSharp.Data
 
module Logs =
    module Sql =
       type InsertCmd = SqlCommandProvider<"INSERT INTO [dbo].[Logs] (GameId, ServerGuid, ClientGuid, Seed, CustomData, CreatedAt) VALUES (@GameId, @ServerGuid, @ClientGuid, @Seed, @CustomData, GETUTCDATE())", DataBase.Connection, ResultType.Records> 
       type GetByGameIdCmd = SqlCommandProvider<"SELECT TOP(1) * FROM dbo.Logs WHERE GameId = @gameId",  DataBase.Connection, ResultType = ResultType.Records, SingleRow = true>
       type GetByUserNameCmd = SqlCommandProvider<"""

        DECLARE @PageNumberP int
        set @PageNumberP = @PageNumber

        DECLARE @RowspPageP int
        set @RowspPageP = @RowspPage

        SELECT l.* FROM dbo.Logs l
        JOIN dbo.Games g
        ON g.Id = l.GameId
        WHERE g.UserName = @UserName
        ORDER BY l.Id DESC 
        OFFSET ((@PageNumberP - 1) * @RowspPageP) ROWS FETCH NEXT @RowspPageP ROWS ONLY

        """, DataBase.Connection, ResultType.Records>
    
    let private insert gameId serverGuid clientGuid seed customData =
       Sql.InsertCmd.Create().Execute(gameId, serverGuid, clientGuid, seed, customData) |> ignore
    
    let private getByUserName userName pageNumber rowsPage =
        let mapLog (dbLog : Sql.GetByUserNameCmd.Record) =
            {   DTO.Log.Id = dbLog.Id 
                DTO.Log.GameId = dbLog.GameId
                DTO.Log.Seed = dbLog.Seed
                DTO.Log.ServerGuid = dbLog.ServerGuid
                DTO.Log.ClientGuid = dbLog.ClientGuid
                DTO.Log.CustomData = dbLog.CustomData
                DTO.Log.CreatedAt =  dbLog.CreatedAt }

        let r = Sql.GetByUserNameCmd.Create().Execute(pageNumber, rowsPage, userName)
        r |> Seq.map mapLog |> Seq.toList

    let private getByGameId gameId =
        let r = Sql.GetByGameIdCmd.Create().Execute(gameId)
        r |> Option.map (fun d -> { DTO.Log.Id = d.Id 
                                    DTO.Log.GameId = d.GameId
                                    DTO.Log.Seed = d.Seed
                                    DTO.Log.ServerGuid = d.ServerGuid
                                    DTO.Log.ClientGuid = d.ClientGuid
                                    DTO.Log.CustomData = d.CustomData
                                    DTO.Log.CreatedAt = d.CreatedAt })

    let Create() =
        { DAO.LogDAO.GetByGameId = getByGameId
          DAO.LogDAO.GetByUserName = getByUserName 
          DAO.LogDAO.Insert = insert }       

 
 
 