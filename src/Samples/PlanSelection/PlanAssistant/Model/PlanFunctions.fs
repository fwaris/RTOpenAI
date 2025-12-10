namespace RT.Assistant.Plan
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Json.Serialization
open FSharp.Control
open Fabulous.Maui.KeyboardAccelerator
open RTOpenAI
open RTOpenAI.Events
open Microsoft.Maui.ApplicationModel
open RT.Assistant


module Functions =

    //sends 'response.create' to prompt the LLM to generate audio (otherwise it seems to wait).
    let sendResponseCreate conn=
        (ClientEvent.ResponseCreate {ResponseCreateEvent.Default with
                                        event_id = Utils.newId()
                                        //response.modalities = Some [M_AUDIO; M_TEXT]
                                        })
        |> Api.Connection.sendClientEvent conn
        
    let inline sendFunctionResponse conn (ev:ResponseOutputItemEvent) result =
        let callId = match ev.item with Function_call i -> i.call_id | _ -> failwith "not expected"
        let outEv =
            { ConversationItemCreateEvent.Default with
                item = ConversationItem.Function_call_output (ContentFunctionCallOutput.Create callId (JsonSerializer.Serialize(result)))
            }
            |> ClientEvent.ConversationItemCreate
        Api.Connection.sendClientEvent conn outEv  //send prolog query results (or error)
        sendResponseCreate conn                    //prompt the LLM to respond now
    
    let MAX_RETRY = 2
    
    type PrologParseError = {
        Error : string
        Code : string
    }
    
    let getArg (argName:string) (jsonStr:string) =    
        let jargs = JsonSerializer.Deserialize<JsonObject>(jsonStr)
        jargs.[argName].ToString()
    
    //get details for a single plan
    let getPlanDetails dispatch hybridWebView conn (ev:ResponseOutputItemEvent) =
        async {
            try
                match ev.item with
                | Function_call fc ->
                    let title = fc.arguments |> getArg "planTitle" 
                    let prolog = {Predicates=""; Query= $"plan('{title}',Category,Lines,Features)."}
                    dispatch (SetCode prolog)
                    let! ans = PlanQuery.evalQuery hybridWebView prolog
                    dispatch (Log_Append ans)
                    sendFunctionResponse conn ev ans
                | _ -> failwith "function call expected"
            with ex ->
                Log.exn (ex,"getPlanDetails")
        }
        |> Async.Start
        
    //evaluate LLM generated Prolog code (predicates and query)
    let rec runQuery count dispatch hybridWebView conn (ev:ResponseOutputItemEvent) (prologError:PrologParseError option)=
        async {
            try
                match ev.item with
                | Function_call fc ->
                    let query = fc.arguments |> getArg "query"
                    let key = RT.Assistant.Settings.Values.openaiKey()
                    let parms = {Model=AICore.models.gpt_4o; Key=key}
                    let! ans =
                        match prologError with
                        | None -> AICore.getOutput parms PlanPrompts.sysMsg.Value query typeof<CodeGenResp>
                        | Some err -> AICore.getOutput {parms with Model=AICore.models.o3_mini} (PlanPrompts.fixCodePrompt err.Code err.Error) query typeof<CodeGenResp>
                    let codeGen = JsonSerializer.Deserialize<CodeGenResp>(ans.Content)                
                    dispatch (SetCode codeGen)
                    dispatch (Log_Append $"Code: {codeGen}")
                    try 
                        let! result = PlanQuery.evalQuery hybridWebView codeGen
                        dispatch (Log_Append $"Code eval: {result}")
                        let result = $"""
Code Evaluated```
{ans}
```

Evaluation Results:
{result}
    """
                        sendFunctionResponse conn ev result
                    with
                    | TimeoutException -> sendFunctionResponse conn ev "query timed out"
                    | PrologError ex when count < MAX_RETRY ->
                        Log.info $"Prolog err: {ex}. Regenerating code."
                        return! runQuery (count+1) dispatch hybridWebView conn ev (Some {Error=ex; Code=ans.Content})
                    | ex -> raise ex
                | _ -> failwith "function call expected"
            with exn ->
                dispatch (Log_Append exn.Message)
                Log.exn (exn,"Machine.runQuery")
                sendFunctionResponse conn ev "unexpected error"
        }        
