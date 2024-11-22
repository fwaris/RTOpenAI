namespace RTOpenAI.Audio.Al
open RTOpenAI.Audio
open System
open System.Threading
open System.Threading.Channels
open FSharp.Control

#nowarn "9" //suppress native interop warning
type Recorder(audioFormat:AudioFormat) =
    let r = lazy(RecordState.Create(audioFormat))   
    let channel = lazy(Channel.CreateBounded<byte[]>(30))
    let mutable _cancelToken : CancellationTokenSource option = None
    
    let startRecordLoop() =
        if _cancelToken.IsNone then
            _cancelToken <- Some (new CancellationTokenSource()) 
            let comp = async {                
                r.Value.mic.Start()
                while _cancelToken.IsSome && not _cancelToken.Value.Token.IsCancellationRequested do                     
                     let samples = r.Value.mic.AvailableSamples
                     if samples > 0 then
                        let sz = samples * audioFormat.FrameSize
                        let audioBuffer : byte[] = Array.zeroCreate (sz)
                        let ptr = fixed audioBuffer
                        let ptr2 = NativeInterop.NativePtr.toVoidPtr ptr
                        r.Value.mic.CaptureSamples(ptr2, samples)
                        r.Value.CheckError("capture samples")
                        Utils.debug $"{samples} bytes {sz} %A{audioBuffer.[0..20]}" 
                        let r = channel.Value.Writer.TryWrite(audioBuffer)
                        if not r then Log.warn $"record dropped {sz} bytes"
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
                if r.IsValueCreated then (r.Value :> IDisposable).Dispose()
                if channel.IsValueCreated then channel.Value.Writer.Complete()
                    
    let mute() =
        if r.IsValueCreated then
            r.Value.mic.Stop()
            r.Value.CheckError("mic stop")
            
    let unmute() =
        if r.IsValueCreated then
            r.Value.mic.Start()
            r.Value.CheckError("mic start")

    interface IRecorder with             
        member this.Record() = startRecordLoop()
        member this.Mute() =  mute()
        member this.Unmute() = unmute()
        member this.Stop() = stop()    
        member this.Channel = channel.Value

    interface IDisposable with member _.Dispose() = stop()
