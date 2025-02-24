namespace RT.Assistant
open System.IO
open Fabulous
open Fabulous.Maui
open type Fabulous.Maui.View
open Microsoft.Maui.Storage
open RT.Assistant.Plan
open RTOpenAI.Api.Events
open RT.Assistant.Navigation

module Update =
    let testConsultBad = """
has_premium_data(plan(_, _, _, features(A))) :-
    member(feature(high_speed_data(_,
                                   unlimited(yes),
                                   premium_data_limited(yes, limit_gb(_))),
                   _),
           A).
"""
    let testQueryBad = "findall(Plan, has_premium_data(Plan), Plans)."
    
    let testConsult = """
valid_plan_for_military_veteran(Title, Lines, Features) :-
    plan(Title, category(military_veteran), lines(Lines), features(Features)),
    member(line(4, monthly_price(MonthPrice), _), Lines),
    % Assuming 'around $100' means a maximum of $100 total monthly price
    MonthPrice =< 120.
"""
    let testQuery = "valid_plan_for_military_veteran(Title,Lines, Features)."
    
    let initModel() = 
        {
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            connection = None
            sessionState = RTOpenAI.WebRTC.State.Disconnected
            log = []
            isActive = false
            conversation = []
            modelId = RTOpenAI.Api.C.OPENAI_RT_MODEL_GPT4O_MINI
            item = ""
            hybridView = ViewRef<Microsoft.Maui.Controls.HybridWebView>()
            code = {CodeGenResp.Predicates=testConsult; CodeGenResp.Query=testQuery}
            fontSize = 11.0
        }, Cmd.none
        
    let submitCode (code,viewRef) =
        async {
            let temp = PlanPrompts.planTemplate.Value    
            let! str = PlanQuery.evalQuery viewRef code
            return str
        }
        
    let update nav msg model =
        //Log.info $"%A{msg}"
        match msg with
        | EventError exn -> debug exn.Message; {model with log=exn.Message::model.log}, Cmd.none
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG }, Cmd.none
        | Log_Clear -> { model with log = [] }, Cmd.none
        | Cn_EnsureKey_Start -> model, Cmd.OfTask.either Connect.lookForKey () Cn_StartStop InputKey
        | InputKey _ -> model, Cmd.ofMsg Settings_Show
        | Cn_Started _ -> model, Cmd.none
        | Cn_StartStop _ -> model, Cmd.OfAsync.either Connect.startStopConnection model Cn_Set EventError 
        | Cn_Set None -> {model with connection = None }, Cmd.none
        | Cn_Set (Some(sess)) -> {model with connection = Some sess}, Cmd.ofMsg (Cn_Connect sess); 
        | Cn_Connect sess -> model, Cmd.OfTask.attempt Connect.connect model EventError
        | WebRTC_StateChanged s -> {model with sessionState = s}, Cmd.none
        | Nop -> model, Cmd.none
        | Settings_Show -> model, Navigation.navigateToSettings nav
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true},Cmd.none
        | InActive -> {model with isActive = false},Cmd.none
        | ItemStarted -> {model with item=""}, Cmd.none
        | ItemAdded txt -> {model with item = model.item + txt}, Cmd.none
        | SubmitCode -> model,Cmd.OfAsync.either submitCode (model.code,model.hybridView) Log_Append EventError
        | SetQuery q -> {model with code = {model.code with Query=q}}, Cmd.none
        | SetConsult q -> {model with code = {model.code with Predicates=q}}, Cmd.none
        | SetCode code -> {model with code = code}, Cmd.none
        | FontLarger -> {model with fontSize = model.fontSize + 1.0}, Cmd.none
        | FontSmaller -> {model with fontSize = model.fontSize - 1.0}, Cmd.none
