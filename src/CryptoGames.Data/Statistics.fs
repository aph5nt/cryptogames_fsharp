namespace CryptoGames.Data

open FSharp.Data
open CryptoGames

[<AutoOpen>]
module DataBase =
    [<Literal>]
    let Connection = "name=default"

module Statistics = 
    
    module Sql =
        type Insert = SqlCommandProvider<"INSERT INTO [dbo].[Statistics] VALUES (@GameId, @Network, @Type, @UserName, GETUTCDATE(), @Turn, @Bet, @Win, @Loss)", DataBase.Connection, ResultType.Records>  
        type GetByUserName = SqlCommandProvider<"""
        DECLARE @PageNumberP int
        set @PageNumberP = @PageNumber
        DECLARE @RowspPageP int
        set @RowspPageP = @RowspPage
        SELECT s.*, g.Size FROM dbo.[Statistics] s
        JOIN dbo.Games g ON g.Id = s.GameId
        WHERE s.UserName = @UserName AND s.Network = @Network
        ORDER BY Id DESC 
        OFFSET ((@PageNumberP - 1) * @RowspPageP) ROWS FETCH NEXT @RowspPageP ROWS ONLY
        """, DataBase.Connection, ResultType.Records>
        type GetByUserNameTotalPages = SqlCommandProvider<"""
            SELECT COUNT(Id) as 'totalItems' FROM dbo.[Statistics]
            WHERE UserName = @UserNameP AND Network = @NetworkP
            """, DataBase.Connection, ResultType.Records, SingleRow = true>
        type GetLastGames = SqlCommandProvider<"""
            SELECT TOP(25) s.*, g.Size FROM dbo.[Statistics] s
            JOIN dbo.Games g ON g.Id = s.GameId
            WHERE s.Network = @Network
            ORDER BY Id DESC 
            """, DataBase.Connection, ResultType.Records>
        type GetBestPlayers = SqlCommandProvider<"""
          SELECT TOP(25) Sum(Win) as TotalWin, MAX(Win) as MaxWin, [UserName]
          FROM [GreedyRun].[dbo].[Statistics]
           WHERE Network = @Network AND CreatedAt >= dateadd(month, -1, getutcdate())
          GROUP BY UserName
          ORDER BY TotalWin DESC, MaxWin DESC
            """, DataBase.Connection, ResultType.Records>
          
    let private insert network gameId gameType userName turn bet win loss =
        Sql.Insert.Create().Execute(gameId, network.ToString(), gameType.ToString(), userName, turn, bet, win, loss) |> ignore
   
    let private getByUserName network userName pageNumber rowsPage = 
        Sql.GetByUserName.Create().Execute(pageNumber, rowsPage, userName, network.ToString())
        |> Seq.map (fun db -> { DTO.Statistic.Id = db.Id
                                DTO.Statistic.GameId = db.GameId
                                DTO.Statistic.Size = db.Size
                                DTO.Statistic.Network = db.Network |> Network.Parse
                                DTO.Statistic.Type = db.Type |> GameType.Parse
                                DTO.Statistic.UserName = db.UserName
                                DTO.Statistic.CreatedAt = db.CreatedAt
                                DTO.Statistic.Turn = db.Turn
                                DTO.Statistic.Bet = db.Bet
                                DTO.Statistic.Win = db.Win
                                DTO.Statistic.Loss = db.Loss } ) |> Seq.toList
    
    let private getByUserNameTotalPages network userName =
        Sql.GetByUserNameTotalPages.Create().Execute(userName, network.ToString()).Value.Value
    
    let private getLastGames network = 
        Sql.GetLastGames.Create().Execute(network.ToString())
        |> Seq.map (fun db -> { DTO.Statistic.Id = db.Id
                                DTO.Statistic.GameId = db.GameId
                                DTO.Statistic.Size = db.Size
                                DTO.Statistic.Network = db.Network |> Network.Parse
                                DTO.Statistic.Type = db.Type |> GameType.Parse
                                DTO.Statistic.UserName = db.UserName
                                DTO.Statistic.CreatedAt = db.CreatedAt
                                DTO.Statistic.Turn = db.Turn
                                DTO.Statistic.Bet = db.Bet
                                DTO.Statistic.Win = db.Win
                                DTO.Statistic.Loss = db.Loss } ) |> Seq.toList

    let private getBestPlayers network =
        let result = Sql.GetBestPlayers.Create().Execute(network.ToString()) 
        result |> Seq.map (fun db -> 
                                { DTO.BestPlayer.TotalWin = db.TotalWin.Value
                                  DTO.BestPlayer.MaxWin = db.MaxWin.Value
                                  DTO.BestPlayer.UserName = db.UserName
                                }) |> Seq.toList

    let Create() = 
        { DAO.StatisticDAO.Insert = insert
          DAO.StatisticDAO.GetBestPlayers = getBestPlayers
          DAO.StatisticDAO.GetLastGames = getLastGames
          DAO.StatisticDAO.GetByUserName = getByUserName
          DAO.StatisticDAO.GetByUserNameTotalPages = getByUserNameTotalPages }