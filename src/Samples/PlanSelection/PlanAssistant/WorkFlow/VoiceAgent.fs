namespace RT.Assistant.WorkFlow

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.Control
open RT.Assistant.Plan
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
    
    //sends 'response.create' to prompt the LLM to generate audio (otherwise it seems to wait).
    let sendResponseCreate conn=
        (ClientEvent.ResponseCreate {ResponseCreateEvent.Default with
                                        event_id = Utils.newId()
                                        //response.modalities = Some [M_AUDIO; M_TEXT]
                                        })
        |> Api.Connection.sendClientEvent conn
                
    let inline sendFunctionResponse conn (callId:CallId) result =
        let rslt = JsonSerializer.Serialize(result)
        let outEv =
            { ConversationItemCreateEvent.Default with
                item = ConversationItem.Function_call_output
                            (ContentFunctionCallOutput.Create callId.id rslt)
            }
            |> ClientEvent.ConversationItemCreate
        Api.Connection.sendClientEvent conn outEv  //send prolog query results (or error)
        sendResponseCreate conn                    //prompt the LLM to respond now
    
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
                description = "Accepts a set of English instructions that are to be converted to a Prolog query to query the plan database. Responds with results of the query"
                parameters = {Parameters.Default with
                                properties = Map.ofList ["query", JsProperty.String {description = Some"detailed steps in English"; enum = None}]
                                required = ["query"]
                             }
            }
            {Tool.Default with
                name = PLAN_DETAILS_FUNCTION
                description = "Plan title or name for which detail is required"
                parameters = {Parameters.Default with
                                properties = Map.ofList ["planTitle", JsProperty.String {description = Some "Plan title or name for which detail is required"; enum = None}]
                                required = ["planTitle"]
                             }
            }
        ]               
                                    
    let updateSession (s:Session) =
        { s with
            id = Skip
            object = Skip
            instructions = Some PlanPrompts.rtInstructions.Value // set, unset, or override other fields as needed 
            tool_choice = Include "auto"            
            tools = Include voiceFunctions
        }
        
    let toUpdateEvent (s:Session) =
        { SessionUpdateEvent.Default with
            event_id = Utils.newId()
            session = s}
        |> ClientEvent.SessionUpdate
            
    let sendUpdateSession conn session =
        session
        |> updateSession
        |> toUpdateEvent
        |> Api.Connection.sendClientEvent conn
            
    let  isRunQuery (ev:ResponseOutputItemEvent) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_QUERY_FUNCTION -> true
        | _ -> false

    let isGetPlanDetails (ev:ResponseOutputItemEvent) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_DETAILS_FUNCTION -> true
        | _ -> false
        
    let  getQuery (ev:ResponseOutputItemEvent) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_QUERY_FUNCTION -> CallId fc.call_id, fc.arguments
        | _ -> failwith "no query: incorrect response type"
            
    let  getPlanTitle (ev:ResponseOutputItemEvent) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_DETAILS_FUNCTION ->
            CallId fc.call_id, fc.arguments
        |_ ->
            failwith "no plan title: incorrect response type"
            
    type SE = ServerEvent
             
    // accepts old state and next event - returns new state
    let updateVoice conn (st: VoState) ev =
        async {
            match ev with
            | SE.SessionCreated s when not st.initialized -> sendUpdateSession conn s.session; return {st with initialized = true} 
            | SE.SessionCreated s -> return {st with currentSession = s.session }
            | SE.SessionUpdated s -> return {st with currentSession = s.session }
            | SE.ResponseOutputItemDone ev when isRunQuery ev  -> st.bus.PostToAgent(Ag_Query(getQuery ev)); return st
            | SE.ResponseOutputItemDone ev when isGetPlanDetails ev -> st.bus.PostToAgent(Ag_GetPlanDetails(getPlanTitle ev)); return st
            | SE.Error e -> Log.error e.error.message; return st
            | other ->  (*Log.info $"unhandled event: {other}";*)  return st //log other events
        }
        
    let startVoice (conn:RTOpenAI.Api.Connection) (bus:WBus<FlowMsg, AgentMsg>) = async {
        let initState = VoState.Create bus
        if conn.WebRtcClient.State.IsDisconnected then
                let keyReq = KeyReq.Default                
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
        | Ag_QueryResult(callId, s) -> sendFunctionResponse st.conn callId s;  return st
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
        