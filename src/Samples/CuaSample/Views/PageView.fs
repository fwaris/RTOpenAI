namespace FsPlaySamples.Cua.Views
open System
open Fabulous
open Microsoft.Maui
open Microsoft.Maui.Controls
open Microsoft.Maui.Graphics
open FSharp.Control
open Fabulous.Maui
open type Fabulous.Maui.View
open type Fabulous.Context
open Microsoft.Maui.Layouts
open FsPlaySamples.Cua
open FsPlaySamples.Cua.Navigation

module PageView =
    
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
                         model.mailbox.Reader.ReadAllAsync(ctx.Token)
                         |> AsyncSeq.ofAsyncEnum
                         |> AsyncSeq.iter dispatch            
                    match! Async.Catch(comp) with
                    | Choice1Of2 _ -> () //debug "dispose subscribeBackground"
                    | Choice2Of2 ex -> debug ex.Message
                }
            Async.Start(comp,ctx.Token)            
            {new IDisposable with member _.Dispose() = ctx.Dispose()} //debug "disposing subscription backgroundEvent";
        backgroundEvent
        
    let webviewInteraction (model:Model) =
        let rec webviewInteraction dispatch =
            let ctx = new System.Threading.CancellationTokenSource()
            let comp =
                async{
                    let comp =
                         FsPlay.ServiceEvents.mailbox.Value.Reader.ReadAllAsync(ctx.Token)
                         |> AsyncSeq.ofAsyncEnum
                         |> AsyncSeq.bufferByTime 1000
                         |> AsyncSeq.choose Array.tryLast
                         |> AsyncSeq.map WebviewInteraction                         
                         |> AsyncSeq.iter dispatch            
                    match! Async.Catch(comp) with
                    | Choice1Of2 _ -> debug $"disposed subscribe {nameof(webviewInteraction)}"
                    | Choice2Of2 ex -> debug ex.Message
                }
            Async.Start(comp,ctx.Token)            
            {new IDisposable with member _.Dispose() = ctx.Dispose(); debug $"disposing subscribe {nameof(webviewInteraction)}";}
        webviewInteraction
        
    let subscriptions appMsgDispatcher model =
        let sub1 = subscribe appMsgDispatcher model
        let sub2 = subscribeBackground model
        let sub3 = webviewInteraction model
        [
            if model.isActive then
                [nameof sub1], sub1
                [nameof sub2], sub2                
                [nameof sub3], sub3                
        ]
       
    let program nav appMsgDispatcher =
        Program.statefulWithCmd Update.initModel (Update.update nav)
        |> Program.withSubscription(subscriptions appMsgDispatcher)

    let headerView (model:Model) :WidgetBuilder<Msg,IFabGrid> =
        (Grid([Dimension.Star; Dimension.Star],[Dimension.Absolute 50.0]) {
            (HStack(){
                Button(UiOps.playIcon model, UiOps.playMsg model)
                    .font(size=20.0, fontFamily=C.FA)
                    .background(Colors.Transparent)
                    .centerVertical()
                    .alignStartHorizontal()
                match model.action with
                | Some a when model.interactiveTask.IsNone -> 
                    Label(a)
                        .verticalTextAlignment(TextAlignment.Center)                    
                        .lineBreakMode(LineBreakMode.WordWrap)
                | _ -> ()
                match model.interactiveTask with
                | Some d ->
                    FlexLayout() {
                        Label(d |> shorten 50)
                            .verticalTextAlignment(TextAlignment.Center)                    
                            .lineBreakMode(LineBreakMode.WordWrap)
                            .centerVertical()
                            .alignStartHorizontal()
                        Button(Icons.next, DoneInteractiveTask)                   
                            .background(Colors.Transparent)
                            .centerVertical()
                            .alignEndHorizontal()
                    }
                | None -> ()
             })
                .isClippedToBounds(true)
                .centerVertical()
                .alignStartHorizontal()
                .gridColumn(0)               
            (HStack(){
                // Button(Icons.fa_test,TestSomething)
                //     .font(size=20.0, fontFamily=C.FA)
                //     .background(Colors.Transparent)
                //     .textColor(Colors.Magenta)
                //     .centerVertical()
                //     .alignStartHorizontal()
                Button(Icons.fa_bars,ToggleNavBar)
                    .font(size=20.0, fontFamily=C.FA)
                    .background(Colors.Transparent)
                    .textColor(Colors.Magenta)
                    .centerVertical()
                    .alignStartHorizontal()
             })
                .centerVertical()
                .alignEndHorizontal()
                .gridColumn(1)                
        })
            .gridRow(0)
            .background(Colors.Orange)
            .gridColumnSpan(2)
            .margin(3,0,3,0)       
           
    let navBar (model:Model) =
        (Border(
            (VStack() {
                (Grid([Dimension.Star],[Dimension.Star;Dimension.Star; Dimension.Star; Dimension.Star; Dimension.Star])) {
                    Button(Icons.fa_xmark,ToggleNavBar).font(size=20,fontFamily=C.FA).alignEndHorizontal().margin(10)
                    Button(Icons.fa_table_list,Nav ViewSummary)
                        .font(size=20.0, fontFamily=C.FA)
                        .background(Colors.Transparent)
                        .textColor(Colors.Magenta)
                        .margin(2)
                        .gridRow(2)
                    Button(Icons.fa_key, Nav ViewCreds)
                         .font(size=20.0, fontFamily=C.FA)
                         .background(Colors.Transparent)
                         .textColor(Colors.Magenta)
                         .margin(2)
                         .gridRow(3)
                    (Grid([Dimension.Star; Dimension.Star],[Dimension.Star; Dimension.Star;]) {
                        Label("Preview Clicks").gridRow(0).margin(2)
                        (CheckBox(model.settings.PreviewClicks,CheckPreview>>Nav))
                                .gridRow(0).gridColumn(1).margin(2)                   
                    }).margin(2,0,0,15).gridRow(4)
                }
        })))
            .background(Colors.Orange)
            .alignStartVertical()
            .alignEndHorizontal()
            .margin(0,5,10,0)
            .gridRow(1)           
            .padding(2)
            .shadow(Shadow(Brush.Gray, Point(5., 5.)).opacity(0.5).blurRadius(40.))
            .strokeShape(RoundRectangle(CornerRadius(8.)))            
            .zIndex(2)            
      
    module Browser =
        type Model = {url:Uri}
        type Msg = SetUrl of Uri | Navigated
        
        let init navigated= {url=Uri "about:blank"}, Cmd.none
        
        let update msg model =
            match msg with
            | SetUrl uri -> {model with url=uri}, Cmd.none
            | Navigated -> model,Cmd.none

        let program = Program.statefulWithCmd init update
            
        let view() =
            Component("browser") {
                let! model = Context.Mvu(program) 
                (WebView(model.url))
                    .reference(Model.webviewCache)
                    
            }
            
    let PTR_SZ =30
    let controlsView (model:Model) =
        (AbsoluteLayout() {
            (Browser.view())
                .layoutBounds(0.,0.,1.,1.)
                .layoutFlags(AbsoluteLayoutFlags.All)
            match model.pointer, Model.webviewCache.TryValue with
            | None,_ -> ()
            | _, None -> ()
            | Some (x,y),Some wv->
                let left = x - PTR_SZ / 2  |> max 0
                let top = y - PTR_SZ / 2 |> max 0
                Ellipse()
                    .size(PTR_SZ,PTR_SZ)
                    .fill(Colors.Yellow)
                    .opacity(0.4)
                    .layoutBounds(left,top,PTR_SZ,PTR_SZ)
        })
            .gridRow(1)
            .margin(5.)
            
    let view nav appMsgDispatcher = //: WidgetBuilder<Msg,_> =
        Component("Main") {
            let! settings = EnvironmentObject Settings.Environment.settingsKey
            let! model = Context.Mvu(program nav appMsgDispatcher,settings)
            let page =
                ContentPage(
                    (Grid([Dimension.Star],[Dimension.Absolute 53.0; Dimension.Star]) {
                        controlsView model
                        headerView model 
                        if model.isOpenNavBar then navBar model
                    })
                        .margin(5.)
                )
                    .title("Main")
                    .onNavigatedTo(Active)
                    .onNavigatedFrom(InActive)

            //let () = animateSettingsPopup model.isOpenSettings
            page
        }
