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
                rcdr.Stop()
                return None
            | None -> 
                let! permission = MainThread.InvokeOnMainThreadAsync<PermissionStatus>(fun () -> getRecordPermission())
                debug $"Permission: {permission}"
                if permission = PermissionStatus.Granted then
                    try 
                        let rcdr = new Recorder()
                        return Some (rcdr)
                    with ex ->
                        debug ex.Message
                        return None
                else
                    return None
        }

    let play (model:Model) =
        task {
            model.player |> Option.iter (fun p -> p.Stop())
            let player = new Player()
            let mfolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            let wav = File.ReadAllBytes(mfolder @@ "PinkPanther30.wav").[44..]
            let comp = 
                wav
                |> Seq.chunkBySize (22050 * 2)
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
            recorder = None
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
        | Recorder_Set (Some (rcdr)) -> { model with recorder = Some rcdr},[]
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
        (VStack(spacing = 25.) {
            Ellipse()
                .size(10., 10.)
                .background(Colors.Green)
            Button(Icons.play, Play_Start)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Play audio")
                .centerHorizontal()
                //.textColor(light = Colors.DarkBlue, dark = FabColor. )
                
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
