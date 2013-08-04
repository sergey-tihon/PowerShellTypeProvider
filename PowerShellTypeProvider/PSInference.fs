namespace FSPowerShell

open System
open System.Management.Automation
open System.Management.Automation.Runspaces

module PSInference =
    
    let getOutputTypes (cmdlet:CmdletConfigurationEntry) =
        let types =
            cmdlet.ImplementingType.GetCustomAttributes(false) 
            |> Seq.filter (fun x-> x :? OutputTypeAttribute)
            |> Seq.cast<OutputTypeAttribute>
            |> Seq.fold (fun state (attr:OutputTypeAttribute) ->
                attr.Type 
                |> Array.map (fun x -> x.Type)
                |> Array.append state
              ) [||]
        match types with
        | [||] // Cmlets without declared OutputType may return values
        | _ when types |> Seq.exists (fun x -> x = null) // OutputTypeAttribute instantiated by string[]
            -> [|typeof<PSObject>|] 
        | _ -> types |> Array.rev

    let buildResultType possibleTypes = 
        let listOfTy ty = typedefof<list<_>>.MakeGenericType([|ty|])
        let tys = possibleTypes |> Array.map listOfTy
        match Array.length tys with
        | 1 -> tys.[0]
        | 2 -> typedefof<Choice<_,_>>.MakeGenericType(tys)
        | 3 -> typedefof<Choice<_,_,_>>.MakeGenericType(tys)
        | 4 -> typedefof<Choice<_,_,_,_>>.MakeGenericType(tys)
        | 5 -> typedefof<Choice<_,_,_,_,_>>.MakeGenericType(tys)
        | 6 -> typedefof<Choice<_,_,_,_,_,_>>.MakeGenericType(tys)
        | 7 -> typedefof<Choice<_,_,_,_,_,_,_>>.MakeGenericType(tys)
        | _ -> listOfTy typeof<PSObject>

    let getParameterProperties (cmdlet:CmdletConfigurationEntry) =
        cmdlet.ImplementingType.GetProperties()
        |> Seq.map (fun p ->
             let paramAttr = 
                p.GetCustomAttributes(false)
                |> Seq.tryFind (fun a -> (a :? ParameterAttribute))
             match paramAttr with
             | None -> None
             | Some(attr) ->
                Some(p.Name, p.PropertyType)//, ((attr :?> ParameterAttribute).Mandatory))
           )
        |> Seq.filter Option.isSome
        |> Seq.map Option.get
        |> Seq.toArray

    let toCamelCase s =
        if (String.IsNullOrEmpty(s) || not <| Char.IsLetter(s.[0]) || Char.IsLower(s.[0]))
            then s
            else sprintf "%c%s" (Char.ToLower(s.[0])) (s.Substring(1))

    let getTypeOfObjects (collection:PSObject seq) = 
        let types = 
            collection |> Seq.fold (fun state o ->
                state |> Set.add (o.BaseObject.GetType().ToString())
            ) (Set.empty)
        match (types |> Set.toList) with
        | [ty] -> (collection |> Seq.head).BaseObject.GetType()
        | [] -> typeof<PSObject>
        | x -> failwithf "Collection is heterogeneous:'%A'" x

    type CollectionConverter<'T> =
        static member Convert (objSeq:obj seq) = 
            objSeq |> Seq.cast<'T> |> Seq.toList


type PSCommandLet(cmdlet:CmdletConfigurationEntry) = 
    member __.RawName
        with get() = cmdlet.Name
    member __.Name 
        with get() = __.RawName //.Replace("-","")

    member __.ResultObjectTypes 
        with get() = (lazy (cmdlet |> PSInference.getOutputTypes)).Value
    member __.ResultType
        with get() = (lazy (__.ResultObjectTypes |> PSInference.buildResultType)).Value
    member __.ParameterProperties 
        with get() = (lazy (cmdlet |> PSInference.getParameterProperties)).Value