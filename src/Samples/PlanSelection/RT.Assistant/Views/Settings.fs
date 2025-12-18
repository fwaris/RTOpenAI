namespace RT.Assistant.Views
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Graphics
open type Fabulous.Maui.View
open type Fabulous.Context
open RTOpenAI
open RT.Assistant.Navigation
open RT.Assistant

module Settings =   
    type SettingsModel = { settings: Settings.SettingsModel; isActive : bool; hidden : bool}        
    type SettingsMsg =
        | BackButtonPressed
        | Active
        | InActive
        | Nop
        | ToggleVisibility
        | Set_Key_Anthropic of string
        | Set_Key_OpenAI of string
        | Set_UseCodex of bool

    let init settings =
        { settings = settings; isActive=false; hidden = true}, Cmd.none        

    let update nav msg (model:SettingsModel) =
        //printfn "%A" msg
        match msg with
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true}, Cmd.none
        | InActive -> {model with isActive = false}, Cmd.none
        | Nop -> model, Cmd.none
        | ToggleVisibility -> {model with hidden = not model.hidden}, Cmd.none
        | Set_Key_Anthropic k -> model.settings.AnthropicKey <- k; model,Cmd.none
        | Set_Key_OpenAI k -> model.settings.OpenAIKey <- k; model,Cmd.none
        | Set_UseCodex v-> model.settings.UseCodex <- v; model,Cmd.none

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
            let! settings = EnvironmentObject(Settings.Values.settingsKey)
            let! model = Context.Mvu(program nav appMsDispatcher, settings) 
            (ContentPage(
                Grid([Dimension.Absolute 120.0; Dimension.Star; Dimension.Absolute 55.0],
                     [Dimension.Absolute 50.0;Dimension.Absolute 50.0;Dimension.Absolute 50.0;]) {
                    Label($"OpenAI Key:")
                        .gridColumn(0)
                        .alignEndHorizontal()
                        .centerVertical()
                        .margin(2.)
                    Entry(settings.OpenAIKey,Set_Key_OpenAI)
                       .gridColumn(1)
                       .isPassword(model.hidden)
                       .margin(2.)
                    Button((if model.hidden then Icons.visible else Icons.visibility_off), ToggleVisibility)
                        .gridColumn(2)
                        .font(size=25.0, fontFamily=C.FONT_SYMBOLS)
                        .background(Colors.Transparent)
                        .textColor(Colors.Magenta)
                        .centerHorizontal()        
                    Label($"Antrhopic Key:")
                        .gridRow(1)
                        .gridColumn(0)
                        .alignEndHorizontal()
                        .centerVertical()
                        .margin(2.)
                    Entry(settings.AnthropicKey,Set_Key_Anthropic)
                       .gridRow(1)
                       .gridColumn(1)
                       .isPassword(model.hidden)
                       .margin(2.)
                    Label($"Use Codex for code gen:")
                        .gridRow(2)
                        .gridColumn(0)
                        .alignEndHorizontal()
                        .centerVertical()
                        .margin(2.)
                    CheckBox(settings.UseCodex,Set_UseCodex)
                       .gridRow(2)
                       .gridColumn(1)
                       .margin(2.)
                })
                    .padding(5.)
            )
                .title("Settings")
                .hasBackButton(true)
                .onNavigatedTo(Active)
                .onNavigatedFrom(InActive)                
        }
