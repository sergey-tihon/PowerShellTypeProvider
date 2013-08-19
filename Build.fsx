// include Fake lib
#r @"build\FAKE\tools\FakeLib.dll"
open Fake 
open Fake.AssemblyInfoFile

// Assembly / NuGet package properties
let projectName = "PowerShellProvider"
let version = "0.2.1"
let projectSummary = "An F# Type Provider providing strongly typed access to PowerShell cmdlets."
let projectDescription = "An F# type provider for interoperating with PowerShell"
let authors = ["Sergey Tihon"]

// Folders
let buildDir = @".\build\temp"
let nugetDir = @".\build\nuget\"

// Targets

// Update assembly info
Target "UpdateAssemblyInfo" (fun _ ->
    CreateFSharpAssemblyInfo ".\AssemblyInfo.fs"
        [ Attribute.Product projectName
          Attribute.Title projectName
          Attribute.Description projectDescription
          Attribute.Version version ]
)

// Clean build directory
Target "Clean" (fun _ ->
    CleanDir buildDir
)

// Build PowerShell Type Provider
Target "BuildPowerShellTypeProvider" (fun _ ->
    !! @"PowerShellTypeProvider.ExternalRuntime\PowerShellTypeProvider.ExternalRuntime.fsproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

// Clean NuGet directory
Target "CleanNuGet" (fun _ ->
    CleanDir nugetDir
)

// Create NuGet package
Target "CreateNuGet" (fun _ ->     
    XCopy @".\build\temp" (nugetDir @@ @"lib\net45")
    !+ @"build/nuget/lib/net45/*.*"
        -- @"build/nuget/lib/net45/PowerShellTypeProvider*.dll"
        -- @"build/nuget/lib/net45/PowerShellTypeProvider*.exe"
        |> ScanImmediately
        |> Seq.iter (System.IO.File.Delete)   

    "PowerShellTypeProvider.nuspec"
      |> NuGet (fun p -> 
            {p with
                Project = projectName
                Authors = authors
                Version = version+"-alpha"
                Description = projectDescription
                Summary = projectSummary
                NoPackageAnalysis = true
                ToolPath = @".\.Nuget\Nuget.exe" 
                WorkingDir = nugetDir
                OutputPath = nugetDir })
)

// Default target
Target "Default" (fun _ ->
    trace "Building PowerShell Type Provider"
)

// Dependencies
"UpdateAssemblyInfo"
  ==> "Clean"
  ==> "BuildPowerShellTypeProvider"
  ==> "CleanNuGet"
  ==> "CreateNuGet"
  ==> "Default"

// start build
Run "Default"