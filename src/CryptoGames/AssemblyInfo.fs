namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("CryptoGames")>]
[<assembly: AssemblyProductAttribute("CryptoGames")>]
[<assembly: AssemblyDescriptionAttribute("Project has no summmary; update build.fsx")>]
[<assembly: AssemblyVersionAttribute("0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1"
    let [<Literal>] InformationalVersion = "0.1"
