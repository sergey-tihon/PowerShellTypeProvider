module FSPowerShell.PSRuntime

open System
open System.Management.Automation
open System.Management.Automation.Runspaces

type PSRuntime(snapIns:string[]) =
    let runSpace = 
        let config = RunspaceConfiguration.Create(); 
        for snapIn in snapIns do
            if (not <| String.IsNullOrEmpty(snapIn)) then 
                let info, ex = config.AddPSSnapIn(snapIn)
                if ex <> null then
                    failwithf "AddPSSnapInException: %s" ex.Message
        let rs = RunspaceFactory.CreateRunspace(config)
        rs.Open()
        rs

    let cmdlets =
        runSpace.RunspaceConfiguration.Cmdlets
        |> Seq.map (fun cmdlet ->
                       let wrapper = PSCommandLet cmdlet
                       wrapper.RawName, wrapper)
        |> Map.ofSeq

    member __.AllCmdlets
        with get() = cmdlets |> Map.toSeq |> Seq.map snd

    member __.Execute(rawName,parameters:obj seq) =
        // Create command
        let cmdlet = cmdlets.[rawName]
        let command = Command(cmdlet.RawName)
        parameters |> Seq.iteri (fun i value->
            let key, ty = cmdlet.ParameterProperties.[i]
            match ty with
            | _ when ty = typeof<System.Management.Automation.SwitchParameter>->
                if (unbox<bool> value) then 
                    command.Parameters.Add(CommandParameter(key))
            | _ when ty.IsValueType ->
                if (value <> System.Activator.CreateInstance(ty))
                then command.Parameters.Add(CommandParameter(key, value))
            | _ -> 
                if (value <> null)
                then command.Parameters.Add(CommandParameter(key, value))
        )
        // Execute
        let pipeline = runSpace.CreatePipeline()
        pipeline.Commands.Add(command)
        let result = pipeline.Invoke()

        // Format result
        let tyOfObj = PSInference.getTypeOfObjects result
        let tys = cmdlet.ResultObjectTypes
        let len = tys.Length

        let targetType = 
            if (1<=len && len<=7) then tyOfObj else typeof<PSObject>
        let collectionConverter = 
            typedefof<PSInference.CollectionConverter<_>>.MakeGenericType(targetType)
        let objectCollection = 
            if (len=0) then box result 
            else result |> Seq.map (fun x->x.BaseObject) |> box
        let typedCollection = 
            collectionConverter.GetMethod("Convert").Invoke(null, [|objectCollection|])

        if (len <=1 || len > 7) 
        then typedCollection
        else let ind = tys |> Array.findIndex (fun x-> x = tyOfObj)
             let funcName = sprintf "NewChoice%dOf%d" (ind+1) (tys.Length)
             cmdlet.ResultType.GetMethod(funcName).Invoke(null, [|typedCollection|])

    member __.GetXmlDoc(rawName:string) =
        // Create command
        let command = Command("Get-Help")
        command.Parameters.Add(CommandParameter("Name", rawName))
        // Execute
        let pipeline = runSpace.CreatePipeline()
        pipeline.Commands.Add(command)
        let result = pipeline.Invoke() |> Seq.toArray
        // Format result
        let (?) (this : PSObject) (prop : string) : obj =
            let prop = this.Properties |> Seq.find(fun p -> p.Name = prop)
            prop.Value
        match result with
        | [|help|] ->
            let lines = 
                (help?description :?> PSObject[])
                |> Array.map (fun x->x?Text :?> string)
            sprintf "<summary><para>%s</para></summary>"
                (String.Join("</para><para>", lines |> Array.map (fun s->s.Replace("<","").Replace(">",""))))
        | _ -> String.Empty

// PowerShell runtime singleton
let private runtime = ref Option<PSRuntime>.None
let Current(snapIns) = 
    if (Option.isNone !runtime)
        then runtime := Some(PSRuntime(snapIns))
    (!runtime) |> Option.get
