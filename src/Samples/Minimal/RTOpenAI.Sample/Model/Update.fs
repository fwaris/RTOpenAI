namespace RTOpenAI.Sample
open Fabulous
open type Fabulous.Maui.View
open RTOpenAI.Api.Events
open RTOpenAI.Sample.Navigation

module Update =
    let initModel() = 
        {
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            connection = None
            sessionState = RTOpenAI.WebRTC.State.Disconnected
            log = []
            isActive = false
            conversation = []
            modelId = RTOpenAI.Api.C.OPENAI_RT_MODEL_GPT4O_MINI
        }, Cmd.none
        
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

       