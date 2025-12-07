namespace RTFlow
open System
open Microsoft.Extensions.AI
open RTFlow.Functions

type ToolName = ToolName of string
type ToolCache = Map<ToolName,AITool>

module Toolbox =
    open System.Collections.Generic
    open System.ComponentModel
    open System.Reflection
    let invoke (call:FunctionCallContent) (tools:ToolCache) =
        let func =
            tools
            |> Map.tryFind (ToolName call.Name)
            |> Option.defaultWith (fun _ -> failwith $"function named {call.Name} not found in cache")
        // AIFunction.InvokeAsync expects AIFunctionArguments; wrap the IDictionary into that type
        let args = new AIFunctionArguments(call.Arguments)
        task {
            match func with 
            | :? AIFunction as func -> 
                let! rslt = func.InvokeAsync(args) 
                return FunctionResultContent(call.CallId, rslt)     
            | _ -> return failwith "AITool cannot be invoked as a function"
        }

    open Microsoft.SemanticKernel
    let getToolMetadata (t:Type) =
        let bindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.DeclaredOnly
        t.GetMethods(bindingFlags)
        |> Array.choose (fun m ->
            m.GetCustomAttributes(typeof<KernelFunctionAttribute>, false)
            |> Seq.cast<KernelFunctionAttribute>
            |> Seq.tryHead
            |> Option.map (fun kernelAttr ->
                let name =
                    if System.String.IsNullOrWhiteSpace kernelAttr.Name then
                        m.Name
                    else
                        kernelAttr.Name

                let description =
                    m.GetCustomAttributes(typeof<DescriptionAttribute>, false)
                    |> Seq.cast<DescriptionAttribute>
                    |> Seq.tryHead
                    |> Option.map (fun attr -> attr.Description)
                ToolName name,(m,description))
        )
        |> Map.ofArray

    let getToolNames ts = 
        ts 
        |> Seq.map getToolMetadata 
        |> Seq.collect Map.keys 
        |> Seq.toList
        |> List.distinct
        
    let makeToolsOne (t:obj) =
        getToolMetadata (t.GetType())
        |> Map.map (fun (ToolName name) (m,description) -> 
                let serializerOptions = FlowUtils.serOpts.Value
                let aiFunction =
                    AIFunctionFactory.Create(
                        m,
                        t,
                        name,
                        defaultArg description null,
                        serializerOptions
                    )

                aiFunction :> AITool)

    let makeTools (ts: obj seq) = 
        (Map.empty,ts) 
        ||> Seq.fold (fun acc o -> 
            (acc, makeToolsOne o |> Map.toList) 
            ||> List.fold(fun acc (k,v) -> 
                acc 
                |> Map.tryFind k 
                |> Option.map (fun _ -> acc) 
                |> Option.defaultWith (fun _ -> acc |> Map.add k v)))

    let filter maybeSet tools =
        maybeSet
        |> Option.map (fun xs ->
            let xs = set xs
            tools 
            |> Map.filter (fun k v -> xs.Contains k))
        |> Option.defaultValue tools
