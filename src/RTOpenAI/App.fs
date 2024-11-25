namespace RTOpenAI
open System
open System.IO
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Controls
open Microsoft.Maui
open Microsoft.Maui.Graphics
open type Fabulous.Maui.View
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open RTOpenAI.Audio

module App =
    open Utils
    let tempFile() = Path.Combine(FileSystem.CacheDirectory, Path.GetRandomFileName() + ".pcm")
    let getRecordPermission() = Permissions.RequestAsync<Permissions.Microphone>()
    let startStopRecording (model:Model) =
        task {
            match model.recorder with
            | Some rcdr ->
                rcdr.Stop()
                printfn $"{model.outputFile}"
                return None
            | None -> 
                let! permission = MainThread.InvokeOnMainThreadAsync<PermissionStatus>(fun () -> getRecordPermission())
                debug $"Permission: {permission}"
                if permission = PermissionStatus.Granted then
                    try
                        let fn = tempFile()
                        let rcdr = Utils.audioManager.CreateRecorder(model.audioFormat)
                        let comp =
                            asyncSeq {
                                use str = File.Create fn
                                use wtr = new BinaryWriter(str)
                                for samples in rcdr.Channel.Reader.ReadAllAsync() |> AsyncSeq.ofAsyncEnum do
                                    wtr.Write(samples)
                                    debug $"written {samples.Length}"
                            }
                            |> AsyncSeq.iterAsync Async.Ignore
                        async {
                            match! Async.Catch comp with
                            | Choice1Of2 _ -> printfn "recording stopped"
                            | Choice2Of2 ex -> Log.exn(ex,"record to file")
                        }
                        |> Async.Start
                        rcdr.Record()
                        return Some (rcdr,fn)
                    with ex ->
                        Log.exn(ex,"startStopRecording")
                        return None
                else
                    return None
        }

    let playStartStop (model:Model) =
        task {
            match model.player with
            | Some p ->
                p.Stop()
                return None
            | None -> 
                use! testFile = FileSystem.Current.OpenAppPackageFileAsync("PinkPanther30.wav")
                use ms = new MemoryStream()
                do! testFile.CopyToAsync(ms)
                let wav = ms.GetBuffer().[44..]
                //if model.outputFile.IsNone then failwith "Nothing recorded"
                model.player |> Option.iter (fun p -> p.Stop())
                let player = Utils.audioManager.CreatePlayer(model.audioFormat)
                //let mfolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
                //let wav = mfolder @@ "PinkPanther30 - Copy.pcm" |> File.ReadAllBytes
                //let wav = File.ReadAllBytes(model.outputFile.Value)
                let comp = 
                    wav
                    |> Seq.chunkBySize (model.audioFormat.ByteRate * 2)
                    |> Seq.indexed
                    |> AsyncSeq.ofSeq
                    |> AsyncSeq.iterAsync (fun (i,bytes) -> 
                        task {
                            do! player.Channel.Writer.WriteAsync(bytes)
                        }
                        |> Async.AwaitTask)
                async {
                    match! Async.Catch comp with
                    | Choice1Of2 _ -> printfn "all data written to channel"
                    | Choice2Of2 ex -> Log.exn(ex,"App.play")
                }
                |> Async.Start
                player.Play()
                return (Some player)
        }

    let init () = 
        {
            audioFormat = AudioFormat.Default
            recorder = None
            outputFile = None
            player = None
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            log = []
        },[]

    let update msg model =
        match msg with
        | Export -> model, []
        | Play_StartStop -> model,Cmd.OfTask.either playStartStop model Play_Started EventError
        | Play_Started player -> { model with player = player },[]
        | Recorder_StartStop -> model,Cmd.OfTask.either startStopRecording model Recorder_Set EventError
        | Recorder_Set (Some (rcdr,fn)) -> { model with recorder = Some rcdr; outputFile= Some fn},[]
        | Recorder_Set None -> { model with recorder = None},[]
        | EventError exn -> debug exn.Message; model,[]
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG },[]
        | Log_Clear -> { model with log = [] },[]

    let logView (model:Model) =
        (ListView(model.log) (fun item -> TextCell($"{item}")))
            .gridColumn(1)
            .header(Label("Log"))
            .horizontalScrollBarVisibility(ScrollBarVisibility.Never)
    let bPlay = FontImageSource( Size=48, FontFamily="MaterialSymbols", Glyph = Icons.play, Color = Colors.Lime)
    let bCancel =  FontImageSource( Size=48, FontFamily="MaterialSymbols", Glyph = Icons.cancel, Color=Colors.Lime)
    let bMic =  FontImageSource(Size=48,FontFamily="MaterialSymbols",Glyph = Icons.mic, Color = Colors.Lime)
    let bStop = FontImageSource(Size=48,FontFamily="MaterialSymbols",Glyph = Icons.stop, Color = Colors.Lime)

    let controlsView (model:Model) =
         (VStack(spacing = 25.) {
            Ellipse()
                .size(10., 10.)
                .background(Colors.Green)
                
            ImageButton((if model.player.IsSome then bStop else bPlay),Play_StartStop)
                .semantics(hint = "Play audio")
                .centerHorizontal()                                           

            ImageButton((if model.recorder.IsSome then bStop else bMic),Recorder_StartStop)
                .semantics(hint = "Record audio")
                .centerHorizontal()                        
        })
            .padding(30., 0., 30., 0.)
            .centerVertical()
            .gridColumn(0)
        
    let view model =
        Application() {
            Window() {
                ContentPage(
                  
                        Grid([Dimension.Star; Dimension.Star],[Dimension.Star]) {
                            controlsView model
                            logView model
                        }
                  )
                 .padding(0., 0., 0., 0.)
                 .width(300)
                }            
            }
        
    
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
