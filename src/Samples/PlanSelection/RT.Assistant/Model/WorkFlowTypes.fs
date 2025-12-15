namespace RT.Assistant.WorkFlow
open RT.Assistant.Plan
open RTFlow
type CallId = CallId of string with member this.id with get() = match this with  CallId s -> s
type CodeGenReq = {query:string; callId:CallId}

type FlowMsg =
    //app msgs into flow
    | Fl_Start
    | Fl_Terminate of {|abnormal:bool|}
    | Fl_Usage of Map<string,RTFlow.Usage> 
    //agent messages flow
    | CGi_Code of CodeGenResp
    | VOi_Query of string
    
type AgentMsg =
    //msgs from flow to agents 
    | Ag_FlowError of WErrorType
    | Ag_FlowDone of {|abnormal:bool; |}
    | Ag_Query of CallId * string
    | Ag_Prolog of CodeGenResp
    | Ag_QueryResult of CallId * string
    | Ag_GetPlanDetails of CallId * string
    | Ag_PlanDetails of CallId * string
    
    with override this.ToString() =
            match this with
            | Ag_FlowError _ -> "Ag_FlowError"
            | Ag_FlowDone _ -> "Ag_FlowDone"
            | Ag_QueryResult _ -> "Ag_QueryResult"
            | Ag_Query _       -> "Ag_Query"
            | Ag_PlanDetails _ -> "Ag_PlanDetails"
            | Ag_GetPlanDetails _ -> "Ag_GetPlanDetails"
            | Ag_Prolog _ -> "Ag_Prolog"
