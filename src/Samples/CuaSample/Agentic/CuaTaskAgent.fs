namespace FsPlaySamples.Cua.Agentic

open Microsoft.Extensions.AI
open FsAICore
open FsPlan
open RTFlow
open RTFlow.Functions

module CuaTaskAgent = 
    open System
    open FSharp.Control
    
    let MAX_CUA_LOOP = 20
    
    type internal State = {
        bus: CuaBus
        task : FsTask<Cu_Task> option
        history:ChatMessage list
        cuaLoopCount : int
        usage : UsageMap
    }
        with
            static member Create bus = {
                                bus=bus
                                task=None
                                history=[]
                                cuaLoopCount = 0
                                usage = Map.empty
            }            
                
    let handleRemainingCalls (bus: CuaBus)  context funcCalls = 
        match funcCalls with 
        | [] -> bus.PostToAgent (Ag_Task_Restart context)        //no computer call so restart task  
        | xs -> bus.PostToAgent (Ag_App_ComputerCall (funcCalls,None))
        
    let internal startCuaLoop state context (systemMsg:string) = async {
        let history = [ChatMessage(ChatRole.System,systemMsg)]
        let! funcCalls,history = CuaLoop.sendRequest (Ag_Usage>>state.bus.PostToAgent) context history
        state.bus.PostToAgent(Ag_App_ComputerCall (funcCalls,None))
        return {state with history = history; cuaLoopCount=0}        
    }        
       
    let internal update (state:State) cuaMsg =
        async {
            match cuaMsg with
            | Ag_Task_Run (t,context) when state.task.IsSome ->
                Log.warn $"[CuaTaskAgent] Task already in progress. Ignoring new task"                 
                return state         
            | Ag_Task_Run (t,context) when not t.task.IsCu_Cua ->
                Log.warn $"[CuaTaskAgent] ignoring non-cua task"                 
                return state         
            | Ag_Task_Run (t,context)  ->
                Log.info $"[CuaTaskAgent] Started task {t.id}"
                let! state = startCuaLoop state context t.description
                return {state with usage=Map.empty; task=Some t} //reset usage for a new task
            | Ag_Task_Continue cr when state.cuaLoopCount >= MAX_CUA_LOOP ->
                Log.info $"[CuaTaskAgent] max cua loop count exceeded {state.cuaLoopCount}. Restarting task"
                let pending = cr.pendingCalls
                let handled = cr.results
                let! history = CuaLoop.wrapUpFunctionCalls cr.context state.history pending handled                
                state.bus.PostToAgent (Ag_Task_Restart cr.context)
                return {state with history=history}
            | Ag_Task_Continue cr ->
                let pending = cr.pendingCalls
                let handled = cr.results
                let! funcCalls,history = CuaLoop.handleNonCuaFunctionCalls (Ag_Usage>>state.bus.PostToAgent) cr.context state.history pending handled
                handleRemainingCalls state.bus cr.context funcCalls 
                return {state with history = history; cuaLoopCount = state.cuaLoopCount + 1}
            | Ag_Task_Restart context ->
                let t = state.task |> Option.defaultWith (fun () -> failwith $"[CuaTaskAgent] no task found")
                Log.info $"[CuaTaskAgent] Checking for restart task {t.id}"
                let! compRslt = CuaLoop.isTaskEnded (Ag_Usage>>state.bus.PostToAgent) context state.history None
                if compRslt.taskComplete then
                    Log.info $"Task is deemed to be done. Ending task. Reason:\n{compRslt.reason}"
                    state.bus.PostToAgent Ag_Task_End
                    return state
                else
                    Log.info $"Task is not complete. Restarting task. Reason:\n{compRslt.reason}"
                    return! startCuaLoop state context t.description
            | Ag_Task_End when state.task.IsSome ->
                state.bus.PostToAgent (Ag_Plan_DoneTask {history=state.history; status=Cu_Task_Status.Done; usage=state.usage})
                Log.info $"[CuaTaskAgent] Ending task {state.task |> Option.map (fun x -> x.id.id) |> Option.defaultValue String.Empty}"                
                return {state with task=None}            
            | Ag_Usage usg -> return {state with usage = Usage.combineUsage state.usage usg}
            | _ -> return state
        }
            
    let start (bus: CuaBus) =
        let channel = bus.agentChannel.Subscribe("task")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update (State.Create bus)
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow

