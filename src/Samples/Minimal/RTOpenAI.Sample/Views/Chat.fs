namespace RTOpenAI.Sample.Views
open System
open Fabulous
open Microsoft.Maui
open Microsoft.Maui.Controls
open Microsoft.Maui.Graphics
open FSharp.Control
open Fabulous.Maui
open type Fabulous.Maui.View
open type Fabulous.Context
open RTOpenAI.Sample
open RTOpenAI.Sample.Navigation

module Chat =
    let subscribe (appMsgDispatcher: IAppMessageDispatcher) (model:Model)=
        //debug "adding subscription appDispatcher"
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
                    | Choice1Of2 _ -> ()
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
        (Grid([Dimension.Star; Dimension.Star; Dimension.Star],[Dimension.Star]) {
            Button(Icons.settings,Settings_Show)
                 .isEnabled(not (model.sessionState = RTOpenAI.WebRTC.State.Connecting))
                .font(size=30.0, fontFamily="MaterialSymbols")
                .background(Colors.Transparent)
                .textColor(Colors.Magenta)
                .centerVertical()
                .alignStartHorizontal()
                .gridColumn(0)
            Button((match model.sessionState with RTOpenAI.WebRTC.State.Connected -> Icons.link | _ -> Icons.link_off),Cn_EnsureKey_Start)
                .isEnabled(not (model.sessionState = RTOpenAI.WebRTC.State.Connecting))
                .font(size=30.0, fontFamily="MaterialSymbols")
                .background(Colors.Transparent)
                .textColor(match model.sessionState with RTOpenAI.WebRTC.State.Connecting -> Colors.Gray | _ -> Colors.Magenta)
                .centerVertical()
                .alignEndHorizontal()
                .gridColumn(2)               

        })
            .gridRow(0)
            .height(50.)
            .centerVertical()
            .gridColumnSpan(2)
    
    let logView (model:Model) =
        ((CollectionView model.log)
            (fun item -> Label($"{item}").lineBreakMode(LineBreakMode.WordWrap)))
            .gridColumn(1)
            .gridRow(1)
            .margin(5.0)
                    
    let view nav appMsgDispatcher=
        Component("Chat") {
            let! model = Context.Mvu(program nav appMsgDispatcher)            
            ContentPage(
                    (Grid([Dimension.Star],[Dimension.Absolute 25.0; Dimension.Star]) {
                        headerView model
                        logView model
                    })
                        .margin(5.)
              )
                .title("Main")
                .onNavigatedTo(Active)
                .onNavigatedFrom(InActive)
        }
