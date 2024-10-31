namespace RTOpenAI
open System
open System.IO
open System.Threading
open System.Threading.Channels
open FSharp.Control
open Microsoft.Maui
open Plugin.Maui.Audio

 type Player() =
    let audioManager = lazy(IPlatformApplication.Current.Services.GetService(typeof<IAudioManager>) :?> IAudioManager)
    let channel = lazy(Channel.CreateBounded<byte[]>(30))
    let mutable _cancelToken : CancellationTokenSource option = None

    let stop() = 
        match _cancelToken with
        | Some p -> p.Cancel(); _cancelToken <-None; 
        | None -> ()

    let play (chunk:byte[]) = 
        async {
            try 
                use str = new MemoryStream(chunk)
                use player = audioManager.Value.CreateAsyncPlayer(str)
                use tknSrc = new CancellationTokenSource()
                _cancelToken <- Some tknSrc
                do! player.PlayAsync(tknSrc.Token) |> Async.AwaitTask              
                Utils.debug "Audio played"
            with ex -> 
                Utils.debug $"Failed to play audio: {ex.Message}"
        }

    let startLoop() = 
        channel.Value.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.iterAsync play
        |> Async.Start

    member this.Stop() = stop()
    member this.Dispose() = stop(); channel.Value.Writer.Complete()
    member this.IsPlaying() = _cancelToken.IsSome
    member this.Channel = 
        if not channel.IsValueCreated then startLoop()
        channel.Value