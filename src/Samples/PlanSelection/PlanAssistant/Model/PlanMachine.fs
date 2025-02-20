namespace RT.Assistant.Plan
open System.Text.Json
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
        
    //take an existing session and 'update' it to new settings       
    let reconfigure (s:Session) =
        { s with
            id = None                               //*** set 'id' and 'object' to None when updating an existing session
            object = None   
                                                    // set, unset, or override other fields as needed 
            instructions = Some PlanPrompts.rtInstructions.Value
            tool_choice = Some "auto"
            tools = [
                {
                    ``type`` = "function"
                    name = PLAN_QUERY_FUNCTION
                    description = "Accepts a set of English instructions that are to be converted to a Prolog query to query the plan database. Responds with results of the query"
                    parameters =
                        {
                            ``type`` = "object"
                            properties = Map.ofList ["query", {``type``= "string"; description= Some "detailed steps in English"}] 
                            required = []                            
                        }
                }
                {
                    ``type`` = "function"
                    name = PLAN_DETAILS_FUNCTION
                    description = "Retrieve the details of a plan from the Prolog plan database"
                    parameters =
                        {
                            ``type`` = "object"
                            properties = Map.ofList ["planTitle", {``type``= "string"; description= Some "Plan title or name for which detail is required"}] 
                            required = []                            
                        }
                }                
            ]                
        }
        
    let toUpdateEvent (s:Session) =
        { SessionUpdateEvent.Default with
            event_id = Api.Utils.newId()
            session = s}
        |> SessionUpdate
            
    let sendUpdateSession conn session =
        session
        |> reconfigure
        |> toUpdateEvent
        |> Api.Connection.sendClientEvent conn
            
    let  isRunQuery (ev:ResponseOutputItemDoneEvent) =
        ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_QUERY_FUNCTION

    let isGetPlanDetails (ev:ResponseOutputItemDoneEvent) =
        ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_DETAILS_FUNCTION
        
    let  getQuery (ev:ResponseOutputItemDoneEvent) =
        if ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_QUERY_FUNCTION then
             ev.item.arguments |> Option.defaultValue "no query found"
        else
            "no query: incorrect response type"
            
    let  getPlanTitle (ev:ResponseOutputItemDoneEvent) =
        if ev.item.``type`` = FUNCTION_CALL && ev.item.name = Some PLAN_DETAILS_FUNCTION then
             ev.item.arguments |> Option.defaultValue "no plan title found"
        else
            "no plan title: incorrect response type"               
 
    let  isQueryResult (ev:ResponseOutputItemDoneEvent) =
        ev.item.``type`` = FUNCTION_CALL_OUTPUT && ev.item.name = Some PLAN_QUERY_FUNCTION
        
    let  isPlanDetailsResult (ev:ResponseOutputItemDoneEvent) =
        ev.item.``type`` = FUNCTION_CALL_OUTPUT && ev.item.name = Some PLAN_DETAILS_FUNCTION
        
    let extractAns dispatch (ev:ResponseOutputItemDoneEvent) =
        ev.item.output
        |> Option.map(fun s -> JsonSerializer.Deserialize<Answer>(s))
        |> Option.map(fun a -> [SetAnswer a; Log_Append $"{a}"]|> List.iter dispatch)
        |> Option.defaultWith (fun _ -> dispatch (Log_Append "answer not found"))        
        
    let formatAnswer (ans:Answer) =
        ans.Results
        |> List.map(fun a ->
            a.Plan  
            |> Map.toSeq
            |> Seq.filter (fun (k,_) -> k.Equals("unique_plan_id",System.StringComparison.CurrentCultureIgnoreCase))
            |> Seq.map(fun (k,v) -> $"{k}: {v}")
            |> String.concat "\n")
       
    // accepts old state and next event - returns new state
    let update dispatch hybridWebView conn (st:State) ev =
        async {
            match ev with
            | SessionCreated s when not st.initialized -> sendUpdateSession conn s.session; return {st with initialized = true} 
            | SessionCreated s -> return {st with currentSession = s.session }
            | SessionUpdated s -> return {st with currentSession = s.session }
            | ResponseOutputItemDone ev when isRunQuery ev  -> Functions.runQuery 1 dispatch hybridWebView conn ev None |> Async.Start; dispatch (Log_Append(getQuery ev)); return st
            | ResponseOutputItemDone ev when isQueryResult ev  -> extractAns dispatch ev; return  st            
            | ResponseOutputItemDone ev when isGetPlanDetails ev -> Functions.getPlanDetails dispatch hybridWebView conn ev; return st
            | ResponseOutputItemDone ev when isPlanDetailsResult ev -> return st
            | ResponseCreated ev -> return dispatch ItemStarted; return {st with responses = Set.add ev.response.id st.responses}
            | ResponseDone ev -> return {st with responses = Set.remove ev.response.id st.responses}
            | ResponseTextDelta ev when st.responses.Contains ev.response_id -> dispatch (ItemAdded ev.delta); return st
            | ResponseAudioDelta _
            | ResponseAudioTranscriptDelta _
            | ResponseFunctionCallArgumentsDelta _ -> return st // suppress logging 'delta' events 
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
        