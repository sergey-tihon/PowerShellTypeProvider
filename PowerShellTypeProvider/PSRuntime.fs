﻿module FSPowerShell.PSRuntime.Manager

open System
open FSPowerShell.PSRuntime.Hosted
open FSPowerShell.PSRuntime.External

// PowerShell runtime resolver
let private runtimes = ref Map.empty<string[] * bool, IPSRuntime>
let Current(snapIns, is64bitRequired, isDesignTime) =
    let key = (snapIns, isDesignTime)
    if (not <| Map.containsKey key !runtimes) then
        let value = 
            if (is64bitRequired && not(Environment.Is64BitProcess)) then
                if (isDesignTime) 
                then new PSRuntimeExternal(snapIns) :> IPSRuntime
                else failwith "You should compile your code as x64 application"
            else 
                PSRuntimeHosted(snapIns) :> IPSRuntime
        runtimes := (!runtimes |> Map.add key value)
    (!runtimes).[key]
