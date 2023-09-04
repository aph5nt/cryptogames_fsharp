namespace CryptoGames

[<AutoOpen>]
module Common = 
    let getNetwork network = 
        match network with
        | Network.BTC ->  NBitcoin.Network.Main
        | Network.BTCTEST ->  NBitcoin.Network.TestNet
        | _ -> failwith "unsupported network"