open FSPowerShell.PSRuntime.External

open System
open System.ServiceModel
open System.ServiceModel.Description

[<EntryPoint>]
let main argv = 
    let snapIns = argv
    let psRuntimeService = PSRuntimeService(snapIns)
    let serviceHost = 
        new ServiceHost(psRuntimeService, [|Uri(ExternalPowerShellHost)|])
    serviceHost.AddServiceEndpoint(
        typeof<IPSRuntimeService>, 
        getNetNamedPipeBinding(), 
        ExternalPowerShellServiceName) 
      |> ignore

    serviceHost.Open()

    for endpoint in serviceHost.Description.Endpoints do
        printfn "%s" (endpoint.Address.Uri.AbsoluteUri)

    Console.ReadLine() |> ignore
    0 // return an integer exit code
