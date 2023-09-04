namespace CryptoGames

open System
open System.Threading
open Microsoft.FSharp.Reflection
open System.Globalization

[<AutoOpen>]
module LocalSettings =
    let SetGlobalizationSettings() =
        let customCulture = Thread.CurrentThread.CurrentCulture.Clone() :?> CultureInfo
        customCulture.NumberFormat.NumberDecimalSeparator <- "."
        Thread.CurrentThread.CurrentCulture <- customCulture
 
[<AutoOpen>]
module SystemTime = 
    let mutable UtcNow = fun() -> DateTime.UtcNow
    let SetDateTime (date : DateTime) = UtcNow <- fun() -> date
    let ResetDateTime() = UtcNow <- fun() -> DateTime.UtcNow

[<AutoOpen>]
module Helpers = 
    let toString (x:'a) = 
            match FSharpValue.GetUnionFields(x, typeof<'a>) with
                case, _ -> case.Name

    let fromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> FSharpValue.MakeUnion(case,[||]) :?> 'a
        |_ -> failwith "not supported"

    let AsDecimal (str :string) =
        try
            Decimal.Parse(str.Replace(',','.'), NumberStyles.Any, CultureInfo.InvariantCulture)
        with 
        | exn -> 0m

[<AutoOpen>]
module Scheduler =

    type ScheduleBuilder() =
        
        let schedule (timer : Timer) (due : int, interval : int) =
            timer.Change(due, interval) |> ignore

        let create ((due, interval), callback) =
            let timer = new Timer(fun _ -> callback())      
            schedule timer (due, interval)  
            timer  

        member __.Bind(((due, interval), callback), f) = 
            f((due, interval), callback)

        member __.Return((due, interval), callback) = 
            create((due, interval),callback)

    let schedule = new ScheduleBuilder()