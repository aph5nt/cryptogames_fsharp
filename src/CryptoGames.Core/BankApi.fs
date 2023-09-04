namespace CryptoGames

open CryptoGames.Data
 
module Bankroll =
    
    let Create(database : DAO.Database) =

        let run (dependency : Services.Bankroll.Dependency) (args :  Services.Bankroll.Args) =
            for network in args.Networks do
                for game in args.Games do
                    let userName = (game.ToString())
                    let externalApi = dependency.Api.ExternalApis.[network]
                    let internalApi = dependency.Api.InternalApi
                    let x = externalApi.CreateAddress "asd" Services.Payment.ExternalApi
                    let result = externalApi.GetBalance userName 
                    match result with
                    | None -> 
                        let b = externalApi.CreateAddress userName
                        internalApi.UpdateBalance network userName b.Address 0m
                    | Some(b) ->
                        internalApi.TryCreateBalance network b.UserName b.Address b.Balance

            database.Balances.GetBankAll()
            |> Seq.map(fun b -> (b.Network, b.Address))
            |> Map.ofSeq

        { Services.Bankroll.Run = run}

(*
    unsynced external api and local one fro bankrolls.
    do we need to store the bank roll amount?
    YES

    HOW TO SETUP THE BANK ROLL
    HOW TO ADD FUND TO BANK ROLL

    TODO: job for monitoring the funds; task for grafana!!
*)