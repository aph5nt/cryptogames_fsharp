namespace CryptoGames.Data

open System.Data.SqlClient
open FSharp.Data
open System
open System.Text

module UserAuths = 
    module Sql =
       type DeleteAll = SqlCommandProvider<"DELETE FROM dbo.UserAuth",  DataBase.Connection, ResultType.DataReader>
       type Sequence = SqlCommandProvider<"SELECT NEXT VALUE FOR dbo.UserNameSequence", DataBase.Connection, ResultType.Records, SingleRow = true>
       type CreateCmd = SqlCommandProvider<"INSERT INTO dbo.UserAuth (Name, Hash, LastActivity) VALUES (@Name, @Hash, getutcdate())",  DataBase.Connection, ResultType.DataReader>
       type ExistsCmd = SqlCommandProvider<"SELECT * FROM dbo.UserAuth WHERE Name = @Name",  DataBase.Connection, ResultType.Records, SingleRow = true>
       type ValidateCmd = SqlCommandProvider<"SELECT * FROM dbo.UserAuth WHERE Name = @Name",  DataBase.Connection, ResultType.Records, SingleRow = true>

    let private deleteAll() =
        Sql.DeleteAll.Create().Execute() |> ignore

    let private exists userName = 
        let data = Sql.ExistsCmd.Create().Execute(userName)
        match data with
        | Some(_) -> true
        | None -> false

    let private validate userName secret =
        let cmd = Sql.ValidateCmd.Create()
        let result = cmd.Execute(userName)
        match result with
        | None -> false
        | Some(user) ->
            let isValid =  CryptoGames.Security.SHA256.Equals(secret, user.Hash)
            let update = new SqlCommandProvider<"UPDATE dbo.UserAuth SET LastActivity = getutcdate() WHERE Name = @Name",  DataBase.Connection, ResultType.DataReader>()
            update.Execute(user.Name) |> ignore
            isValid

    let private create() =
        let seq = Sql.Sequence.Create().Execute().Value
        let name = CryptoGames.Security.Encoder.Conceal(seq)
        let pwd = System.Guid.NewGuid().ToString()
        let hash = CryptoGames.Security.SHA256.Encode(pwd)
        use cmd = Sql.CreateCmd.Create()
        cmd.Execute(name, hash) |> ignore
        { DTO.UserAuth.Name = name 
          DTO.UserAuth.Secret = pwd }

    let Create() = 
        { DAO.UserAuthDAO.Create = create
          DAO.UserAuthDAO.DeleteAll = deleteAll
          DAO.UserAuthDAO.Exists = exists
          DAO.UserAuthDAO.Validate = validate }   