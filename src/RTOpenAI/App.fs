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
    open Utils
    let tempFile() = Path.Combine(FileSystem.CacheDirectory, Path.GetRandomFileName() + ".wav")

    let getRecordPermission() = Permissions.RequestAsync<Permissions.Microphone>()

    let startStopRecording (model:Model) =
        task {
            match model.recorder with
            | Some rcdr ->    
                do! rcdr.StopPcmAsync()
                model.audioPipe |> Option.iter(fun p -> p.Writer.Complete())
                do! Async.Sleep(100)
                model.stream |> Option.iter(fun s -> s.Dispose())
                //let! s = rcdr.StopAsync()
                //use ms = new MemoryStream()
                //do! s.GetAudioStream().CopyToAsync(ms)
                //model.player.Queue(ms.GetBuffer())
                return None
            | None -> 
                let rcdr = model.audioManager.Value.CreateRecorder()
                let! permission = MainThread.InvokeOnMainThreadAsync<PermissionStatus>(fun () -> getRecordPermission())
                debug $"Permission: {permission}"
                if permission = PermissionStatus.Granted then
                    try 
                        let fn = tempFile()
                        debug $"Recording to: {fn}"                        
                        let pipe = System.Threading.Channels.Channel.CreateBounded<byte[]>(5)
                        let opts = AudioRecordingOptions()
                        opts.Encoding <- Encoding.LinearPCM                        
                        opts.BitDepth <- BitDepth.Pcm16bit   
                        opts.Channels <- ChannelType.Mono
                        opts.SampleRate <- 24000
                        let str = fn |> File.Create :> Stream
                        let loop = 
                            pipe.Reader.ReadAllAsync()
                            |> AsyncSeq.ofAsyncEnum
                            |> AsyncSeq.iterAsync (fun bytes -> str.WriteAsync(bytes.AsMemory()).AsTask() |> Async.AwaitTask)
                            |> Async.Catch
                        async {
                            match! loop with 
                            | Choice1Of2 _ -> ()
                            | Choice2Of2 ex -> debug ex.Message
                        }
                        |> Async.Start
                        do! rcdr.StartPcmAsync(pipe)
                        do! File.Create(fn).DisposeAsync()
                        do! rcdr.StartAsync(fn, opts)
                        debug $"Recording started"
                        return Some (rcdr,null,pipe)
                    with ex ->
                        debug ex.Message
                        return None
                else
                    return None
        }


    let init () = 
        let audioManager = lazy(IPlatformApplication.Current.Services.GetService(typeof<IAudioManager>) :?> IAudioManager)
        let pipe = System.Threading.Channels.Channel.CreateBounded<byte[]>(5)
       // pipe.Writer.WriteAsync(Plugin.Maui.Audio.IAudioRecorder.StreamWaveFileHeader((24000, 16, 1))) |> ignore
        //let player = audioManager.Value.CreatePlayer()
        
        //player.Play();
        //player.PlaybackEnded.Add(fun x -> debug $"play stopped")
        { 
            audioManager=audioManager; 
            recorder = None
            player = None
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            audioPipe = None
            log = []
            stream = None
        },[]

    let update msg model =
        match msg with
        | Export -> model, []
        | Play_Stop -> model.player |> Option.iter _.Stop(); model, []
        | Recorder_StartStop -> model,Cmd.OfTask.either startStopRecording model Recorder_Set EventError
        | Recorder_Set (Some (rcdr,str,pipe)) -> { model with recorder = Some rcdr; stream = Some str; audioPipe=Some pipe},[]
        | Recorder_Set None -> { model with recorder = None; stream = None; audioPipe = None },[]
        | EventError exn -> debug exn.Message; model,[]
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG },[]
        | Log_Clear -> { model with log = [] },[]

    let logView (model:Model) =
        (ListView(model.log) (fun item -> TextCell($"{item}")))
            .gridColumn(1)
            .header(Label("Log"))
            .horizontalScrollBarVisibility(ScrollBarVisibility.Never)

    let controlsView (model:Model) = 
        (VStack(spacing = 25.) {
            Ellipse()
                .size(10., 10.)
                .background(Colors.Green)
            Button(Icons.cancel, Play_Stop)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Counts the number of times you click")
                .centerHorizontal()
            Button((if model.recorder.IsNone then Icons.mic else Icons.stop) , Recorder_StartStop)
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
