namespace FsPlaySamples.Cua

open System
open Fabulous
open FsPlay
open FsPlaySamples.Cua.Navigation
open UiOps

module Update =
    
    let initModel settings = 
        {
            mailbox         = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            settings        = settings
            isActive        = false
            interactiveTask = None
            stepping        = false
            summaries       = []     
            pointer         = None
            action          = None
            isOpenSettings  = false
            isOpenNavBar    = false
            flow            = None
            runSteps        = 1
            usage           = Map.empty
            lastError       = None
            driver          = None
        }, Cmd.ofMsg Init
      
    let update nav msg model =
        //Log.info $"%A{msg}"
        match msg with
        | Init ->  model, Cmd.ofMsg PostInit
        | TestSomething -> testSomething model; model, Cmd.none
        | PostInit -> installDriver model, Cmd.none
        | CheckStartFlow -> model, Cmd.ofMsg (if Model.settingsValid() then StartFlow else ViewCreds)
        | StartFlow -> model, Cmd.OfAsync.either UiOps.startStopFlow model SetFlow EventError
        | StopFlow -> model, Cmd.OfAsync.either UiOps.startStopFlow model SetFlow EventError
        | SetFlow f -> {model with flow = f}, Cmd.none
        | CheckPreview b -> model.settings.PreviewClicks <- b; model,Cmd.none
        | ViewCreds -> model, Navigation.navigateToSettings nav
        | ViewSummary -> model, Navigation.navigateToSummaries nav model.summaries      
        | DoneInteractiveTask -> doneTask model
        | Nav msg -> {model with isOpenNavBar=false}, Cmd.ofMsg msg
        | WebviewInteraction e -> model, Cmd.none //Cmd.ofMsg Highlight
        | ToggleSettings -> {model with isOpenSettings = not model.isOpenSettings}, Cmd.none
        | ToggleNavBar -> {model with isOpenNavBar = not model.isOpenNavBar}, Cmd.none
        | EventError exn -> debug exn.Message; {model with lastError=Some exn.Message}, Cmd.none
        | Nop -> model, Cmd.none
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true},Cmd.none
        | InActive -> {model with isActive = false},Cmd.none
        | FromRunningTask (Agentic.FromAgent.Summary (title,summary)) -> {model with summaries = {Summary=summary; Title=title}::model.summaries},Cmd.none
        | FromRunningTask (Agentic.FromAgent.Preview c) -> postMsgDelayed model PreviewClear |> Async.Start; {model with pointer = c.click; action = Some c.action},Cmd.none
        | FromRunningTask (Agentic.FromAgent.PlanDone rnr) -> model, Cmd.ofMsg StopFlow
        | FromRunningTask (Agentic.FromAgent.LoadTask(t,r)) -> loadTask model (t,r)
        | FromRunningTask Agentic.FromAgent.FlowEnded -> model, Cmd.ofMsg StopFlow
        | PreviewClear -> {model with pointer=None}, Cmd.none
        