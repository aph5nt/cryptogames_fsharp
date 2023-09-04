namespace CryptoGames.Data

open FSharp.Data
open CryptoGames

//module Quartz = 
//    module Sql =
//        type JobExist = SqlCommandProvider<" SELECT top(1) JOB_NAME FROM [GreedyRunTest].[dbo].[QRTZ_JOB_DETAILS] where JOB_NAME = @JobName ", DataBase.Connection, ResultType.Records, SingleRow = true>  
         
//        type FiredTrigger  = SqlCommandProvider<""" 

//        SELECT TOP 100 [SCHED_NAME] FROM [dbo].[QRTZ_FIRED_TRIGGERS] WHERE TRIGGER_NAME = @TriggerName order by [FIRED_TIME] DESC
        
//        """, DataBase.Connection, ResultType.Records>  

//    let private jobExist name =
//        Sql.JobExist.Create().Execute(name)

//    let private firedTriggers name =
//        Sql.FiredTrigger.Create().Execute(name) |> Seq.toList

//    let Create() = 
//        { DAO.QuartzDAO.JobExist = jobExist 
//          DAO.QuartzDAO.FiredTrigger = firedTriggers } 
            

module Database =
  
    let Create() =
        { 
            DAO.Database.Forwards = Forwards.Create()
            DAO.Database.Withdraws = Withdraws.Create()
            DAO.Database.Deposits = Deposits.Create()
            DAO.Database.Balances = Balances.Create()
            DAO.Database.Statistics = Statistics.Create()
            DAO.Database.Games = Data.Games.Create()
            DAO.Database.Locks = Locks.Create()
            DAO.Database.Logs = Logs.Create()
            DAO.Database.UserAuths = UserAuths.Create()
            DAO.Database.FreeApiDeposits = FreeApiDeposits.Create()
            DAO.Database.GameSettings = GameSettings.Create()
        }
            
       
  