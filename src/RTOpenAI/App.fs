namespace RTOpenAI
open System
open System.IO
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Controls
open Microsoft.Maui
open Microsoft.Maui.Graphics
open Microsoft.Maui.Accessibility
open Microsoft.Maui.Primitives
open type Fabulous.Maui.View
open Plugin.Maui.Audio
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open Microsoft.Maui.Storage

module App =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)

    let semanticAnnounce text =
        Cmd.ofMsg(SemanticScreenReader.Announce(text))

    let mapCmd cmdMsg =
        match cmdMsg with
        | SemanticAnnounce text -> semanticAnnounce text

    let testData = [
        { Date = DateTime.Now; Weight = 100.0 }
        { Date = DateTime.Now.AddDays(-1.); Weight = 99.0 }
    ]

    let playRecording (model:Model) =
        task {
            try
                match model.audioSource with 
                | Some src -> 
                    use str = src.GetAudioStream()
                    let player = model.audioManager.Value.CreateAsyncPlayer(str)
                    do! player.PlayAsync(System.Threading.CancellationToken.None)
                    if  not player.IsPlaying then model.mailbox.Writer.TryWrite(Play_Done) |> ignore
                | None -> ()
            with ex -> 
                debug ex.Message            
        }
        |> ignore

    let getRecordPermission() = Permissions.RequestAsync<Permissions.Microphone>()

    let startStopRecording (model:Model) =
        task {
            match model.recorder with
            | Some rcdr ->    
                let! s = rcdr.StopAsync()                
                debug $"Recording stopped: {s}"
                return None, Some s
            | None -> 
                let rcdr = model.audioManager.Value.CreateRecorder()
                let! permission = MainThread.InvokeOnMainThreadAsync<PermissionStatus>(fun () -> getRecordPermission())
                debug $"Permission: {permission}"
                if permission = PermissionStatus.Granted then
                    try 
                        let temp = Path.GetRandomFileName()
                        let fn = Path.Combine(FileSystem.CacheDirectory, temp + ".wav")
                        do! File.Create(fn).DisposeAsync()
                        debug $"Recording file: {fn}"
                        let opts = AudioRecordingOptions()
                        opts.Encoding <- Encoding.LinearPCM                        
                        opts.BitDepth <- BitDepth.Pcm16bit                    
                        do! rcdr.StartAsync(fn) 
                        debug $"Recording started"
                        return Some rcdr, None
                    with ex ->
                        debug ex.Message
                        return None,None
                else
                    return None,None
        }

    let init () = 
        let audioManager = lazy(IPlatformApplication.Current.Services.GetService(typeof<IAudioManager>) :?> IAudioManager)
        { 
            weights=testData; 
            audioManager=audioManager; 
            recorder = None
            isPlaying = false
            audioSource = None
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            log = []
        },[]

    let update msg model =
        match msg with
        | Export -> model, []
        | SetWeight s -> debug s; model,[]
        | Play -> playRecording model; {model with isPlaying=true},[]
        | Play_Done -> {model with isPlaying=false},[]
        | Recorder_StartStop -> model,Cmd.OfTask.either startStopRecording model Recorder_Set EventError
        | Recorder_Set (rcdr,src) -> { model with recorder = rcdr; audioSource = src },[]
        | EventError exn -> debug exn.Message; model,[]
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG },[]
        | Log_Clear -> { model with log = [] },[]

    let logView (model:Model) =
        (ListView(model.log) (fun item -> TextCell($"{item}")))
            .gridColumn(1)
            .header(Label("Log"))
            .horizontalScrollBarVisibility(ScrollBarVisibility.Never)

    let controlsView (model:Model) = 
        let indicatorViewRef = ViewRef<IndicatorView>()
        (VStack(spacing = 25.) {
            IndicatorView(indicatorViewRef)
                .selectedIndicatorColor(Colors.Green)
                .indicatorSize(24.)
                .indicatorsShape(IndicatorShape.Circle)
                .indicatorColor(Colors.LightGray)
            Button("\ue037", Play)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Counts the number of times you click")
                .isEnabled(model.audioSource.IsSome && not model.isPlaying)
                .centerHorizontal()
            Button((if model.recorder.IsNone then "\ue029" else "\ue047") , Recorder_StartStop)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Counts the number of times you click")
                .centerHorizontal()
        })
            .padding(30., 0., 30., 0.)
            .centerVertical()
            .gridColumn(0)
        

    let view model =
        Application(
            ContentPage(
                ScrollView(
                    Grid([Dimension.Star; Dimension.Star],[Dimension.Star]) {
                        controlsView model
                        logView model
                    }
                )
                    .width(300)
            )
        )

    
    let subscribe model : Sub<Msg> =
        let backgroundEvent dispatch =
            async{
                let comp = 
                     model.mailbox.Reader.ReadAllAsync()
                     |> AsyncSeq.ofAsyncEnum
                     |> AsyncSeq.iter (fun msg -> debug $"{msg}"; dispatch msg)
                match! Async.Catch(comp) with
                | Choice1Of2 _ -> ()
                | Choice2Of2 ex -> debug ex.Message
            }
            |> Async.Start
            {new IDisposable with member __.Dispose() = model.mailbox.Writer.Complete()}
        [ [ nameof backgroundEvent ], backgroundEvent ]

    let program  =         
        Program.statefulWithCmd init update
        |> Program.withSubscription subscribe
        |> Program.withView view
