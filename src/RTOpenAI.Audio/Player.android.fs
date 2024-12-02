namespace RTOpenAI.Audio.Android
open System
open System.Threading
open System.Threading.Channels
open Silk.NET.OpenAL
open FSharp.Control
open RTOpenAI.Audio

#if ANDROID
open Android.Media
type Player(audioFormat:RTOpenAI.Audio.AudioFormat) =
    let channel  = lazy(Channel.CreateBounded<byte[]>(30))
    
    let mutable _cancelToken : CancellationTokenSource option = None
    
    let audioTrack = lazy(        
        let encoding =
                match audioFormat.BufferFormat with
                | BufferFormat.Mono16 | BufferFormat.Stereo16 -> Encoding.Pcm16bit
                | BufferFormat.Mono8 | BufferFormat.Stereo8 -> Encoding.Pcm8bit
                | x -> failwithf $"encoding not supported for {x}"
        let mask =
              match audioFormat.BufferFormat with
              | BufferFormat.Stereo8 | BufferFormat.Stereo16 -> ChannelOut.Stereo
              | BufferFormat.Mono8 | BufferFormat.Mono16 -> ChannelOut.Mono
              | x -> failwithf $"channel out configuration not supported for {x}"

        new AudioTrack(Stream.Music,audioFormat.Frequency,mask,encoding,audioFormat.ByteRate*2,AudioTrackMode.Stream)
        )
         
    let enqueueChunk(chunk:byte[]) =
        let mutable written = 0
        while audioTrack.Value.PlayState = PlayState.Playing &&  written < chunk.Length do 
            written <- audioTrack.Value.Write(chunk,written,chunk.Length-written)
            
    let rec startPlayLoop() =         
        if _cancelToken.IsNone then
            _cancelToken <- Some (new CancellationTokenSource())
            let comp =
                task {
                    let mutable chunk = [||]
                    audioTrack.Value.Play()
                    while _cancelToken.IsSome && not _cancelToken.Value.Token.IsCancellationRequested do
                        let! r =  channel.Value.Reader.WaitToReadAsync(_cancelToken.Value.Token)
                        if r then         
                            let _ = channel.Value.Reader.TryRead(&chunk) 
                            enqueueChunk chunk
                        else
                            chunk <- null                    
                }
            async {
                match! Async.Catch (Async.AwaitTask comp) with
                | Choice1Of2 _ -> printfn "android play done"
                | Choice2Of2 ex -> RTOpenAI.Audio.Log.exn(ex,"android Player.startPlayLoop")
            }
            |> Async.Start
        else
            RTOpenAI.Audio.Log.info "android playLoop alreadyStarted"
              
    let cleanup() =
        if channel.IsValueCreated then
            channel.Value.Writer.TryComplete() |> ignore
                
    let cancel() =
        match _cancelToken with
        | Some cts -> cts.Cancel(); _cancelToken <-None 
        | None -> ()

    let stop() =
        if audioTrack.IsValueCreated then
            audioTrack.Value.Stop()
        cancel()
        cleanup()
     
    
    let play() =
        startPlayLoop()
        
    interface IPlayer with
        member this.Channel: System.Threading.Channels.Channel<byte array> = channel.Value             
        member this.IsPlaying(): bool = _cancelToken.IsSome         
        member this.Pause(): unit =            
            raise (System.NotImplementedException())
        member this.Play(): unit = 
            startPlayLoop()
        member this.Stop(): unit = 
            stop()

#endif

