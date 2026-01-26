namespace FsPlaySamples.Cua
open Fabulous
open FsPlan
open FsPlay.Abstractions
open FsPlaySamples.Cua.Agentic
open RTFlow

exception InputKeyExn

type WebviewScroll = {x:float; y:float;}

type RunStatus = Init | Stepping | Running | Finished

type ArticleSummary =
    {
       Title         : string
       Summary       : string
    }
    with
        static member Default = {
            Title = ""
            Summary = ""
        }

type Model = 
    {
        mailbox         : System.Threading.Channels.Channel<Msg> //background messages
        settings        : Settings.SettingsModel        
        isActive        : bool
        interactiveTask : string option        
        pointer         : (int*int) option
        action          : string option
        stepping        : bool
        summaries       : ArticleSummary list
        isOpenSettings  : bool
        isOpenNavBar    : bool
        runSteps        : int
        flow            : IFlow<FlowMsg,AgentMsg> option
        usage           : FsAICore.UsageMap
        lastError       : string option
        driver          : IUIDriver option        
    }

and Msg =
    | CheckStartFlow
    | StartFlow
    | StopFlow
    | SetFlow of IFlow<FlowMsg,AgentMsg> option
    | EventError of exn        
    | CheckPreview of bool
    | DoneInteractiveTask
    | Nop
    | TestSomething
    | ToggleSettings
    | ToggleNavBar
    | Init
    | PostInit
    | ViewCreds
    | Nav of Msg
    | ViewSummary   
    | Active
    | InActive
    | BackButtonPressed
    | PreviewClear
    | WebviewInteraction of FsPlay.WvEvent
    | FromRunningTask of FromAgent

module Model =

    let webviewCache = ViewRef<Microsoft.Maui.Controls.WebView>()
    let webviewWrapper = lazy(FsPlay.Service.createWebViewWrapper(webviewCache.Value))

    
    let settingsValid () =
        Settings.Environment.apiKey() |> isEmpty |> not
        
    let rec startPlan driver previewActions poster = async {
        let cfg = Utils.configuration.Value
        cfg.[FsAICore.ConfigKeys.ANTHROPIC_API_KEY] <- Settings.Environment.apiKey()
        let context : FsAICore.AIContext = {
            backend = FsAICore.AIBackend.AnthropicLike
            kernel = Utils.services.Value
            jsonSerializationOptions = None
            toolsCache =  FsAICore.Toolbox.makeTools None [ Agentic.ArticleTools(poster) ]
            optionsConfigurator = None            
        }
        let iflow,bus = Agentic.StateMachine.create previewActions context driver poster
        let taskRunner = PlanAgent.taskRunner driver bus 
        let runner : FsPlan.Runner<Cu_Task,Cu_Task_Output> = FsPlan.createRunner context Plans.testPlan taskRunner
        iflow.PostToFlow(Fl_Start)
        do! Async.Sleep 500
        iflow.PostToAgent(Ag_Plan_Run runner)
        return iflow        
    }
    
