namespace RT.Assistant.WorkFlow

open System.Text.Json
open FSharp.Control
open RT.Assistant.Plan
open RT.Assistant.WorkFlow
open RTFlow
open RTFlow.Functions
open RTOpenAI
open RTOpenAI.Api
open RTOpenAI.Api.Events
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
                                        event_id = Api.Utils.newId()
                                        //response.modalities = Some [M_AUDIO; M_TEXT]
                                        })
        |> Api.Connection.sendClientEvent conn
                
    let inline sendFunctionResponse conn (callId:CallId) result =
        let outEv =
            { ConversationItemCreateEvent.Default with
                item =
                      { ConversationItem.Default with
                          ``type`` = ConversationItemType.Function_call_output
                          call_id = Some callId.id
                          output = Some (JsonSerializer.Serialize(result))                                      
                      }
            }
            |> ConversationItemCreate
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
            id = None                                            //*** set 'id' and 'object' to None when updating an existing session
            object = None                                                       
            instructions = Some PlanPrompts.rtInstructions.Value // set, unset, or override other fields as needed 
            tool_choice = Some "auto"            
            tools = voiceFunctions
        }
        
    let toUpdateEvent (s:Session) =
        { SessionUpdateEvent.Default with
            event_id = Api.Utils.newId()
            session = s}
        |> SessionUpdate
            
    let sendUpdateSession conn session =
        session
        |> updateSession
        |> toUpdateEvent
        |> Api.Connection.sendClientEvent conn
            
    let  isRunQuery (ev:ResponseOutputItemDoneEvent) =
        ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_QUERY_FUNCTION

    let isGetPlanDetails (ev:ResponseOutputItemDoneEvent) =
        ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_DETAILS_FUNCTION
        
    let  getQuery (ev:ResponseOutputItemDoneEvent) =
        if ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_QUERY_FUNCTION then
             CallId ev.item.call_id , ev.item.arguments |> Option.defaultValue "no query found"
        else
            failwith "no query: incorrect response type"
            
    let  getPlanTitle (ev:ResponseOutputItemDoneEvent) =
        if ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_DETAILS_FUNCTION then
             CallId ev.item.call_id, ev.item.arguments |> Option.defaultValue "no plan title found"
        else
            failwith "no plan title: incorrect response type"               
             
    // accepts old state and next event - returns new state
    let updateVoice conn (st: VoState) ev =
        async {
            match ev with
            | SessionCreated s when not st.initialized -> sendUpdateSession conn s.session; return {st with initialized = true} 
            | SessionCreated s -> return {st with currentSession = s.session }
            | SessionUpdated s -> return {st with currentSession = s.session }
            | ResponseOutputItemDone ev when isRunQuery ev  -> st.bus.PostToAgent(Ag_Query(getQuery ev)); return st
            | ResponseOutputItemDone ev when isGetPlanDetails ev -> st.bus.PostToAgent(Ag_GetPlanDetails(getPlanTitle ev)); return st
            | Error e -> Log.error e.error.message; return st
            | other ->  (*Log.info $"unhandled event: {other}";*)  return st //log other events
        }
        
    let startVoice (conn:RTOpenAI.Api.Connection) (bus:WBus<FlowMsg, AgentMsg>) = async {
        let initState = VoState.Create bus
        if conn.WebRtcClient.State.IsDisconnected then
                let keyReq = {KeyReq.Default with model = RTOpenAI.Api.C.OPENAI_RT_MODEL_GPT_REALTIME}                
                let! ephemKey = Connection.getEphemeralKey (Settings.Values.openaiKey()) keyReq |> Async.AwaitTask
                do! Connection.connect ephemKey conn |> Async.AwaitTask
        let comp = 
            conn.WebRtcClient.OutputChannel.Reader.ReadAllAsync()
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.map Api.Exts.toEvent
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
        