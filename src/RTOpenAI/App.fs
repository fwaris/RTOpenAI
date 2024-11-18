namespace RTOpenAI
open System
open System.IO
open System.Text.Json.Serialization.TypeCache
open FSharp.Control.Websockets
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Controls
open Microsoft.Maui
open Microsoft.Maui.Graphics
open Microsoft.Maui.Accessibility
open Microsoft.Maui.Primitives
open type Fabulous.Maui.View
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open Microsoft.Maui.Storage
open Microsoft.Win32.SafeHandles

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
                        let rcdr = new Recorder(model.audioFormat)
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

    let play (model:Model) =
        task {
            if model.outputFile.IsNone then failwith "Nothing recorded"
            model.player |> Option.iter (fun p -> p.Stop())
            let player = new Player(model.audioFormat)
            //let mfolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            let wav = File.ReadAllBytes(model.outputFile.Value)
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

    let playStop (model:Model) = 
        model.player |> Option.iter (fun p -> p.Stop())
        {model with player = None},[]

    let init () = 
        {
            audioFormat = AudioFormat.RTApi
            recorder = None
            outputFile = None
            player = None
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            log = []
        },[]

    let update msg model =
        match msg with
        | Export -> model, []
        | Play_Start -> model,Cmd.OfTask.either play model Play_Started EventError
        | Play_Started player -> { model with player = player },[]
        | Play_Stop -> playStop model
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

    let controlsView (model:Model) =
        let f1 = FontImageSource( FontFamily="MaterialSymbols", Glyph = Icons.play, Color = Colors.Red)
        (VStack(spacing = 25.) {
            Ellipse()
                .size(10., 10.)
                .background(Colors.Green)
                
            ImageButton(
                FontImageSource( Size=48, FontFamily="MaterialSymbols", Glyph = Icons.play, Color = ThemeAware.With(Colors.DarkMagenta, Colors.Magenta)),
                Play_Start)
                .semantics(hint = "Play audio")
                .centerHorizontal()                        
       
            ImageButton(
                FontImageSource( Size=48, FontFamily="MaterialSymbols", Glyph = Icons.cancel, Color = ThemeAware.With(Colors.DarkMagenta, Colors.Magenta)),
                Play_Stop)
                .semantics(hint = "Stop audio")
                .centerHorizontal()                        

            ImageButton(
                FontImageSource(
                        Size=48,
                        FontFamily="MaterialSymbols",
                        Glyph = (if model.recorder.IsNone then Icons.mic else Icons.stop),
                        Color = ThemeAware.With(Colors.DarkMagenta, Colors.Magenta)),
                Recorder_StartStop)
                .semantics(hint = "Record audio")
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
