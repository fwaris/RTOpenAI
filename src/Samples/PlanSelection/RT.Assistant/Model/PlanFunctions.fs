namespace RT.Assistant.Plan
open System.Text.Json
open System.Text.Json.Nodes
open FSharp.Control
open Fabulous.Maui.KeyboardAccelerator
open RTOpenAI
open RTOpenAI.Api.Events
open Microsoft.Maui.ApplicationModel
open RT.Assistant


module Functions =

    let sendInitResp conn = 
        (ClientEvent.ResponseCreate {ResponseCreateEvent.Default with
                                        event_id = Api.Utils.newId()
                                        response.instructions = Some "briefly introduce yourself"
                                        //response.modalities = Some [M_AUDIO; M_TEXT]
                                        })
        |> Api.Connection.sendClientEvent conn

    //sends 'response.create' to prompt the LLM to generate audio (otherwise it seems to wait).
    let sendResponseCreate conn=
        (ClientEvent.ResponseCreate {ResponseCreateEvent.Default with
                                        event_id = Api.Utils.newId()
                                        //response.modalities = Some [M_AUDIO; M_TEXT]
                                        })
        |> Api.Connection.sendClientEvent conn

    
        
    let inline sendFunctionResponse conn (ev:ResponseOutputItemDoneEvent) result =
        let outEv =
            { ConversationItemCreateEvent.Default with
                item =
                      { ConversationItem.Default with
                          ``type`` = ConversationItemType.Function_call_output
                          call_id = Some ev.item.call_id
                          output = Some (JsonSerializer.Serialize(result))                                      
                      }
            }
            |> ConversationItemCreate
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
    let getPlanDetails dispatch hybridWebView conn (ev:ResponseOutputItemDoneEvent) =
        async {
            try
                let title = ev.item.arguments |> Option.map (getArg "planTitle") |> Option.defaultWith (fun _ -> failwith "function call argument not found")
                let prolog = {Predicates=""; Query= $"plan('{title}',Category,Lines,Features)."}
                dispatch (SetCode prolog)
                let! ans = PlanQuery.evalQuery hybridWebView prolog
                dispatch (Log_Append ans)
                sendFunctionResponse conn ev ans
            with ex ->
                Log.exn (ex,"getPlanDetails")
        }
        |> Async.Start
        
    //evaluate LLM generated Prolog code (predicates and query)
    let rec runQuery count dispatch hybridWebView conn (ev:ResponseOutputItemDoneEvent) (prologError:PrologParseError option)=
        async {
            try 
                let query = ev.item.arguments |> Option.map (getArg "query") |> Option.defaultWith (fun _ -> failwith "function call argument not found")
                let key = RT.Assistant.Settings.Environment.apiKey()
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
            with exn ->
                dispatch (Log_Append exn.Message)
                Log.exn (exn,"Machine.runQuery")
                sendFunctionResponse conn ev "unexpected error"
        }        
