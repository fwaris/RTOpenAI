namespace FsAICore

open System.Collections.Generic
open System.Text.Json
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.AI
open Microsoft.Extensions.Configuration

(*
Module and types to isolate the CUA loop processing
*)

[<RequireQualifiedAccess>]
type CallType = 
    | Cua of Action list*FunctionCallContent
    | NonCua of FunctionCallContent
    | Invalid of string*FunctionCallContent
    
type TaskContext = {screenDimensions:int*int; aiContext:AIContext }
    with static member Default = {screenDimensions=0,0; aiContext=AIContext.Default }
    
type CallsResult = {Handled:AIContent list; Pending:CallType list; taskContext:TaskContext}

type Completion = {taskComplete:bool; reason:string}

module CuaLoop = 
    open System
    open FSharp.Control
    
    let MAX_CUA_LOOP = 20
            
    let ignoreCase = StringComparison.CurrentCultureIgnoreCase

    let content chooser (asstResp:ChatMessage option) =
        asstResp
        |> Option.map (fun m -> m.Contents|> Seq.cast<AIContent>)
        |> Option.defaultValue Seq.empty
        |> Seq.choose chooser
        |> Seq.toList
        
    let cuaCalls msgs =  
        content (function :? FunctionCallContent as c when c.Name = "computer" -> Some c |  _ -> None) msgs

    let nonCuaCalls msgs = content (function :? FunctionCallContent as c when c.Name <> "computer" -> Some c |  _ -> None) msgs

    let textContent msgs = 
        content (function :? TextContent as c -> Some c.Text | _ -> None) msgs
        |> Seq.tryHead

    let asstMsg (response:ChatResponse) = 
        response.Messages
        |> Seq.rev
        |> Seq.tryFind (fun m -> m.Role = ChatRole.Assistant)
        |> Option.defaultWith (fun _ -> failwith "Assistant response missing after tool invocation")

    let invokeTool (tool:AITool) (args: IDictionary<string,obj>) = async {
        match tool with 
        | :? AIFunction as fc -> return! fc.InvokeAsync(AIFunctionArguments(args)).AsTask() |> Async.AwaitTask
        | _                   -> return $"called {tool.Name}" :> obj
    }
    
    let chatOptions tools = 
        let opts = ChatOptions()
        opts.ModelId <- Anthropic.SDK.Constants.AnthropicModels.Claude45Sonnet
        opts.Tools <-
            tools
            |> Map.toSeq
            |> Seq.map snd
            |> ResizeArray
    
        opts

    let invokeFunction (tools:ToolCache) (funcCall:FunctionCallContent) = async {
        let! result = 
            match tools |> Map.tryFind (ToolName funcCall.Name) with 
            | Some tool -> invokeTool tool funcCall.Arguments
            | None      -> async{return failwith $"Tool or function named {funcCall.Name} not found in tool cache"}
        return FunctionResultContent(funcCall.CallId,result) :> AIContent
    }

    let validateCuaCall (accCua,accInvalid) (call:FunctionCallContent) = 
        try 
            let actions = 
                Anthropic.ToolUtils.toJsonElement call.Arguments
                |> Option.map Anthropic.Parser.parseActions
                |> Option.defaultWith(fun _ -> failwith $"unable to extract action for 'computer' tool call")

            let unkAction = actions |> List.tryFind _.IsUnknown
            match unkAction with 
            | Some (Anthropic.Unknown msg) -> accCua,(msg,call)::accInvalid            
            | _                            -> (actions |> List.map Anthropic.Parser.mapToUIDriverAction,call)::accCua,accInvalid
        with ex -> 
            accCua,(ex.Message,call)::accInvalid

    let extractFunctions (msg:ChatMessage) =
        let nonCuaCalls = nonCuaCalls (Some msg)
        let cuaCalls = cuaCalls (Some msg)
        let cuaCalls,invalidCalls = (([],[]),cuaCalls) ||> List.fold validateCuaCall
        seq {
            yield! cuaCalls |> Seq.map CallType.Cua
            yield! nonCuaCalls |> Seq.map CallType.NonCua
            yield! invalidCalls |> Seq.map CallType.Invalid
        }
        |> Seq.toList
        
    let mapUsage (usage:UsageDetails) =
        let input = usage.InputTokenCount.GetValueOrDefault() |> int
        let output = usage.OutputTokenCount.GetValueOrDefault() |> int
        let total =  usage.TotalTokenCount.GetValueOrDefault() |> int
        let total = if total < input + output then input + output else total
        {
          Usage.input_tokens = input
          Usage.output_tokens = output
          Usage.total_tokens = total
        }
         
    /// <summary>
    /// This function forms a mutually recursive loop with [<see cref="FsPlaySamples.Cua.Agentic.TaskAgent.handleNonCuaFunctionCalls"/>].<br />
    /// The call to the LLM can generate a response which contains function calls. There are two types of function calls cua and non-cua.<br />
    /// Any non-cua calls are handled by [handleNonCuaFunctionCalls] (but only after any cua calls have been handled first).<br />
    /// The [handleNonCuaFunctionCalls] function internally calls this function to send the response to the LLM (which may in turn generate new function calls). 
    /// </summary>
    let rec sendRequest (postUsage:UsageMap->unit) (taskContext:TaskContext) history = async {
       let cfg = taskContext.aiContext.kernel.GetRequiredService<IConfiguration>()
       let client = AnthropicClient.createClient(cfg)
       let opts = chatOptions taskContext.aiContext.toolsCache
       taskContext.aiContext.optionsConfigurator |> Option.iter (fun c-> c opts)
       Anthropic.ToolUtils.addCuaAnthropicTool taskContext.screenDimensions opts        
       let! resp = client.GetResponseAsync(List.rev history, opts) |> Async.AwaitTask
       let usage = mapUsage resp.Usage
       postUsage (Map.ofList [resp.ModelId,usage])     
       let asstMsg = asstMsg resp
       Some asstMsg |> textContent |> Option.iter (fun t -> debug $"Resp: {t}")
       let funcCalls = extractFunctions asstMsg //there may multiple 'parallel' calls that need to be handled
       let history = asstMsg :: history
       if funcCalls |> List.exists _.IsCua then                                       //handle any cua calls first
           return funcCalls, history                                        
       elif funcCalls |> List.exists (fun x -> x.IsInvalid || x.IsNonCua) then        //handle any non cua / invalid calls
           return! handleNonCuaFunctionCalls postUsage taskContext history funcCalls []
       else 
           return [],history                                                          //no calls in input message
    }    

    ///wrap up any pending calls
    and wrapUpFunctionCalls (taskContext:TaskContext) history (nonCuaCalls:CallType list) (cuaResults:AIContent list) = async {
        let! nonCuaResults = 
            nonCuaCalls
            |> List.choose (function CallType.NonCua f -> Some f | _ -> None)
            |> AsyncSeq.ofSeq
            |> AsyncSeq.mapAsync (invokeFunction taskContext.aiContext.toolsCache)
            |> AsyncSeq.toListAsync
        let invalidCallResults = 
            nonCuaCalls
            |> List.choose (function CallType.Invalid (a,b) -> Some (a,b) | _ -> None)
            |> List.map (fun (m,call) -> FunctionResultContent(call.CallId,m) :> AIContent)
        let content = nonCuaResults @ invalidCallResults @ cuaResults
        let history = if content.IsEmpty then history else ChatMessage(ChatRole.User, content  |> ResizeArray) :: history
        return history
    }
    
    /// <summary>
    /// See [<see cref="FsPlaySamples.Cua.Agentic.TaskAgent.sendRequest"/>]
    /// This should be called after the application has handled the cua calls.<br />
    /// This function will handle any non-cua function calls and send the request to<br />
    /// the LLM to get a response for the next round.<br />
    /// Note: The screenshot of the page - after the cua actions have been taken - is normally included in the cuaResults. 
    /// </summary>
    and handleNonCuaFunctionCalls postUsage (taskContext:TaskContext) history (nonCuaCalls:CallType list) (cuaResults:AIContent list) = async {
        let! history = wrapUpFunctionCalls taskContext history nonCuaCalls cuaResults
        return! sendRequest postUsage taskContext history
    }
    
    let defaultCompletionTestPrompt = """Can the original task be considered complete?
Observe the goal of the original task instructions and the chat history to make the determination.
Answer in the following JSON format:
```
{
    taskComplete : true | false
    reason : "<rationale for the decision>"
}
```
"""
    
    ///Ask a higher model if the CUA task can be considered to be complete
    let isTaskEnded poster (taskContext:TaskContext) (history:ChatMessage list) (completionPrompt : string option)= async {
       let cfg = taskContext.aiContext.kernel.GetRequiredService<IConfiguration>()
       let client = AnthropicClient.createClient(cfg)
       let opts = chatOptions Map.empty
       opts.ModelId <- "claude-opus-4-5-20251101"
       opts.ResponseFormat <- ChatResponseFormat.ForJsonSchema(typeof<Completion>)
       let msg = defaultArg completionPrompt defaultCompletionTestPrompt
       let history = ChatMessage(ChatRole.User,msg)::history
       let! resp = client.GetResponseAsync(List.rev history, opts) |> Async.AwaitTask
       let usage = mapUsage resp.Usage
       poster (Map.ofList [resp.ModelId,usage])        
       let asstMsg = asstMsg resp
       let text = textContent (Some asstMsg) |> Option.defaultWith (fun _ -> failwith "code not found")
       let code = AIUtils.extractCode text
       let comp = JsonSerializer.Deserialize<Completion>(code)
       return comp
    }     
    
    let splitCalls funcCalls = 
        (([],[]),funcCalls)
        ||> List.fold (fun (accCua,accOth) call ->
            match call with 
            | CallType.Cua (a,c) -> (a,c)::accCua,accOth
            | x                  -> accCua,x::accOth)
        