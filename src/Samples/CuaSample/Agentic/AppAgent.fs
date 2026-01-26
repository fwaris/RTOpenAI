namespace FsPlaySamples.Cua.Agentic

open FsAICore
open FSharp.Control
open FsPlan
open FsPlay.Abstractions
open Microsoft.Extensions.AI
open RTFlow
open RTFlow.Functions

//sniff agent messages and notify app for selected ones
module AppAgent =
    type internal State = {
        previewActions : bool
        aiContext : AIContext
        poster : FromAgent -> unit//background messages
        bus : CuaBus
        driver : IUIDriver
        task : FsTask<Cu_Task> option
    }
        with member this.Send (msg:FromAgent) =  this.poster msg
        
    let imageContent (snapshot:byte[] ,mediaType:string) = DataContent(snapshot, mediaType) :> AIContent
        
    let mapPreview a =
        match a with 
        | Action.Click c -> FromAgent.Preview {click=Some(c.x,c.y); action=a.toString()}
        | _              -> FromAgent.Preview{click=None; action=a.toString()}

    let internal performActions (ss:State) cuaActions = async {
        let! results = 
            ([],cuaActions |> AsyncSeq.ofSeq) 
                 ||> AsyncSeq.foldAsync(fun acc (acts,fc:FunctionCallContent) -> async {
                    let previews = acts |> List.map mapPreview
                    if ss.previewActions && not previews.IsEmpty then
                          previews |> List.iter ss.Send 
                          do! Async.Sleep 1000                            //wait for the UI to display click                        
                    do! acts |> AsyncSeq.ofSeq |> AsyncSeq.iterAsync (fun a -> async {
                              let a =
                                  match a with
                                  | Action.Keypress k when k.keys = [K.Alt;K.ArrowLeft] ->
                                      Action.Click {|button=Button.Back; x=0; y=0|}
                                  | a -> a
                              Log.info (a.toString())
                              do! FsPlay.Actions.perform ss.driver a
                         })
                    let callRslt = FunctionResultContent(fc.CallId, "{ \"status\": \"done\" }") :> AIContent //need response for each call
                    return (callRslt::acc)
                 })
        do! Async.Sleep 1500
        let! (snapshot,dims,mediaType) = ss.driver.snapshot()
        let screenContent = imageContent (snapshot,mediaType) 
        let results = results @ [screenContent] //append latest screenshot after performing actions
        return results,dims
    }
    
    let internal update (state:State) msg = async {
        match msg with
        | Ag_App_ComputerCall (callTypes,msg) ->
            let (cuaCalls,pendingCalls) = CuaLoop.splitCalls callTypes
            let! results,dims = performActions state cuaCalls
            let ctx = {screenDimensions=dims; aiContext=state.aiContext}            
            state.bus.PostToAgent(Ag_Task_Continue {|results=results; pendingCalls=pendingCalls; context=ctx |})                                 
            return state 
        | Ag_Task_Run (t,context) when state.task.IsSome ->
            Log.warn $"[AppAgent] interactive task already in progress. Ignoring new task"                 
            return state         
        | Ag_Task_Run (t,context) when t.task.IsCu_Interactive ->
            Log.info $"[AppAgent] starting interactive task"                                 
            let target,desc = match t.task with | Cu_Interactive (t,d) -> t,d | _ -> failwith "no expected"
            match target with
            | Some (Target t) -> do! state.driver.start t
            | None -> ()
            state.Send(FromAgent.LoadTask(target,desc))
            return {state with task = Some t}
        | Ag_Task_End when state.task.IsSome ->
            Log.info $"[AppAgent] interactive task ended"
            state.bus.PostToAgent(Ag_Plan_DoneTask {history=[]; status=Done; usage=Map.empty})
            return {state with task=None}
        | Ag_Plan_Done p ->
            state.poster (FromAgent.PlanDone p)
            return state
        | Ag_FlowDone _ ->
            state.Send(FromAgent.FlowEnded)
            return state
        | _ -> return state
        
    }

    let start previewActions aiContext (driver:IUIDriver) poster (bus: CuaBus) =
        let st0 = {
            poster = poster
            bus=bus
            driver = driver
            task=None
            previewActions = previewActions
            aiContext = aiContext
        }
        let channel = bus.agentChannel.Subscribe("app")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update st0
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow