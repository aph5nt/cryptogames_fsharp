namespace CryptoGames

open System
open WebSocketSharp
open WebSocketSharp.Server
open WebSocketSharp.Net
open System.Collections.Generic


module UINotfierProxy = ()
//    open Akka.FSharp

//    type NotifierProxyActor(system : ActorSystem) as x =
//        inherit ReceiveActor()
//        let mutable notifier : IActorRef = null;
//        let handleOnBalanceUpdated (msg : OnBalanceUpdated) = 
//            if notifier = null then
//                try
//                    notifier <- system.ActorSelection("akka.tcp://GreedyRunWebApi@localhost:8084/user/notifier").ResolveOne(timeout) |> Async.AwaitTask |> Async.RunSynchronously
//                with
//                | exn -> ()
//            if notifier <> null then
//                notifier <! msg
//        do
//            x.Receive<OnBalanceUpdated> handleOnBalanceUpdated
//        static member Create(system : ActorSystem) =
//            let name = "notifierProxy"
//            let props = Akka.Actor.Props.Create(typeof<NotifierProxyActor>, system)
//            let actorRef = system.ActorOf(props, name);
//            system.EventStream.Subscribe(actorRef, typeof<OnBalanceUpdated>) |> ignore
//            actorRef

//    let Create() =
//        let run system = NotifierProxyActor.Create system |> ignore
//        { Services.UINotifier.Task.Run =  run }

//module UINotifier = 
//    type NotifierBehavior() as x =
//        inherit WebSocketBehavior()
//        static let monitor = new Object()
//        static let mutable userSessions = new Dictionary<string, List<string>>()
//        do
//            x.IgnoreExtensions <- true
            
//        override x.OnOpen() = 
//            if x.Context.IsAuthenticated then
//                let userName = x.Context.User.Identity.Name
//                let sessions = new List<string>()
//                lock monitor (fun() -> 
//                        if not (userSessions.ContainsKey(userName)) then
//                            sessions.Add x.ID
//                            userSessions.[userName] <- sessions
//                        else
//                            userSessions.[userName].Add(x.ID)
//                )
//                ()

//        override x.OnClose _ =
//            if x.Context.IsAuthenticated then
//                let userName = x.Context.User.Identity.Name
                
//                lock monitor (fun() -> 
//                    if (userSessions.ContainsKey(userName)) then
//                        let sessions = userSessions.[userName]
//                        sessions.Remove(x.ID) |> ignore
//                        if sessions.Count = 0 then 
//                            userSessions.Remove(userName) |> ignore
//                        else
//                            userSessions.[userName] <- sessions
//            )

//        override x.OnMessage args =
//            try
//                let evnt = Newtonsoft.Json.JsonConvert.DeserializeObject<OnBalanceUpdated>(args.Data)
//                let mutable activeSessions : List<string> = new List<string>()
//                lock monitor (fun() -> activeSessions <- userSessions.[evnt.UserName])
//                for s in activeSessions  do
//                    //let json = sprintf """{  "network": "%s", "amount": "%s" }  """ (evnt.Network |> toString) (evnt.Balance.ToString())
//                    x.Sessions.SendTo(args.Data, s)

//            with exn -> 
//                ()

 
//    type NotifierActor(database : Database) as x =
//        inherit ReceiveActor()

//        let name = "notifier"
//        let wsserver = new WebSocketServer "ws://localhost:64952" 
//        let ws = new WebSocket "ws://localhost:64952/notifier"

//        let handleOnBalanceUpdated (msg : OnBalanceUpdated) = 
//            if ws.IsAlive <> true then 
//                ws.SetCredentials(name, name, true)
//                ws.Connect()
//            ws.Send(Newtonsoft.Json.JsonConvert.SerializeObject(msg))
//        do
//            let localPath = System.Reflection.Assembly.GetExecutingAssembly().Location
//            let logPath = System.IO.Path.Combine(localPath, "..\\logs\ws.log")
//            ws.Log.File <- logPath
//            wsserver.AddWebSocketService<NotifierBehavior>("/notifier")
//            wsserver.AuthenticationSchemes <- AuthenticationSchemes.Digest
//            wsserver.Realm <- "greedyrun"
//            wsserver.ReuseAddress <- true
            
//            wsserver.UserCredentialsFinder <- (fun identity -> 
//            match identity.Name.Contains("notifier") with
//            | true -> new NetworkCredential(identity.Name, identity.Name)
//            | false ->  
//                match database.UserAuths.Exists identity.Name with
//                | true -> new NetworkCredential(identity.Name, identity.Name)
//                | false -> null)
//            wsserver.Start()

//            x.Receive<OnBalanceUpdated> handleOnBalanceUpdated
        
//        interface IDisposable with
//            member x.Dispose() =
//                ws.Close()
//                wsserver.Stop()
//                GC.SuppressFinalize(x)

//        static member Create(system : ActorSystem) =
//            let name = "notifier"
//            let props = Akka.Actor.Props.Create(typeof<NotifierActor>, CryptoGames.Data.Database.Create())
//            let actorRef = system.ActorOf(props, name);
//            actorRef

//    let Create() =
//        let run system = NotifierActor.Create system |> ignore
//        { Services.UINotifier.Task.Run =  run }