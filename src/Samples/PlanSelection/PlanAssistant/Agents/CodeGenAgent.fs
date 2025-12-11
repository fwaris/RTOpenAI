namespace RT.Assistant.WorkFlow
open System.Text.Json
open Microsoft.Extensions.AI
open Fabulous
open RT.Assistant.Plan
open RTFlow
open RTFlow.Functions

exception FlowException of exn * Microsoft.Extensions.AI.ChatMessage list

module CodeGenAgent = 
    open FSharp.Control
 
     type internal State = {viewRef:ViewRef<Microsoft.Maui.Controls.HybridWebView>; bus:WBus<FlowMsg,AgentMsg>}
        with
            static member Create viewRef bus = {viewRef = viewRef; bus=bus}
                            
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
    
    let chatOptions autoInvoke tools = 
        let opts = ChatOptions()
        if autoInvoke then
            opts.ToolMode <- ChatToolMode.Auto
        opts.ModelId <- Anthropic.SDK.Constants.AnthropicModels.Claude45Sonnet
        opts.Tools <-
            tools
            |> Map.toSeq
            |> Seq.map snd
            |> ResizeArray    
        opts

    let mapUsage (usage:UsageDetails) =
        let input = usage.InputTokenCount.GetValueOrDefault() |> int
        let output = usage.OutputTokenCount.GetValueOrDefault() |> int
        let total =  usage.TotalTokenCount.GetValueOrDefault() |> int
        let total = if total < input + output then input + output else total
        {
          RTFlow.Usage.input_tokens = input
          RTFlow.Usage.output_tokens = output
          RTFlow.Usage.total_tokens = total
        }

    let internal sendRequest state (history : ChatMessage list)= async {
        let client = Anthropic.Client.createClient()
        let opts = ChatOptions()
        opts.ResponseFormat <- ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema typeof<RT.Assistant.Plan.CodeGenResp>)
        opts.ModelId <- Anthropic.SDK.Constants.AnthropicModels.Claude45Sonnet
        let! resp = client.GetResponseAsync<RT.Assistant.Plan.CodeGenResp>(history,opts,useJsonSchemaResponseFormat=true) |> Async.AwaitTask
        let asstMsg = asstMsg resp
        let text = textContent (Some asstMsg) |> Option.defaultWith (fun _ -> failwith "code not found")
        let code = AICore.extractCode text
        let prolog = JsonSerializer.Deserialize<RT.Assistant.Plan.CodeGenResp>(code)
        let usage = [resp.ModelId,mapUsage resp.Usage] |> Map.ofList
        state.bus.PostToFlow(W_Msg (Fl_Usage usage))
        return prolog
    }
    
    type Query = {query:string}
    
    let createRequest query =
        [
            ChatMessage(ChatRole.System, CodeGenPrompts.sysMsg.Value)
            ChatMessage(ChatRole.User, JsonSerializer.Serialize({query=query}))
        ]
        
    let createFixRequest query (pe:RT.Assistant.Plan.Functions.PrologParseError)=
         [
             ChatMessage(ChatRole.System, CodeGenPrompts.fixCodePrompt pe.Code pe.Error)
             ChatMessage(ChatRole.User, JsonSerializer.Serialize({query=query}))
         ]
         
    let MAX_RETRY = 2         
    open RT.Assistant.Plan
    //evaluate LLM generated Prolog code (predicates and query)
    let rec internal runQuery count state query (prologError:RT.Assistant.Plan.Functions.PrologParseError option)=
        async {
            try
                let msgs = match prologError with Some pe -> createFixRequest query pe | _ -> createRequest query
                let! code = sendRequest state msgs
                try
                    let! ans = QueryService.evalQuery state.viewRef code
                    return ans
                with
                | TimeoutException -> return "query timed out"
                | PrologError ex when count < MAX_RETRY ->
                    Log.info $"Prolog err: {ex}. Regenerating code."
                    return! runQuery (count+1) state query  (Some {Code=code.Query; Error=ex})
                | ex -> return raise ex
            with ex ->
                return raise ex
        }
                
    let internal update state cuaMsg =
        async {
            match cuaMsg with
            | Ag_Query (callId,query) ->
                let! result = runQuery 0 state query None                
                state.bus.PostToAgent(Ag_QueryResult (callId,result))
                return state
            | _ -> return state
        }
            
    let start viewRef (bus:WBus<FlowMsg, AgentMsg>) =
        let channel = bus.agentChannel.Subscribe("codegen")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update (State.Create viewRef bus)
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow
