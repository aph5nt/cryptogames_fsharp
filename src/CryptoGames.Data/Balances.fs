namespace CryptoGames.Data

open FSharp.Data
open CryptoGames

module Balances =
    
    module Sql =
        type DbProvider = SqlProgrammabilityProvider<DataBase.Connection>
        type TransferCommand = SqlCommandProvider<"""
            declare @networkP nvarchar(50) set @networkP = @network
            declare @fromAddressP nvarchar(250) set @fromAddressP = @fromAddress
            declare @toAddressP nvarchar(250) set @toAddressP = @toAddress
            declare @amountP decimal(18,8) set @amountP = @amount
            BEGIN TRANSACTION
            -- from
            UPDATE dbo.Balances SET Amount = Amount - @amountP WHERE Address = @fromAddressP AND Network =  @networkP
            UPDATE dbo.Balances SET Amount = Amount + @amountP WHERE Address = @toAddressP   AND Network =  @networkP
            COMMIT TRANSACTION
            """, DataBase.Connection, ResultType.DataReader>

        type UpdateBalanceCommand = SqlCommandProvider<"""
            declare @AmountP decimal(18,8)
            set @AmountP = @Amount

            declare @NetworkP nvarchar(50)
            set @NetworkP = @Network

            declare @UserNameP nvarchar(50)
            set @UserNameP = @UserName

            declare @AddressP nvarchar(250)
            set @AddressP = @Address

            UPDATE dbo.Balances SET
                Amount = @AmountP
            WHERE Network = @NetworkP and UserName = @UserNameP and [Address] = @AddressP
            IF @@ROWCOUNT = 0
                INSERT INTO [dbo].[Balances] ([Network],[UserName],[Address],[Amount]) VALUES (@NetworkP, @UserNameP, @AddressP, @AmountP)
            SELECT TOP(1) * FROM dbo.Balances WHERE UserName = @UserNameP AND Network = @NetworkP
         """,DataBase.Connection, ResultType.Records, SingleRow = true>
        type Insert = SqlCommandProvider<"""
            declare @UserNameP nvarchar(50) set @UserNameP = @UserName
            declare @NetworkP nvarchar(50) set @NetworkP = @Network
            IF NOT EXISTS (SELECT TOP(1) Id FROM dbo.Balances WHERE UserName = @UserNameP AND Network = @NetworkP)
            BEGIN
                INSERT INTO [dbo].[Balances] ([Network],[UserName],[Address],[Amount]) VALUES (@NetworkP, @UserNameP, @Address, @Amount) 
            END
            """, DataBase.Connection>
        type BalanceChangedCommand = SqlCommandProvider<"""
            declare @NetworkP nvarchar(50)
            set @NetworkP = @Network

            declare @UserNameP nvarchar(50)
            set @UserNameP = @UserName

            UPDATE dbo.Balances SET
                Amount = Amount + @AmountChange
            WHERE Network = @NetworkP and UserName = @UserNameP
            SELECT TOP(1) * FROM dbo.Balances WHERE UserName = @UserNameP AND Network = @NetworkP
        """,DataBase.Connection, ResultType.Records, SingleRow = true>
        type GetCommand = SqlCommandProvider<"SELECT * FROM [dbo].[Balances] WHERE UserName =  @UserName AND Network = @Network", DataBase.Connection, ResultType.Records, SingleRow = true>
        type GetAllCommand = SqlCommandProvider<"SELECT * FROM [dbo].[Balances] WHERE UserName = @UserName", DataBase.Connection, ResultType.Records, SingleRow = false>
        type DeleteCommand = SqlCommandProvider<"DELETE FROM [dbo].[Balances] WHERE UserName = @UserName AND Network = @Network", DataBase.Connection, ResultType.Records, SingleRow = true>
        type ResetAllCommand = SqlCommandProvider<"UPDATE dbo.Balances SET Amount = 0; DELETE FROM dbo.Balances WHERE Network = 'FREE'", DataBase.Connection, ResultType.Records, SingleRow = true>
        type DeleteAllCommand = SqlCommandProvider<"DELETE FROM dbo.Balances", DataBase.Connection, ResultType.Records, SingleRow = true>
        type GetBankBalancesCmd = SqlCommandProvider<"""
            SELECT * FROM dbo.Balances WHERE UserName IN ('Mines')
            """, DataBase.Connection>
        type GetAllLabelMapsCmd = SqlCommandProvider<"""
              SELECT Id, Address, [UserName], Network FROM dbo.Balances Order by Id ASC
        """, DataBase.Connection>
        type GetNewLabelMapsCmd = SqlCommandProvider<"""
             SELECT Id, Address, [UserName], Network FROM dbo.Balances WHERE Id > @Id Order by Id ASC
        """, DataBase.Connection>

    let private transfer network fromAddress toAddress amount =
        let networkStr = network.ToString()
        Sql.TransferCommand.Create().Execute(networkStr, fromAddress, toAddress, amount) |> ignore
 
    let private update network userName address amount = 
         let networkStr = network.ToString()
         use cmd = Sql.UpdateBalanceCommand.Create()
         let map (data : Sql.UpdateBalanceCommand.Record) : DTO.Balance =
            { Id = data.Id
              Network = data.Network |> Network.Parse
              UserName = data.UserName
              Address = data.Address
              Amount = data.Amount
              UpdatedAt = data.UpdatedAt }   
         let result = cmd.Execute(amount, networkStr, userName,address) |> (Option.map map)
         result.Value
 
    let private insert network userName address amount =
         Sql.Insert.Create().Execute(userName, network.ToString(), address, amount) |> ignore
    
    let private get network userName = 
        let result = Sql.GetCommand.Create().Execute(userName, network.ToString())
        let map (data : Sql.GetCommand.Record) : DTO.Balance =
            { Id = data.Id
              Network = data.Network |> Network.Parse
              UserName = data.UserName
              Address = data.Address
              Amount = data.Amount
              UpdatedAt = data.UpdatedAt }   
        result |> (Option.map map)
    
    let private getAll userName = 
        let map (data : Sql.GetAllCommand.Record) : DTO.Balance =
            { Id = data.Id
              Network = data.Network |> Network.Parse
              UserName = data.UserName
              Address = data.Address
              Amount = data.Amount
              UpdatedAt = data.UpdatedAt }   
        Sql.GetAllCommand.Create().Execute(userName)
        |> (Seq.map map) |> Seq.toList

    let private delete network userName  = 
        Sql.DeleteCommand.Create().Execute(userName, network.ToString()) |> ignore
    
    let private reset() = 
        Sql.ResetAllCommand.Create().Execute() |> ignore
    
    let private deleteAll() = 
        Sql.DeleteAllCommand.Create().Execute() |> ignore

    let private getBankBalances() = 
        let map (data : Sql.GetBankBalancesCmd.Record) : DTO.Balance =
            { Id = data.Id
              Network = data.Network |> Network.Parse
              UserName = data.UserName
              Address = data.Address
              Amount = data.Amount
              UpdatedAt = data.UpdatedAt }   
        Sql.GetBankBalancesCmd.Create().Execute()  |> Seq.map map |> Seq.toList

    let private getAllLabelMaps() = 
       Sql.GetAllLabelMapsCmd.Create().Execute()
       |> Seq.map(fun data -> (data.Network |> Network.Parse, data.Address ))
       |> Map.ofSeq

    let private getNewLabelMaps id = 
       Sql.GetNewLabelMapsCmd.Create().Execute(id)  
       |> Seq.map(fun data -> (data.Network |> Network.Parse, data.Address ))
       |> Map.ofSeq 

    let Create() =
        { 
          DAO.BalanceDAO.Get = get
          DAO.BalanceDAO.GetAll = getAll
          DAO.BalanceDAO.GetBankAll = getBankBalances
          DAO.BalanceDAO.GetMaps = getAllLabelMaps
          DAO.BalanceDAO.GetNewMap = getNewLabelMaps
          DAO.BalanceDAO.Reset = reset
          DAO.BalanceDAO.Delete = delete
          DAO.BalanceDAO.DeleteAll = deleteAll
          DAO.BalanceDAO.Transfer = transfer
          DAO.BalanceDAO.Update = update
          DAO.BalanceDAO.Insert = insert
        }            
