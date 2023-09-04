namespace CryptoGames

module CfgTypes =

    type WebApiCfg = {
        ServerActorSelectionBase : string
        LogPath : string
        WebSocketAddress : string
        SuaveAddress : string
        SuavePort : int
        WebAppPath : string
    }

    type WebServerCfg = {
        WebApiActorSelection : string
    }



