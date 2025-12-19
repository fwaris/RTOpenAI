namespace RT.Assistant
open Fabulous
open type Fabulous.Maui.View
open RT.Assistant.Navigation
open RT.Assistant.WorkFlow

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
            sessionState = RTOpenAI.WebRTC.State.Disconnected
            log = []
            isActive = false
            hybridView = ViewRef<Microsoft.Maui.Controls.HybridWebView>()
            code = {CodeGenResp.Predicates=testConsult; CodeGenResp.Query=testQuery}
            fontSize = 11.0
            flow = None
        }, Cmd.none
        
    let submitCode (code,viewRef) =
        async {
            let temp = PlanPrompts.planTemplate.Value    
            let! str = QueryService.evalQuery viewRef code
            return str
        }
        
    let update nav msg model =
        //Log.info $"%A{msg}"
        match msg with
        | EventError exn -> debug exn.Message; {model with log=exn.Message::model.log}, Cmd.none
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG }, Cmd.none
        | Log_Clear -> { model with log = [] }, Cmd.none
        | Cn_EnsureKey_Start -> model, (if Connect.checkKeys() then Cmd.ofMsg (Cn_StartStop()) else Cmd.ofMsg Settings_Show)
        | Cn_Started _ -> model, Cmd.none
        | Cn_StartStop _ -> model, Cmd.OfAsync.either Connect.startStopConnection model Cn_Set EventError 
        | Cn_Set None -> {model with flow = None }, Cmd.none
        | Cn_Set (Some(sess)) -> {model with flow = Some sess}, Cmd.none
        | WebRTC_StateChanged s -> {model with sessionState = s}, Cmd.none
        | Nop -> model, Cmd.none
        | Settings_Show -> model, Navigation.navigateToSettings nav
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true},Cmd.none
        | InActive -> {model with isActive = false},Cmd.none
        | SubmitCode -> model,Cmd.OfAsync.either submitCode (model.code,model.hybridView) Log_Append EventError
        | SetQuery q -> {model with code = {model.code with Query=q}}, Cmd.none
        | SetConsult q -> {model with code = {model.code with Predicates=q}}, Cmd.none
        | SetCode code -> {model with code = code}, Cmd.none
        | FontLarger -> {model with fontSize = model.fontSize + 1.0}, Cmd.none
        | FontSmaller -> {model with fontSize = model.fontSize - 1.0}, Cmd.none
