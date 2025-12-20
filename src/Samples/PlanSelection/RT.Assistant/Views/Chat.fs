namespace RT.Assistant.Views
open System
open Fabulous
open Microsoft.Maui
open Microsoft.Maui.Graphics
open FSharp.Control
open Fabulous.Maui
open type Fabulous.Maui.View
open type Fabulous.Context
open RT.Assistant
open RT.Assistant.Navigation

module Chat =
    
    let subscribe (appMsgDispatcher: IAppMessageDispatcher) (model:Model)=        
        let localAppMsgSub dispatch =
            appMsgDispatcher.Dispatched.Subscribe(fun msg ->
                match msg with
                | AppMsg.BackButtonPressed -> dispatch Msg.BackButtonPressed)
        localAppMsgSub

    let subscribeBackground (model:Model) =
        let backgroundEvent dispatch =
            let ctx = new System.Threading.CancellationTokenSource()
            let comp =
                async{
                    let comp =
                         model.mailbox.Reader.ReadAllAsync()
                         |> AsyncSeq.ofAsyncEnum
                         |> AsyncSeq.iter dispatch            
                    match! Async.Catch(comp) with
                    | Choice1Of2 _ -> debug "dispose subscribeBackground"
                    | Choice2Of2 ex -> debug ex.Message
                }
            Async.Start(comp,ctx.Token)            
            {new IDisposable with member _.Dispose() = ctx.Dispose(); debug "disposing subscription backgroundEvent";}
        backgroundEvent
        
    let subscriptions appMsgDispatcher model =
        let sub1 = subscribe appMsgDispatcher model
        let sub2 = subscribeBackground model
        [
            if model.isActive then
                [nameof sub1], sub1
                [nameof sub2], sub2                
        ]
       
    let program nav appMsgDispatcher =
        Program.statefulWithCmd Update.initModel (Update.update nav)
        |> Program.withSubscription(subscriptions appMsgDispatcher)
        
    let headerView (model:Model) =
        (Grid([Dimension.Star; Dimension.Star; Dimension.Star],[Dimension.Absolute 50.0]) {
            Button(Icons.settings,Settings_Show)
                .isEnabled(not (model.sessionState = RTOpenAI.WebRTC.State.Connecting))
                .font(size=30.0, fontFamily=C.FONT_SYMBOLS)
                .background(Colors.Transparent)
                .textColor(Colors.Magenta)
                .alignStartHorizontal()
                .gridColumn(0)
            (HStack() {
            })
                .gridColumn(1)
                .centerHorizontal()
                .padding(2.)
            Button("\ue8ac",Cn_EnsureKey_Start)
                .isEnabled(not (model.sessionState = RTOpenAI.WebRTC.State.Connecting))
                .font(size=30.0, fontFamily=C.FONT_SYMBOLS)
                .background(Colors.Transparent)
                .textColor(
                    match model.sessionState with
                    | RTOpenAI.WebRTC.State.Connecting -> Colors.Orange
                    | _ -> match model.sessionState with
                           | RTOpenAI.WebRTC.State.Connected -> Colors.Magenta
                           | _ -> Colors.Gray)
                .centerVertical()
                .alignEndHorizontal()
                .gridColumn(2)               

        })
            .gridRow(0)            
            .gridColumnSpan(2)
                        
    let logView (model:Model) =
        ((Grid([Dimension.Star],[Dimension.Absolute 38.; Dimension.Star])) {
            Button("Clear Log",Log_Clear)
                .font(size=15.0)
                .margin(1)
            ((CollectionView model.log)
                (fun item -> Border(Label($"{item}").font(size=model.fontSize).lineBreakMode(LineBreakMode.WordWrap).margin(1.0))
                                .stroke(SolidColorBrush(Colors.Silver))
                                .strokeThickness(1.0)
                                .margin(Thickness(0.,0.,0.,2.)))
            )
                .gridRow(1)
        })
            .gridColumn(1)
            .gridRow(1)
            .margin(1)


    let controlsView (model:Model) =
        (Grid(
            [Dimension.Star],
            [Dimension.Absolute 0.0 //hybrid view 
             Dimension.Absolute 30.0 //label 
             Dimension.Star          //pred edit
             Dimension.Absolute 30.0 //label 
             Dimension.Star          //query edit
             Dimension.Absolute 38.0]) { //tool bar
            (HybridWebView())
                .gridRow(0)
                .reference(model.hybridView)
                .margin(5.)
            Label("Predicates:").gridRow(1)            
            (Editor(model.code.predicates,SetConsult))                                
                .font(size=model.fontSize)
                .gridRow(2)
                .margin(5,0,5,5)
                .placeholder("predicates")
                .background(SolidColorBrush(if Controls.Application.Current.RequestedTheme = ApplicationModel.AppTheme.Light then Colors.Bisque else Colors.Navy))
            Label("Query:").gridRow(3)
            (Editor(model.code.query,SetQuery))
                .font(size=model.fontSize)
                .gridRow(4)
                .margin(5,0,5,5)
                .placeholder("query")
                .background(SolidColorBrush(if Controls.Application.Current.RequestedTheme = ApplicationModel.AppTheme.Light then Colors.Bisque else Colors.Navy))
            (HStack() {
                Button("Submit",SubmitCode)
                    .font(size=15.0)
                    .margin(1,1,5,1)
                    .centerVertical()
                Button("+",FontLarger).background(Colors.Transparent).font(size=15.0, fontFamily=C.FONT_REG).textColor(Colors.Magenta).centerVertical()
                Button("-",FontSmaller).background(Colors.Transparent).font(size=15.0, fontFamily=C.FONT_REG).textColor(Colors.Magenta).centerVertical()     
            }).gridRow(5)               
        })
            .gridRow(1)
            .gridColumn(0)
                               
    let view nav appMsgDispatcher=
        Component("Chat") {
            let! model = Context.Mvu(program nav appMsgDispatcher)            
            ContentPage(
                    (Grid([Dimension.Star; Dimension.Star],[Dimension.Absolute 53.0; Dimension.Star]) {
                        controlsView model
                        logView model
                        headerView model //needed at bottom for android
                    })
                        .margin(5.)
              )
                .title("Main")
                .onNavigatedTo(Active)
                .onNavigatedFrom(InActive)
        }
