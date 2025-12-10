namespace RT.Assistant.Plan
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.Control
open RTOpenAI
open RTOpenAI.Api.Events
open RT.Assistant

module Machine =    
    let PLAN_QUERY_FUNCTION = "planQuery"
    let PLAN_DETAILS_FUNCTION = "planDetails"
    let M_AUDIO = "audio"
    let M_TEXT = "text"
    let FUNCTION_CALL = "function_call"
    let FUNCTION_CALL_OUTPUT = "function_call_output"
    
    type State = {
        initialized : bool
        responses : Set<string>
        currentSession : Session  }      
    with static member Default = {
             initialized=false
             currentSession = Session.Default
             responses = Set.empty}
                            
    let ssInit = State.Default          //initial state for server event handling
        
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
            event_id = Api.Utils.newId()
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
        | Function_call fc when fc.name = PLAN_QUERY_FUNCTION -> fc.arguments
        | _ -> failwith "no query: incorrect response type"
            
    let  getPlanTitle (ev:ResponseOutputItemEvent) =
        match ev.item with
        | Function_call fc when fc.name = PLAN_DETAILS_FUNCTION -> fc.arguments
        |_ -> failwith "no plan title: incorrect response type"
       
    type SE = ServerEvent
    // accepts old state and next event - returns new state
    let update dispatch hybridWebView conn (st:State) ev =
        async {
            match ev with
            | SE.SessionCreated s when not st.initialized -> sendUpdateSession conn s.session; return {st with initialized = true} 
            | SE.SessionCreated s -> return {st with currentSession = s.session }
            | SE.SessionUpdated s -> return {st with currentSession = s.session }
            | SE.ResponseOutputItemDone ev when isRunQuery ev  -> Functions.runQuery 1 dispatch hybridWebView conn ev None |> Async.Start; dispatch (Log_Append(getQuery ev)); return st
            | SE.ResponseOutputItemDone ev when isGetPlanDetails ev -> Functions.getPlanDetails dispatch hybridWebView conn ev; return st
            | SE.ResponseCreated ev -> return dispatch ItemStarted; return {st with responses = Set.add ev.response.id st.responses}
            | SE.ResponseDone ev -> return {st with responses = Set.remove ev.response.id st.responses}
            | SE.ResponseOutputTextDelta ev when st.responses.Contains ev.response_id -> dispatch (ItemAdded ev.delta); return st
            | SE.ResponseOutputAudioDelta _
            | SE.ResponseOutputAudioTranscriptDelta _
            | SE.ResponseFunctionCallArgumentsDelta _ -> return st // suppress logging 'delta' events 
            | other -> (* Log.info $"unhandled event: {other}"; *) return st //log other events
        }
        
    //continuously process server events
    let run hybridWebView (conn:RTOpenAI.Api.Connection) dispatch =
        let comp = 
            conn.WebRtcClient.OutputChannel.Reader.ReadAllAsync()
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.map Api.Exts.toEvent
            |> AsyncSeq.scanAsync (update dispatch hybridWebView conn) ssInit   //handle actual event
            |> AsyncSeq.iter (fun s -> ())
        async {
            match! Async.Catch comp with
            | Choice1Of2 _ -> Log.info "server events completed"
            | Choice2Of2 exn -> Log.exn(exn,"Error: Machine.run")
        }
        |> Async.Start
        