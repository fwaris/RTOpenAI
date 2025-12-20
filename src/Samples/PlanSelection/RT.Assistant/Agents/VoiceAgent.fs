namespace RT.Assistant.WorkFlow

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.Control
open RT.Assistant.WorkFlow
open RTFlow
open RTFlow.Functions
open RTOpenAI
open RTOpenAI.Api
open RTOpenAI.Events
open RT.Assistant

module VoiceAgent =    
    let PLAN_QUERY_FUNCTION = "planQuery"
    let PLAN_DETAILS_FUNCTION = "planDetails"
    let M_AUDIO = "audio"
    let M_TEXT = "text"
    let FUNCTION_CALL = "function_call"
    let FUNCTION_CALL_OUTPUT = "function_call_output"
    
    type PlanTitle = {planTitle: string}
    
    let sessionAudio =
        {Audio.Default with
            input = Include {AudioInput.Default with
                               transcription = {
                                  language = "en"
                                  model = "gpt-4o-mini-transcribe"
                                  prompt = Some "Expect words related to phone plans"
                               }
                               |> Some
                               |> Include
                             }                                
            }
    
    //sends 'response.create' to prompt the LLM to generate audio (otherwise it seems to wait).
    let sendResponseCreate conn instructions=
        let rc = { ResponseCreate.Default with event_id = Utils.newId()}
        let rc =            
            instructions
            |> Option.map (fun i ->  {rc with response = Include {Response.Default with instructions = Include i }})
            |> Option.defaultValue rc
        rc |> ClientEvent.ResponseCreate |> Api.Connection.sendClientEvent conn
                
    let inline sendFunctionResponse conn (callId:CallId) result =        
        let rslt = JsonSerializer.Serialize(result)
        debug result
        let outEv =
            { ConversationItemCreate.Default with
                item = ConversationItem.Function_call_output
                            (ContentFunctionCallOutput.Create callId.id rslt)
            }
            |> ClientEvent.ConversationItemCreate
        Api.Connection.sendClientEvent conn outEv  //send prolog query results (or error)
        sendResponseCreate conn None               //prompt the LLM to respond now
        
    ///
    let inline sendQueryResponse conn (callId:CallId) (result:QueryResult) =        
        let rslt = JsonSerializer.Serialize(result)
        let output = ConversationItem.Function_call_output
                         (ContentFunctionCallOutput.Create callId.id rslt)
        let outEv =
            { ConversationItemCreate.Default with item = output}                
            |> ClientEvent.ConversationItemCreate
        Api.Connection.sendClientEvent conn outEv  //send prolog query results (or error)
        sendResponseCreate conn   (Some $"{result.solutions.Length} plans found")                 //prompt the LLM to respond now
        
    type VoState = {
        initialized : bool
        currentSession : Session
        bus : WBus<FlowMsg, AgentMsg>
    }
    with static member Create bus = {
             initialized=false
             currentSession = Session.Default
             bus = bus
        }
    
    type FlState = {
        bus : WBus<FlowMsg, AgentMsg>
        conn : Connection
    }
    
    ///functions that the voice api can call
    let voiceFunctions =
        [
            {Tool.Default with
                name = PLAN_QUERY_FUNCTION
                description = "Accepts a set of English instructions that are to be converted to a Prolog query to query the plan database. The Prolog query is executed and the results returned."
                parameters = {Parameters.Default with
                                properties = Map.ofList ["query", JsProperty.String {description = Some"detailed steps in English"; enum = None}]
                                required = ["query"]
                             }
            }
            {Tool.Default with
                name = PLAN_DETAILS_FUNCTION
                description = "Accepts a plan name or title and responds with the dump of the plan fact from the Prolog database"
                parameters = {Parameters.Default with
                                properties = Map.ofList ["planTitle", JsProperty.String {description = Some "Plan title or name for which detail is required"; enum = None}]
                                required = ["planTitle"]
                             }
            }
        ]               
                                    
    let updateSession (s:Session) = // set, unset, or override other fields as needed 
        { s with
            id = Skip
            object = Skip
            audio = Include sessionAudio
            instructions = Some PlanPrompts.voiceInstructions.Value 
            tool_choice = Include "auto"            
            tools = Include voiceFunctions
            expires_at = Skip
        }
        
    let toUpdateEvent (s:Session) =
        { SessionUpdate.Default with
            event_id = Utils.newId()
            session = s}
        |> ClientEvent.SessionUpdate
            
    let sendUpdateSession conn session =
        session
        |> updateSession
        |> toUpdateEvent
        |> Api.Connection.sendClientEvent conn
            
    let  isRunQuery (ev: ResponseOutputItem) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_QUERY_FUNCTION -> true
        | _ -> false

    let isGetPlanDetails (ev: ResponseOutputItem) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_DETAILS_FUNCTION -> true
        | _ -> false
        
    let  getQuery (ev: ResponseOutputItem) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_QUERY_FUNCTION -> {CodeGenReq.callId=CallId fc.call_id; query= fc.arguments}
        | _ -> failwith "no query: incorrect response type"
            
    let  getPlanTitle (ev: ResponseOutputItem) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_DETAILS_FUNCTION ->
            let title = JsonSerializer.Deserialize<PlanTitle>(fc.arguments)
            CallId fc.call_id, title.planTitle
        |_ ->
            failwith "no plan title: incorrect response type"
            
    type SE = ServerEvent
             
    // accepts old state and next event; processes event; returns new state
    let updateVoice conn (st: VoState) ev =
        async {
            match ev with
            | SE.SessionCreated s when not st.initialized -> sendUpdateSession conn s.session; return {st with initialized = true} 
            | SE.SessionCreated s -> return {st with currentSession = s.session }
            | SE.SessionUpdated s -> return {st with currentSession = s.session }
            | SE.ResponseOutputItemDone ev when isRunQuery ev  -> st.bus.PostToAgent(Ag_Query(getQuery ev)); return st
            | SE.ResponseOutputItemDone ev when isGetPlanDetails ev -> st.bus.PostToAgent(Ag_GetPlanDetails(getPlanTitle ev)); return st
            | SE.Error e -> Log.error $"**************> Received realtime API error message: `{e.error.message}`"; return st
            | SE.EventHandlingError (t,msg,j) -> Log.error $"Error when handling event of type {t} - '{msg}'"; Log.error $"{JsonSerializer.Serialize(j)}"; return st
            | SE.UnknownEvent (t,_) -> Log.info $"Unknown event of type {t} received"; return st
            | other -> Log.info $"unhandled event: {other.GetType().Name}";  return st //log other events
        }
        
    let startVoice (conn:RTOpenAI.Api.Connection) (bus:WBus<FlowMsg, AgentMsg>) = async {
        let initState = VoState.Create bus
        if conn.WebRtcClient.State.IsDisconnected then
            let keyReq = {KeyReq.Default with session.audio = Include sessionAudio}
            let! ephemKey = Connection.getEphemeralKey (Settings.Values.openaiKey()) keyReq |> Async.AwaitTask
            do! Connection.connect ephemKey conn |> Async.AwaitTask
        let comp = 
            conn.WebRtcClient.OutputChannel.Reader.ReadAllAsync()
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.map SerDe.toEvent
            |> AsyncSeq.scanAsync (updateVoice conn) initState   //handle actual event
            |> AsyncSeq.iter (fun s -> ())            
        match! Async.Catch comp with
        | Choice1Of2 _ -> Log.info "server events completed"
        | Choice2Of2 exn -> Log.exn(exn,"Error: VoiceAgent"); bus.PostToFlow(W_Err (WE_Exn exn))           
    }
    
    let handleFlow (st:FlState) msg = async {
        match msg with
        | Ag_QueryResult(callId, s) -> sendQueryResponse  st.conn callId s;  return st
        | Ag_PlanDetails (callId, s) -> sendFunctionResponse st.conn callId s;  return st
        | _ -> return st
    }
    
    let startFlow conn (bus:WBus<FlowMsg,AgentMsg>) =
        let channel = bus.agentChannel.Subscribe("voice")
        let st = {conn = conn; bus = bus}
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync handleFlow st
        |> AsyncSeq.iter(fun _ -> ())
        
    let start (conn:RTOpenAI.Api.Connection) (bus:WBus<FlowMsg, AgentMsg>)   =
        async {
            let! a1 = Async.StartChild (startVoice conn bus)  // open the voice and data channels 
            let! a2 = Async.StartChild (startFlow conn bus)   // process flow-related messages
            do! a1
            do! a2             
        }
        |> FlowUtils.catch bus.PostToFlow
        