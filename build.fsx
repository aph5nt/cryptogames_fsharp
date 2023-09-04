#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open Fake.Testing.XUnit2
open System
open System.IO

let project = "CryptoGames"
let summary = "Project has no summmary; update build.fsx"
let description = "Project has no description; update build.fsx"
let authors = [ "aph5nt" ]
let tags = ""
let solutionFile  = "CryptoGames.sln"
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"
let release = LoadReleaseNotes "RELEASE_NOTES.md"

let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

Target "AssemblyInfo" (fun _ ->
    let getAssemblyInfoAttributes projectName =
        [ Attribute.Title (projectName)
          Attribute.Product project
          Attribute.Description summary
          Attribute.Version release.AssemblyVersion
          Attribute.FileVersion release.AssemblyVersion ]

    let getProjectDetails projectPath =
        let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          System.IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    !! "src/**/*.??proj"
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, projectName, folderName, attributes) ->
        match projFileName with
        | Fsproj -> CreateFSharpAssemblyInfo (folderName </> "AssemblyInfo.fs") attributes
        | Csproj -> CreateCSharpAssemblyInfo ((folderName </> "Properties") </> "AssemblyInfo.cs") attributes
        | Vbproj -> CreateVisualBasicAssemblyInfo ((folderName </> "My Project") </> "AssemblyInfo.vb") attributes
        | Shproj -> ()
        ))

Target "CopyBinaries" (fun _ ->
    !! "src/**/*.??proj"
    -- "src/**/*.shproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin/Release", "bin" </> (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true)))
Target "Clean" (fun _ -> CleanDirs ["bin"; "temp"])
Target "CleanDocs" (fun _ -> CleanDirs ["docs/output"] )
Target "Build" (fun _ -> !! solutionFile |> MSBuildRelease "" "Rebuild"  |> ignore )

Target "RunTests" (fun _ ->
    let cfg p =
        { p with 
            ToolPath = "packages/test/xunit.runner.console/tools/xunit.console.x86.exe"
            MaxThreads = MaxThreads(1)
            Parallel = NoParallelization
            ShadowCopy = false
            HtmlOutputPath = Some(@"tests/output.html") }

    !! testAssemblies |> xUnit2 cfg)
 
 
let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
let fakeStartInfo script workingDirectory args fsiargs environmentVars =
    (fun (info: System.Diagnostics.ProcessStartInfo) ->
        info.FileName <- System.IO.Path.GetFullPath fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)

/// Run the given buildscript with FAKE.exe
let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
    let exitCode =
        ExecProcessWithLambdas
            (fakeStartInfo script workingDirectory "" fsiargs envArgs)
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

// Documentation
let buildDocumentationTarget fsiargs target =
    trace (sprintf "Building documentation (%s), this could take some time, please wait..." target)
    let exit = executeFAKEWithOutput "docs/tools" "generate.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "generating reference documentation failed"
    ()

Target "GenerateReferenceDocs" (fun _ -> buildDocumentationTarget "-d:RELEASE -d:REFERENCE" "Default")

let generateHelp' fail debug =
    let args =
        if debug then "--define:HELP"
        else "--define:RELEASE --define:HELP"
    try
        buildDocumentationTarget args "Default"
        traceImportant "Help generated"
    with
    | e when not fail ->
        traceImportant "generating help documentation failed"

let generateHelp fail = generateHelp' fail false
Target "GenerateHelp" (fun _ -> generateHelp true)
Target "GenerateHelpDebug" (fun _ ->  generateHelp' true true)

Target "KeepRunning" (fun _ ->
    use watcher = !! "docs/content/**/*.*" |> WatchChanges (fun changes ->
         generateHelp' true true
    )
    traceImportant "Waiting for help edits. Press any key to stop."
    System.Console.ReadKey() |> ignore
    watcher.Dispose())

Target "GenerateDocs" DoNothing
Target "DailyBuild" DoNothing

"CleanDocs"
    ==> "GenerateHelp"
    ==> "GenerateReferenceDocs"
    ==> "GenerateDocs"

"Clean"
    ==> "CleanDocs"
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "CopyBinaries"
    ==> "GenerateReferenceDocs"
    ==> "GenerateDocs"
    ==> "RunTests"
    ==> "DailyBuild"

"GenerateHelpDebug"
    ==> "KeepRunning"

RunTargetOrDefault "DailyBuild"
