namespace FsAICore

open System.Text.Json
open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.AI   // For IChatClient, ChatOptions, etc.

#nowarn "57"

module ConfigKeys =
    let ANTHROPIC_API_KEY = "ANTHROPIC_API_KEY"
    let OPENAI_API_KEY = "OPENAI_API_KEY"
    let CHAT_MODEL_ID = "CHAT_MODEL_ID"
    
///Need this distinction until MxExtAI abstracts more backends
type AIBackend = OpenAILike | AnthropicLike 
type AIContext = {
    backend:AIBackend
    
    ///Required services: IChatClient, IConfiguration (see ConfigKeys for expected key names, depending on backends used).   
    kernel:IServiceProvider
    
    jsonSerializationOptions : JsonSerializerOptions option
        
    ///Tool implementations mapped to their names. Should have at least the tools for the list in 'tools'.
    toolsCache:ToolCache
    
    ///Configure any non-tool option settings.
    optionsConfigurator : (ChatOptions -> unit) option
}
    with static member Default = {
                kernel=null
                backend=OpenAILike
                jsonSerializationOptions = None
                toolsCache=Map.empty
                optionsConfigurator = None
    }

module AnthropicClient = 
    open Anthropic.SDK
    open Anthropic.SDK.Messaging

    let createAnthropicClient(key:string) =
        let httpClient = new System.Net.Http.HttpClient()
        httpClient.DefaultRequestHeaders.Add("anthropic-beta","computer-use-2025-01-24")
        httpClient.DefaultRequestHeaders.Add("anthropic-beta","structured-outputs-2025-11-13") //supposedly not needed with new api
        new AnthropicClient(key,httpClient)
        
    let createClientWithKey(key) : IChatClient =  
        (createAnthropicClient(key))
            .Messages
            .AsBuilder()
            //.UseFunctionInvocation()
            .Build()
            
    let createClient(cfg:IConfiguration) : IChatClient =
        createClientWithKey(cfg.[ConfigKeys.ANTHROPIC_API_KEY])
        
    let thinking = lazy(
        let tp = ThinkingParameters()
        tp.BudgetTokens <- 2048
        tp
    )
    
    let toSchema(t:Type) = AIJsonUtilities.CreateJsonSchema(t)

module OpenAIClient =
                
    let createClientWithKey(key:string,modelId:string) : IChatClient =
        let oaiClient = OpenAI.OpenAIClient(key)
        let respClient = oaiClient.GetResponsesClient(modelId)
        respClient.AsIChatClient() 
 
    let createClient(cfg:IConfiguration) =
        createClientWithKey(cfg.[ConfigKeys.OPENAI_API_KEY], cfg.[ConfigKeys.CHAT_MODEL_ID])


module AIUtils =
    open System.IO
    
    let lines keepEmptyLines (s:string) =
        seq{
            use sr = new StringReader(s)
            let mutable line = sr.ReadLine()
            while line <> null do
                yield line
                line <- sr.ReadLine()
        }
        |> Seq.filter (fun s -> if keepEmptyLines then true else s |> String.IsNullOrWhiteSpace |> not)
        |> Seq.toList
        
    let extractTripleQuoted (inp:string) =
        let lines = lines false inp
        let addSnip acc accSnip =
            match accSnip with
            |[] -> acc
            | _ -> (List.rev accSnip)::acc
        let isQuote (s:string) = s.StartsWith("```")
        let rec start acc (xs:string list) =
            match xs with
            | []                   -> List.rev acc
            | x::xs when isQuote x -> accQuoted acc [] xs
            | x::xs                -> start acc xs
        and accQuoted acc accSnip xs =
            match xs with
            | []                   -> List.rev (addSnip acc accSnip)
            | x::xs when isQuote x -> start (addSnip acc accSnip) xs
            | x::xs                -> accQuoted acc (x::accSnip) xs
        start [] lines
    
    let extractCode (inp:string) =
        if inp.IndexOf("```", min 10 inp.Length) >= 0 then 
            extractTripleQuoted inp
            |> Seq.collect id
            |> fun xs -> String.Join("\n",xs)
        else
            inp    
    
    let content chooser (asstResp:ChatMessage option) =
        asstResp
        |> Option.map (fun m -> m.Contents|> Seq.cast<AIContent>)
        |> Option.defaultValue Seq.empty
        |> Seq.choose chooser
        |> Seq.toList            
    
    let textContent msgs = 
        content (function :? TextContent as c -> Some c.Text | _ -> None) msgs
        |> Seq.tryHead

    let asstMsg (response:ChatResponse) = 
        response.Messages
        |> Seq.rev
        |> Seq.tryFind (fun m -> m.Role = ChatRole.Assistant)
        |> Option.defaultWith (fun _ -> failwith "Assistant response missing after tool invocation")

    /// Sends a request to backend LLM with retry but without automated tool calling.
    /// Returns the raw LLM response (which may be tool call).
    let rec sendRequestBase (retries:int) (context:AIContext) (tools:ToolName list)  (history : ChatMessage seq)= async {
        try
            let cfg = context.kernel.GetRequiredService<IConfiguration>()
            let opts = ChatOptions()           
            opts.Temperature <- 0.2f
            context.optionsConfigurator |> Option.iter(fun f -> f opts)            
            opts.Tools <- context.toolsCache |> Toolbox.filter (Some tools)|> Map.toSeq |> Seq.map snd |> ResizeArray
            let client =
                if context.backend.IsAnthropicLike then 
                    opts.ModelId <- cfg.[ConfigKeys.CHAT_MODEL_ID]
                    AnthropicClient.createClient(cfg)           
                else
                    failwith "only anthropic supported"
                    //OpenAIClient.createClient(cfg)
            let! resp = client.GetResponseAsync<'ResponseFormat>(history,opts,useJsonSchemaResponseFormat=true) |> Async.AwaitTask
            return resp
        with ex ->
            if retries <= 0 then
                do! Async.Sleep 1000
                return! sendRequestBase (retries-1) context tools history
            else
                Log.exn(ex,"sendRequest")
                return raise ex
    }    
    
    ///Sends a request to backend LLM with - automated tool calling and retries - to obtain a structured response.   
    let rec sendRequest<'ResponseFormat> (retries:int) (context:AIContext) (tools:ToolName list)  (history : ChatMessage seq)= async {
        try
            let cfg = context.kernel.GetRequiredService<IConfiguration>()
            let opts = ChatOptions()
            opts.Temperature <- 0.2f
            context.optionsConfigurator |> Option.iter(fun f -> f opts)            
            opts.ToolMode <- ChatToolMode.Auto
            opts.Tools <- context.toolsCache |> Toolbox.filter (Some tools) |> Map.toSeq |> Seq.map snd |> ResizeArray
            opts.ResponseFormat <- ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema typeof<'ResponseFormat>)
            let client =
                if context.backend.IsAnthropicLike then 
                    opts.ModelId <- cfg.[ConfigKeys.CHAT_MODEL_ID]
                    AnthropicClient.createClient(cfg)           
                else
                    failwith "only anthropic supported"
                    //OpenAIClient.createClient(cfg)
            let! resp = client.GetResponseAsync<'ResponseFormat>(history,opts,useJsonSchemaResponseFormat=true) |> Async.AwaitTask
            let structuredResp = //need this until structured output support is added to anthropic lib
                if context.backend.IsAnthropicLike then
                    let asstMsg = asstMsg resp
                    let text = textContent (Some asstMsg) |> Option.defaultWith (fun _ -> failwith "code not found")
                    let code = extractCode text
                    JsonSerializer.Deserialize<'ResponseFormat>(code)                  
                else
                    resp.Result
            let usage = [resp.ModelId,resp.Usage] |> Map.ofList           
            return structuredResp,usage
        with ex ->
            if retries <= 0 then
                do! Async.Sleep 1000
                return! sendRequest (retries-1) context tools history
            else
                Log.exn(ex,"sendRequest")
                return raise ex
    }    
 
