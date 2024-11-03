namespace RTOpenAI
open System
open System.IO
open Fabulous
open Fabulous.Maui
open Microsoft.Maui
open Plugin.Maui.Audio
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel
open FSharp.Control

module App =
    open Utils
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
            playState = None
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            log = []            
        },[]

    let tempFile() = Path.Combine(FileSystem.CacheDirectory, Path.GetRandomFileName() + ".wav")

    let getRecordPermission() = Permissions.RequestAsync<Permissions.Microphone>()

    let startStopRecording (model:Model) =
        task {
            match model.recorder with
            | Some rcdr ->    
                do! rcdr.StopPcmAsync()
                //model.audioPipe |> Option.iter(fun p -> p.Writer.Complete())
                //do! Async.Sleep(100)
                //model.stream |> Option.iter(fun s -> s.Dispose())
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

    let startPlay model = 
        task {
            let pipe = System.Threading.Channels.Channel.CreateBounded<byte[]>(5)
            let pcm = @"C:\Users\Faisa\Music\PinkPanther30 - Copy.pcm"
            let wav = @"C:\Users\Faisa\Music\PinkPanther30.wav"
            //let ms :Stream = File.OpenRead(wav)
            use ms = new MemoryStream(File.ReadAllBytes(wav))
            ms.Position <- 0L
            let player = model.audioManager.Value.CreateAsyncPlayer(ms)
            //let wavBytes = ms.ToArray()
            //let byteRate = 44100L
            //let samples = buff.LongLength / byteRate
            //let rem = buff.LongLength % byteRate
            //let testHeader = wavBytes.[0..43]
            //let waveHeader = Plugin.Maui.Audio.IAudioPlayer.WaveFileHeader(samples *  byteRate, 22050,2,16)
            //pipe.Writer.WriteAsync(testHeader) |> ignore
            //File.WriteAllBytes(@"C:\Users\Faisa\Music\PinkPanther30 - Copy.pcm.header",waveHeader)
            let cts = new System.Threading.CancellationTokenSource()
            //async {
            //    let comp = 
            //        wavBytes
            //        |> Seq.chunkBySize 4096
            //        |> AsyncSeq.ofSeq
            //        |> AsyncSeq.iterAsync (fun bytes -> 
            //            task {
            //                use ts = new Threading.CancellationTokenSource()
            //                //ts.CancelAfter(5000)
            //                let! r = pipe.Writer.WaitToWriteAsync(ts.Token)
            //                if r then
            //                    do! pipe.Writer.WriteAsync(bytes)
            //                else
            //                   return failwith "Write timeout"
            //             } |> Async.AwaitTask)
            //    match! Async.Catch(comp) with
            //    | Choice1Of2 _ -> ()
            //    | Choice2Of2 ex -> debug ex.Message
            //}
            //|> Async.Start
            //let player = model.audioManager.Value.CreateAsyncPlayer(pipe)
            player.PlayAsync(cts.Token) |> ignore
            return {Player=player; Token=cts; Pipe=pipe }
        }

    let stopPlay (model:Model) = 
        match model.playState with
        | Some p -> p.Token.Cancel(); p.Player.Dispose(); { model with playState = None },[]
        | None -> model,[]

    let update msg model =
        match msg with
        | Export -> model, []
        | Play_Start -> model, Cmd.OfTask.either startPlay model Play_Started EventError
        | Play_Started p -> { model with playState = Some p },[]
        | Play_Stop -> stopPlay model
        | Recorder_StartStop -> model,Cmd.OfTask.either startStopRecording model Recorder_Set EventError
        | Recorder_Set (Some (rcdr,str,pipe)) -> { model with recorder=Some rcdr},[]
        | Recorder_Set None -> { model with recorder = None},[]
        | EventError exn -> debug exn.Message; model,[]
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG },[]
        | Log_Clear -> { model with log = [] },[]
    
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
        |> Program.withView View.view
