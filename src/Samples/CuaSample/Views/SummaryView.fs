namespace FsPlaySamples.Cua.Views

open FSharp.Control
open Fabulous
open Fabulous.Maui
open type Fabulous.Maui.View
open type Fabulous.Context
open FsPlaySamples.Cua.Navigation
open FsPlaySamples.Cua
open Microsoft.Maui.Graphics


module SummaryView =
    let private (!-) (a:string option) = a |> Option.defaultValue "" 
    type AcctModel = { summaries: ArticleSummary list; isActive:bool}
    and AcctMsg = BackButtonPressed | Active | InActive | Nop of string
    
    let init summaries =
        { summaries=summaries; isActive=false;}, Cmd.none        

    let update nav msg (model: AcctModel) =
        match msg with
        | BackButtonPressed -> model, Navigation.navigateBack nav
        | Active -> {model with isActive = true}, Cmd.none
        | InActive -> {model with isActive = false}, Cmd.none
        | Nop string -> model, Cmd.none        

    let subscribe (appMsgDispatcher: IAppMessageDispatcher) model =
        let localAppMsgSub dispatch =
            appMsgDispatcher.Dispatched.Subscribe(fun msg ->
                match msg with
                | AppMsg.BackButtonPressed -> dispatch BackButtonPressed)

        localAppMsgSub
        
    let subscriptions appMsgDispatcher model =
        let sub1 = subscribe appMsgDispatcher model
        [
            if model.isActive then
                [nameof sub1], sub1
        ]

    let program nav appMsgDispatcher =
        Program.statefulWithCmd init (update nav)
        |> Program.withSubscription(subscriptions appMsgDispatcher)

    let view nav appMsDispatcher articleSummary =
        Component("Data") {            
            let! model = Context.Mvu(program nav appMsDispatcher, articleSummary)            
            (ContentPage(                
                ScrollView(
                    (CollectionView model.summaries (fun x ->
                        VStack() {                            
                            Label($"* {x.Title}")
                            Label($"{x.Summary}")                                
                            (BoxView(Colors.Gray)).height(1)
                        })).margin(5)                
                ).padding(5.)                    
            ))
                .title("Summaries")
                .hasBackButton(true)
                .onNavigatedTo(Active)
                .onNavigatedFrom(InActive)                
        }

