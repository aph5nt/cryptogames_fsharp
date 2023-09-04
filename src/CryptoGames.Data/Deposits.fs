namespace CryptoGames.Data

open FSharp.Data
open CryptoGames
open CryptoGames.Data

module Deposits =
    module Sql =
        type Insert =  SqlCommandProvider<"""
            INSERT INTO dbo.Deposits(Network, UserName, Address, PublicKey, PrivateKey)
            VALUES(@Network, @UserName, @Address, @PublicKey, @PrivateKey)
            """, DataBase.Connection>
        type GetAll =  SqlCommandProvider<""" SELECT * FROM dbo.Deposits WHERE Network = @Network """, DataBase.Connection, ResultType.Records>
        type GetBy  =  SqlCommandProvider<""" SELECT * FROM dbo.Deposits WHERE Network = @Network AND UserName = @UserName """, DataBase.Connection, ResultType.Records, SingleRow = true>
        type GetByAddress  =  SqlCommandProvider<""" SELECT * FROM dbo.Deposits WHERE Network = @Network AND Address = @Address """, DataBase.Connection, ResultType.Records, SingleRow = true>
        type DeleteAll = SqlCommandProvider<""" DELETE FROM dbo.Deposits """, DataBase.Connection>
    let private insert network userName address primaryKey publicKey =
        Sql.Insert.Create().Execute(network.ToString(), userName, address, publicKey, primaryKey) |> ignore

    let private getBy network userName =
        Sql.GetBy.Create().Execute(network.ToString(), userName)
        |> Option.map(fun r -> {    DTO.Deposit.Id = r.Id
                                    DTO.Deposit.Network = r.Network |> Network.Parse
                                    DTO.Deposit.UserName = r.UserName
                                    DTO.Deposit.Address = r.Address
                                    DTO.Deposit.PrivateKey = r.PrivateKey
                                    DTO.Deposit.PublicKey = r.PublicKey } )

    let private getAllExcept network bankAddress =
        Sql.GetAll.Create().Execute(network.ToString()) 
        |> Seq.filter(fun f -> f.Address <> bankAddress)
        |> Seq.map(fun r -> {   DTO.Deposit.Id = r.Id
                                DTO.Deposit.Network = r.Network |> Network.Parse
                                DTO.Deposit.UserName = r.UserName
                                DTO.Deposit.Address = r.Address
                                DTO.Deposit.PrivateKey = r.PrivateKey
                                DTO.Deposit.PublicKey = r.PublicKey } )
        |> Seq.toList

    let private getByAddress network address =
        Sql.GetByAddress.Create().Execute(network.ToString(), address)
        |> Option.map(fun r -> {    DTO.Deposit.Id = r.Id
                                    DTO.Deposit.Network = r.Network |> Network.Parse
                                    DTO.Deposit.UserName = r.UserName
                                    DTO.Deposit.Address = r.Address
                                    DTO.Deposit.PrivateKey = r.PrivateKey
                                    DTO.Deposit.PublicKey = r.PublicKey } )
    
    let private deleteAll() =
        Sql.DeleteAll.Create().Execute() |> ignore

    let Create() =
        { DAO.DepositDAO.Insert = insert
          DAO.DepositDAO.GetAllExcept = getAllExcept
          DAO.DepositDAO.GetBy = getBy
          DAO.DepositDAO.GetByAddress = getByAddress
          DAO.DepositDAO.DeleteAll = deleteAll
        }



        (*
        
        
         
        GetAll : Fee -> BankAddress -> Deposit list
        GetBy : UserName -> Option<Deposit>
        GetByAddress : FromAddress -> Network -> Option<Deposit>

        

        [Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Network] [nvarchar](10) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](50) NOT NULL,
	[PublicKey] [nvarchar](255) NOT NULL,
	[PrivateKey] [nvarchar](255) NOT NULL,

        *)