namespace CryptoGames.Data

open System
open System.IO
open FSharp.Data
open CryptoGames
open CryptoGames.Game.Mines.Types
open System.Runtime.Serialization.Formatters.Binary

module Games =
    module Sql =
        type GetCmd = SqlCommandProvider<"""
            SELECT TOP(1) * FROM [dbo].[Games]
            WHERE Type = @Type
            AND UserName = @UserName
            AND Network = @Network
            AND Status = 'Alive'
            ORDER BY UpdatedAt DESC
            """, DataBase.Connection, ResultType.Records, SingleRow = true>
    
        type DeleteCmd = SqlCommandProvider<"""
            DELETE FROM [dbo].[Games]
            WHERE UserName = @UserName
            AND Network = @Network
            AND Type = @Type
            """, DataBase.Connection, ResultType.Records, SingleRow = true>
        type UpsertCmd = SqlCommandProvider<""" 
        	declare @IdP bigint set @IdP = @Id
	        declare @TypeP nvarchar(50) set @TypeP = @Type
	        declare @UserNameP nvarchar(50) set @UserNameP = @UserName
	        declare @NetworkP nvarchar(50) set @NetworkP = @Network
	        declare @StatusP nvarchar(50) set @StatusP = @Status
	        declare @DataP nvarchar(max) set @DataP = @Data
	        declare @SizeP nvarchar(10) set @SizeP = @Size
	        UPDATE dbo.Games SET
                    Status = @StatusP,
                    Data = @DataP
                WHERE Id = @IdP AND
                    [Status] = 'Alive'  

	        IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.Games(Id,Type, UserName, Network, Status, Data, Size)
                VALUES (@IdP, @TypeP, @UserNameP, @NetworkP, @StatusP, @DataP, @SizeP)
            END
	        SELECT TOP(1) * FROM dbo.Games where Id = SCOPE_IDENTITY()
        """,  DataBase.Connection, ResultType.Records, SingleRow = true>
        type DeleteAllCmd = SqlCommandProvider<"DELETE FROM dbo.Games",  DataBase.Connection, ResultType.DataReader>
        type Sequence = SqlCommandProvider<"SELECT NEXT VALUE FOR dbo.GameSequence", DataBase.Connection, ResultType.Records, SingleRow = true>
        type DbProviderd = SqlProgrammabilityProvider<DataBase.Connection>

    let private mapGame (dbGame : Sql.GetCmd.Record) : CryptoGames.Game.Mines.Types.Game =
        { Id = dbGame.Id
          Type = dbGame.Type |> GameType.Parse
          UserName = dbGame.UserName
          Network = dbGame.Network |> Network.Parse
          Status = dbGame.Status |> Status.Parse
          Data = dbGame.Data
          UpdatedAt = dbGame.UpdatedAt
          Size = dbGame.Size }
    
    let private get type' network userName  =
        use cmd = Sql.GetCmd.Create()
        cmd.Execute(type'.ToString(), userName, network.ToString())
        |> Option.map mapGame

    let private delete type' network userName =
        let cmd = Sql.DeleteCmd.Create()
        cmd.Execute(userName.ToString(), network.ToString(), type'.ToString()) |> ignore

    let private deleteAll() =
        Sql.DeleteAllCmd.Create().Execute() |> ignore

    let private upsert id type' userName network status data size =
        let result = Sql.UpsertCmd.Create().Execute(id, type'.ToString(), userName.ToString(), network.ToString(), status.ToString(), data, size)
        ()
    
    let private newId() =
        Sql.Sequence.Create().Execute().Value
 
    let Create() =
        { DAO.GameDAO.Get = get 
          DAO.GameDAO.Delete = delete
          DAO.GameDAO.DeleteAll = deleteAll
          DAO.GameDAO.Upsert = upsert
          DAO.GameDAO.NewId = newId }

module GameSettings =
 
    /// if the latest record in the game table will have status = alive then the record will be returned
    /// if the status will be != Alive then loadState will return None
    let private serialize data =
        use stream = new MemoryStream()
        let formatter = new BinaryFormatter()
        formatter.Serialize(stream, data)
        stream.Flush()
        stream.Position <- 0L
        Convert.ToBase64String(stream.ToArray())

    let private deserialize (data : string) =
        use stream = new MemoryStream(Convert.FromBase64String(data))
        let formatter = new BinaryFormatter()
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        let state = formatter.Deserialize(stream) :?> State
        state

    let Create() =
        { DAO.GameSettingsConverter.Deserialize = deserialize 
          DAO.GameSettingsConverter.Serialize = serialize }