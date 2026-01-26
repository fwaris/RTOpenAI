namespace FsPlaySamples.Cua.Views
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Graphics
open type Fabulous.Maui.View
open type Fabulous.Context
open FsPlaySamples.Cua.Navigation
open FsPlaySamples.Cua

module CredsView =   
    type KeysModel = { settings: Settings.SettingsModel; isActive : bool; hidden : bool}        
    type KeysMsg = BackButtonPressed | Active | InActive | Nop | ToggleVisibilty

    let init settings =
        { settings = settings; isActive=false; hidden = true}, Cmd.none        

    let update nav msg (model: KeysModel) =
        //printfn "%A" msg
        match msg with
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true}, Cmd.none
        | InActive -> {model with isActive = false}, Cmd.none
        | Nop -> model, Cmd.none
        | ToggleVisibilty -> {model with hidden = not model.hidden}, Cmd.none        

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
        Component("Creds") {
            let! settings = EnvironmentObject(Settings.Environment.settingsKey)
            let! model = Context.Mvu(program nav appMsDispatcher, settings) 
            (ContentPage(
                Grid(
                    [Dimension.Absolute 100.0; Dimension.Star; Dimension.Absolute 55.0],
                    [Dimension.Absolute 50.0; Dimension.Absolute 50.0; Dimension.Absolute 50.0; Dimension.Absolute 50.0;]
                    ) {
                    Label($"API Key:")
                        .alignEndHorizontal()
                        .centerVertical()
                        .margin(2.)
                        .gridColumn(0)
                    Entry(settings.ApiKey,(fun v -> settings.ApiKey <- v.Trim(); Nop))
                       .isPassword(model.hidden)
                       .margin(2.)
                       .gridColumn(1)
                    Button((if model.hidden then Icons.visible else Icons.visibility_off), ToggleVisibilty)
                        .font(size=25.0, fontFamily=C.FONT_SYMBOLS)
                        .background(Colors.Transparent)
                        .textColor(Colors.Magenta)
                        .centerHorizontal()        
                        .gridColumn(2)
                })
                    .padding(5.)
            )
                .title("Creds")
                .hasBackButton(true)
                .onNavigatedTo(Active)
                .onNavigatedFrom(InActive)                
        }
