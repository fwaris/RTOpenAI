namespace FsAICore
open System
open System.Text.Json
open Microsoft.Extensions.AI

type ToolName = ToolName of string
type ToolCache = Map<ToolName,AITool>

module Toolbox =
    open System.ComponentModel
    open System.Reflection
    
    let defaultSerOpts = lazy(
        let opts = JsonSerializerOptions()
        opts.TypeInfoResolver <- System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        opts.WriteIndented <- true
        opts)
    
    ///Explicitly handle a function call by looking up and invoking the function implementation in the given ToolCache. 
    let invoke (call:FunctionCallContent) (tools:ToolCache) =
        let func =
            tools
            |> Map.tryFind (ToolName call.Name)
            |> Option.defaultWith (fun _ -> failwith $"function named {call.Name} not found in cache")
        let args = new AIFunctionArguments(call.Arguments)
        task {
            match func with 
            | :? AIFunction as func -> 
                let! rslt = func.InvokeAsync(args) 
                return FunctionResultContent(call.CallId, rslt)     
            | _ -> return failwith "AITool cannot be invoked as a function"
        }
   
    open Microsoft.SemanticKernel
    ///Extract any methods tagged with KernelFunction attribute from the given type.
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

    ///Extract tool names from the list of types for all methods tagged with KernelFunction. 
    let getToolNames ts = 
        ts 
        |> Seq.map getToolMetadata 
        |> Seq.collect Map.keys 
        |> Seq.toList
        |> List.distinct
                
    ///Convert the KernelFunction tagged methods (in the type of the given instance) to AIFunction instances.     
    let makeToolsOne (serializationOptions:JsonSerializerOptions option) (t:obj) =
        getToolMetadata (t.GetType())
        |> Map.map (fun (ToolName name) (m,description) -> 
                let serializerOptions = defaultArg serializationOptions defaultSerOpts.Value
                let aiFunction =
                    AIFunctionFactory.Create(
                        m,
                        t,
                        name,
                        defaultArg description null,
                        serializerOptions
                    )

                aiFunction :> AITool)

    ///Convert the KernelFunction tagged methods to AIFunction instances.     
    let makeTools (serializationOptions:JsonSerializerOptions option) (ts: obj seq) = 
        (Map.empty,ts) 
        ||> Seq.fold (fun acc o -> 
            (acc, makeToolsOne serializationOptions o |> Map.toList) 
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
