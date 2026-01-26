namespace FsPlaySamples.Cua
open System
open Fabulous
open Fabulous.Maui
open type Fabulous.Maui.View
open FSharp.Control
open FsPlaySamples.Cua.Navigation

// This is the root of the app
module App =
    /// The Model needs only to store the current navigation stack
    type Model = { Navigation: NavigationStack }

    type Msg =
        | NavigationMsg of NavigationRoute
        | BackNavigationMsg
        | BackButtonPressed
    let notifyBackButtonPressed (appMessageDispatcher: IAppMessageDispatcher) =
        Cmd.ofEffect(fun _ -> appMessageDispatcher.Dispatch(AppMsg.BackButtonPressed))


    /// In the init function, we initialize the NavigationStack
    let init () =
        { Navigation = NavigationStack.Init(NavigationRoute.Main) }, Cmd.none

    let update appMsgDispatcher msg model =
        match msg with
        | NavigationMsg route -> { Navigation = model.Navigation.Push(route) }, Cmd.none
        | BackNavigationMsg -> { Navigation = model.Navigation.Pop() }, Cmd.none
        | BackButtonPressed -> model, notifyBackButtonPressed appMsgDispatcher

    let subscribe (nav: NavigationController) _ =
        let navRequestedSub dispatch =
            nav.NavigationRequested.Subscribe(fun route -> dispatch(NavigationMsg route))

        let backNavRequestedSub dispatch =
            nav.BackNavigationRequested.Subscribe(fun () -> dispatch BackNavigationMsg)

        [ [ nameof navRequestedSub ], navRequestedSub
          [ nameof backNavRequestedSub ], backNavRequestedSub ]

    let program nav appMsgDispatcher =
        Program.statefulWithCmd init (update appMsgDispatcher)
        |> Program.withSubscription(subscribe nav)

    let navView nav appMsgDispatcher (path: NavigationRoute) =
        match path with
        | NavigationRoute.Main -> AnyPage(Views.PageView.view nav appMsgDispatcher)
        | NavigationRoute.Keys -> AnyPage(Views.CredsView.view nav appMsgDispatcher)
        | NavigationRoute.ArticleSummaries acct -> AnyPage(Views.SummaryView.view nav appMsgDispatcher acct)

    let view nav appMsgDispatcher () =
        Component("Sample") {
            let! model = Context.Mvu(program nav appMsgDispatcher)

            (Application() {
                Window(
                    (NavigationPage() {
                        // We inject in the NavigationPage history the back stack of our navigation
                        for navPath in List.rev model.Navigation.BackStack do
                            navView nav appMsgDispatcher navPath

                        // The page currently displayed is the one on top of the stack
                        navView nav appMsgDispatcher model.Navigation.CurrentPage
                    })
                        .onBackButtonPressed(BackButtonPressed)
                        .onBackNavigated BackNavigationMsg
                )
            })
                .environment(Settings.Environment.settingsKey, new Settings.SettingsModel())
        }

