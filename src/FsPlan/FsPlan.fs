namespace FsPlan

open System
open FsAICore
open System.Text.Json
open Microsoft.Extensions.AI

type Tid = Tid of string with member this.id with get() = match this with Tid i -> i

type FsTask<'t> = {
    id          : Tid
    description : string
    toolNames   : ToolName list 
    task        : 't
}

type FsTransition = {
    tid : Tid
    prompt : string
    toolNames : ToolName list
    targetTids : Tid list
}

type FsGraph = {root:Tid; transitions:Map<Tid,FsTransition>}

[<RequireQualifiedAccess>]
type FsPlanFlow =
    | Sequential of Tid list
    | Graph of FsGraph
    
type FsPlan<'t> = {
    tasks : Map<Tid,FsTask<'t>>
    flow : FsPlanFlow
}
    
[<ReferenceEquality>]
type Runner<'t,'o> =
    {
        context : AIContext
        plan  : FsPlan<'t>
        runTask: FsTask<'t> -> AIContext -> Async<'o>
        taskOutputForTransition: ('o -> string) option
        metricsCollector : (Map<string,UsageDetails> -> unit) option
        completed : (Tid*'o) list
        current: Tid option
    }
    
type TransitionPromptInput = {
    taskOutput : string
    targets : {|id:string; description:string|} list
}

type TransitionPromptOutput = {
    id : string
    justification : string
}

module FsPlan =
    
    let private findDuplicates (xs:Tid list) = 
        let nonUnique = xs |> List.countBy _.id |> List.filter (fun (id,c) -> c > 1)
        if nonUnique.Length > 0 then
            Some $"Duplicate IDs {nonUnique}"
        else
            None
            
    let private ensureUniqueTargets (xs: FsTransition list) =
        xs
        |> List.choose (fun x -> findDuplicates x.targetTids |> Option.map (fun y->x.tid,y))
        |> List.tryHead
        |> Option.map (fun (e,id) -> failwith $"transition for {id} error: {e}")
        |> ignore
        
    let ensureValidTargets (g:FsGraph) (txns:FsTransition list)=
        let missedTargets = 
            txns
            |> List.collect (fun txn ->
                txn.targetTids
                |> List.choose (fun target ->
                    match g.transitions |> Map.tryFind target with
                    | Some  _ -> None
                    | None    -> Some ($"{txn.tid} -> {target} missing")
                    ))
        if missedTargets.Length > 0 then
            failwith $"No task found for the following targets {missedTargets}" 
            
    let validateFlow  = function
        | FsPlanFlow.Sequential xs -> match findDuplicates xs with Some e -> failwith e | None -> ()
        | FsPlanFlow.Graph g -> //transitions to self allowed
            let transitions = (Map.toList g.transitions |> List.map snd)
            ensureUniqueTargets transitions
            ensureValidTargets g transitions
            if Map.containsKey g.root g.transitions |> not then failwith $"root id '{g.root}' not found in flow graph"
            
    let allFlowIds = function
        | FsPlanFlow.Sequential xs -> set xs
        | FsPlanFlow.Graph g ->
                let rec loop acc xs =
                    match xs with
                    | [] -> acc
                    | x::rest -> loop (Set.union acc (set x)) rest
                loop (set [g.root]) (g.transitions |> Map.toList |> List.map snd |> List.map (fun txn -> txn.tid::txn.targetTids))
                
    let validateTaskIds (plan:FsPlan<_>) = 
        let allFlowTids = allFlowIds plan.flow
        let allTasksIds = plan.tasks |> Map.toList |> List.map fst |> set
        let notInFlow = Set.difference allTasksIds allFlowTids
        let notInTask = Set.difference allFlowTids allTasksIds
        if notInFlow.Count > 0 then
            let msg = $"The following task ids are not referenced in the flow {notInFlow}" 
            Log.error msg
            failwith msg
        if notInTask.Count > 0 then
            let msg = $"The following task ids are not referenced in the flow {notInFlow}"
            Log.error msg
            failwith msg

    let validatePlan (plan:FsPlan<_>) =
        validateFlow plan.flow
        validateTaskIds plan
                        
    let private transitionNextSeq runner xs =
        match runner.completed with
        | [] -> List.tryHead xs
        | (x,_)::_ ->
            match xs |> List.skipWhile (fun y -> y <> x) with
            | _::y::_ -> Some y
            | _ -> None
            
    let inline internal jumpFrom (runner:Runner<_,_>) (srcTid,output) (g:FsGraph)= async {
        let outStr =
            match runner.taskOutputForTransition with
            | Some f -> f output
            | None ->
                let serOpts = runner.context.jsonSerializationOptions |> Option.defaultValue openAIResponseSerOpts
                JsonSerializer.Serialize(output,options=serOpts)
        match g.transitions |> Map.tryFind srcTid with 
        | None -> return None //no transitions from last completed state
        | Some txn when txn.targetTids.IsEmpty -> return None
        | Some txn ->
            let data = {
                 taskOutput = outStr
                 targets = txn.targetTids |> List.map (fun id -> {|id=id.id; description=runner.plan.tasks.[id].description|})   
            }            
            let msgs = seq  {
                ChatMessage(role = ChatRole.System, content= txn.prompt)
                ChatMessage(role = ChatRole.User, content = JsonSerializer.Serialize(data))                
            }
            let! resp,usage = AIUtils.sendRequest<TransitionPromptOutput> 2 runner.context txn.toolNames msgs
            runner.metricsCollector |> Option.iter(fun x -> x usage)
            let targetTid = resp.id |> checkEmpty |> Option.map Tid
            match targetTid with
            | None -> Log.info $"No transition output from {txn.tid}"
            | Some target -> Log.info $"Txn: {txn.tid.id}->{target.id} - Justification:\r\n{resp.justification}"
            return targetTid
    }
                                                                                                                 
    let inline internal transitionGraph runner (g:FsGraph) = async {
        match runner.completed with
        | [] -> return Some g.root
        | (x,o)::_ -> return! jumpFrom runner (x,o) g
    } 
                    
    let transition runner = async {
        let! tid = 
            match runner.plan.flow with
            | FsPlanFlow.Sequential xs -> async {return transitionNextSeq runner xs}
            | FsPlanFlow.Graph graph -> transitionGraph runner graph
        return {runner with current = tid}
    }
    
    let runTask<'t,'o> (runner:Runner<'t,'o>) = async {
        match runner.current with
        | Some tid ->
            let t = runner.plan.tasks.[tid]
            let! o = runner.runTask t runner.context
            let runner = {runner with completed = (tid,o)::runner.completed}
            return! transition runner
        | None -> return runner
    }

    ///next task(s), if any, that runner can transition to, from current state
    let peekNextTasks (runner:Runner<_,_>) =
        match runner.plan.flow with
        | FsPlanFlow.Sequential xs -> [transitionNextSeq runner xs] |> List.choose id |> set
        | FsPlanFlow.Graph g ->
            match runner.completed with
            | [] -> set [g.root]
            | x::_ -> Map.tryFind (fst x) g.transitions
                      |> Option.map (fun txns -> txns.targetTids |> set)
                      |> Option.defaultValue Set.empty                      
    
    let createRunner context plan taskRunner =
        validatePlan plan
        {
            context = context
            runTask = taskRunner
            plan = plan
            taskOutputForTransition = None
            metricsCollector = None
            completed = []
            current = None
        }