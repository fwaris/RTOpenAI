namespace RTOpenAI.Audio.Android
open System
open RTOpenAI.Audio
open System.Threading
open System.Threading.Channels
open Silk.NET.OpenAL
open FSharp.Control

#if ANDROID 
open Android.Media
type Recorder(audioFormat:RTOpenAI.Audio.AudioFormat) = 

    let channel = lazy(Channel.CreateBounded<byte[]>(30))
    
    let mutable _cancelToken : CancellationTokenSource option = None
   
    let audioRecord = lazy(        
        let encoding =
                match audioFormat.BufferFormat with
                | BufferFormat.Mono16 | BufferFormat.Stereo16 -> Encoding.Pcm16bit
                | BufferFormat.Mono8 | BufferFormat.Stereo8 -> Encoding.Pcm8bit
                | x -> failwithf $"encoding not supported for {x}"
        let mask =
              match audioFormat.BufferFormat with
              | BufferFormat.Stereo8 | BufferFormat.Stereo16 -> ChannelIn.Stereo
              | BufferFormat.Mono8 | BufferFormat.Mono16 -> ChannelIn.Mono
              | x -> failwithf $"channel in configuration not supported for {x}"

        new AudioRecord(AudioSource.Default,audioFormat.Frequency,mask,encoding,audioFormat.ByteRate*2))
    
    let startRecordLoop() =
        if _cancelToken.IsNone then
            _cancelToken <- Some (new CancellationTokenSource()) 
            let comp = async {                
                audioRecord.Value.StartRecording()
                let chunk : byte[] = Array.zeroCreate (audioFormat.ByteRate*2)
                while _cancelToken.IsSome && not _cancelToken.Value.Token.IsCancellationRequested do
                     let read = audioRecord.Value.Read(chunk,0,chunk.Length)
                     if read > 0 then
                        let buff = chunk.[0..read-1]
                        let r = channel.Value.Writer.TryWrite(buff)
                        Utils.debug $"recorded: {r} bytes, %A{buff.[0..20]}"
                        if not r then Log.warn $"record dropped {read} bytes"
                     do! Async.Sleep(1000)
            }
            async {
                match! Async.Catch comp with
                | Choice1Of2 _ -> printfn "record done"
                | Choice2Of2 ex -> Log.exn(ex,"Recorder.startRecordLoop")
            }
            |> Async.Start
    let stop() =
        if _cancelToken.IsSome then
                _cancelToken.Value.Cancel()
                _cancelToken <- None
                if audioRecord.IsValueCreated then 
                    audioRecord.Value.Stop()
                    (audioRecord.Value :> IDisposable).Dispose()
                if channel.IsValueCreated then channel.Value.Writer.Complete()            
    
    interface IRecorder with
        member this.Channel: System.Threading.Channels.Channel<byte array> = channel.Value         
        member this.Mute(): unit =
            raise (System.NotImplementedException()) 
        member this.Record(): unit = startRecordLoop()        
        member this.Stop(): unit = stop()            
        member this.Unmute(): unit = 
            raise (System.NotImplementedException()) 

#endif


