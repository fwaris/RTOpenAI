namespace RTOpenAI.Sample.Views
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Graphics
open type Fabulous.Maui.View
open type Fabulous.Context
open RTOpenAI.Sample
open RTOpenAI.Sample.Navigation

module Settings =   
    type SettingsModel = { settings: RTOpenAI.Sample.Settings.Settings; isActive : bool; hidden : bool}        
    type SettingsMsg = BackButtonPressed | Active | InActive | Nop | ToggleVisbilty
    
    let init settings =
        { settings = settings; isActive=false; hidden = true}, Cmd.none        

    let update nav msg (model:SettingsModel) =
        //printfn "%A" msg
        match msg with
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true}, Cmd.none
        | InActive -> {model with isActive = false}, Cmd.none
        | Nop -> model, Cmd.none
        | ToggleVisbilty -> {model with hidden = not model.hidden}, Cmd.none        

    let subscribe (appMsgDispatcher: IAppMessageDispatcher) model =
        let localAppMsgSub dispatch =
            appMsgDispatcher.Dispatched.Subscribe(fun msg ->
                match msg with
                | AppMsg.BackButtonPressed -> dispatch BackButtonPressed)

        [ if model.isActive then
              [ nameof localAppMsgSub ], localAppMsgSub ]

    let program nav appMsgDispatcher =
        Program.statefulWithCmd init (update nav)
        |> Program.withSubscription(subscribe appMsgDispatcher)
       
                    
    let view nav appMsDispatcher=
        Component("Settings") {
            let! settings = EnvironmentObject(Settings.Environment.settingsKey)
            let! model = Context.Mvu(program nav appMsDispatcher, settings) 
            (ContentPage(
                Grid([Dimension.Stars 3.; Dimension.Stars 5.; Dimension.Stars 1.5],[Dimension.Absolute 45.0;]) {
                    Label($"OpenAI Key:")
                        .gridColumn(0)
                        .alignEndHorizontal()
                        .centerVertical()
                        .margin(2.)
                    Entry(settings.ApiKey,(fun v -> settings.ApiKey <- v; Nop))
                       .gridColumn(1)
                       .isPassword(model.hidden)
                       .margin(2.)
                    Button((if model.hidden then Icons.visible else Icons.visibility_off),ToggleVisbilty)
                        .gridColumn(2)
                        .font(size=25.0, fontFamily="MaterialSymbols")
                        .background(Colors.Transparent)
                        .textColor(Colors.Magenta)
                        .centerHorizontal()        
                })
                    .padding(5.)
            )
                .title("Settings")
                .hasBackButton(true)
                .onNavigatedTo(Active)
                .onNavigatedFrom(InActive)                
        }
