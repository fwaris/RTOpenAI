namespace FsPlaySamples.Cua.Agentic

open FsAICore
open FSharp.Control
open FsPlan
open FsPlay.Abstractions
open RTFlow
open RTFlow.Functions

module PlanAgent =
        
    type internal State= {
        bus     : CuaBus
        runner  : CuaRunner option
    }
    
    /// An implementation of taskRunner function for FsPlan where
    /// the task is performed by sending a message to an agent and waiting for a response.
    let taskRunner (driver:IUIDriver) (bus:CuaBus) (t:FsTask<Cu_Task>) (context:AIContext) : Async<Cu_Task_Output> = async {     
         let n = "task_listener"
         let! dims = driver.currentDimensions()
         let taskContext = {screenDimensions=dims; aiContext=context}
         bus.PostToAgent(Ag_Task_Run (t,taskContext))
         let! msg = bus.AwaitAgentMsg(fun m -> m.IsAg_Plan_DoneTask)
         match msg with
         | Some(Ag_Plan_DoneTask output) -> return output
         | _                             -> return {history=[]; status = Cu_Task_Status.Error "timeout"; usage=Map.empty}
    }  
    
    let startTask (bus: CuaBus) runner= async {
         let! runner = FsPlan.transition runner         
         match runner.current with
         | Some _ -> let! runner = FsPlan.runTask runner
                     bus.PostToAgent(Ag_Plan_Next runner)
         | None -> bus.PostToAgent(Ag_Plan_Done runner)
    }
           
    let internal update (state:State) msg = async {
        match msg with
        | Ag_Plan_Run _ when state.runner.IsSome ->
            Log.warn $"[PlanAgent] plan already running"
            return state
        | Ag_Plan_Run runner ->
            Log.info $"[PlanAgent] starting plan with task {FsPlan.peekNextTasks runner}"
            startTask state.bus runner |> Async.Start
            return {state with runner = Some runner}
        | Ag_Plan_Next runner ->
            Log.info $"[PlanAgent] transitioning to {FsPlan.peekNextTasks runner}"
            startTask state.bus runner |> Async.Start
            return {state with runner = Some runner}
        | Ag_Plan_Done runner ->
            Log.info $"[PlanAgent] done plan"
            return {state with runner = None}
        | _ -> return state
    }

    let start<'t,'o> (bus:CuaBus) =        
        let st0 = {bus=bus; runner=None}
        let channel = bus.agentChannel.Subscribe("plan")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update st0
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow
