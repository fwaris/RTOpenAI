namespace RT.Assistant.WorkFlow
open RTFlow
type CallId = CallId of string with member this.id with get() = match this with  CallId s -> s

type CodeGenReq = {query:string; callId:CallId}

[<CLIMutable>]
type CodeGenResp =
  {
    predicates: string
    query: string
  }
  with static member Default = { predicates=""; query=""}
  
type QueryResult = {solutions:string list;}
  
  ///Input to Flow
type FlowMsg =
    //app msgs into flow
    | Fl_Start
    | Fl_Terminate of {|abnormal:bool|}
    | Fl_Usage of Map<string,RTFlow.Usage> 

 ///Agent broadcast messages
type AgentMsg =
    //msgs from flow to agents 
    | Ag_FlowError of WErrorType
    | Ag_FlowDone of {|abnormal:bool; |}
    | Ag_Query of CodeGenReq
    | Ag_Prolog of CodeGenResp
    | Ag_PrologAnswer of string
    | Ag_QueryResult of CallId * QueryResult
    | Ag_VoiceToolError of CallId * string
    | Ag_GetPlanDetails of CallId * string
    | Ag_PlanDetails of CallId * string list
    
    with override this.ToString() =
            match this with
            | Ag_FlowError _ -> "Ag_FlowError"
            | Ag_FlowDone _ -> "Ag_FlowDone"
            | Ag_QueryResult _ -> "Ag_QueryResult"
            | Ag_Query _       -> "Ag_Query"
            | Ag_PlanDetails _ -> "Ag_PlanDetails"
            | Ag_GetPlanDetails _ -> "Ag_GetPlanDetails"
            | Ag_Prolog _ -> "Ag_Prolog"
            | Ag_PrologAnswer _ -> "Ag_SummarizedResults"
            | Ag_VoiceToolError _ -> "Ag_VoiceToolError"
