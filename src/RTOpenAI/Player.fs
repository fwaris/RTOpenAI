namespace RTOpenAI
open System
open System.IO
open System.Diagnostics
open System.Threading
open System.Threading.Channels
open FSharp.Control
open Silk.NET.OpenAL
open FSharp.NativeInterop
#nowarn "9" //suppress native interop warning

type Player(audioFormat:AudioFormat) =
    let p = lazy(PlayState.Create())   
    let channel = lazy(Channel.CreateBounded<byte[]>(30))
    let mutable _cancelToken : CancellationTokenSource option = None
    
    let _dequeueBuffer() = 
        let mutable bcount = -1
        p.Value.al.GetSourceProperty(p.Value.source, GetSourceInteger.BuffersProcessed, &bcount)
        p.Value.CheckError("buffer count")
        if bcount > 0 then            
            let ptr = NativePtr.stackalloc<uint> 1
            NativePtr.set ptr 0 0u
            p.Value.al.SourceUnqueueBuffers(p.Value.source,1,ptr)
            p.Value.CheckError("unqueue buffer")
            Some (NativePtr.get ptr 0)
          else
            None
    
    let dequeueBuffer() =
        match _cancelToken with
        | Some _ -> _dequeueBuffer() 
        | None   -> None
                
    let enqueueChunk(chunk:byte[],b:uint) =
        p.Value.al.BufferData(b,audioFormat.BufferFormat,chunk,audioFormat.Frequency)
        p.Value.CheckError("buffer data")
        let ptr = NativePtr.stackalloc<uint> 1
        NativePtr.set ptr 0 b
        p.Value.al.SourceQueueBuffers(p.Value.source,1,ptr)
        p.Value.CheckError("queue buffer")
        
    let checkState() = 
        let mutable state = 0
        p.Value.al.GetSourceProperty(p.Value.source, GetSourceInteger.SourceState, &state)
        let st = enum<SourceState> state
        printfn $"%A{st}"
        
    let processChunk (chunk:byte[]) =
        async{
            let mutable go = true
            while go && _cancelToken.IsSome do
                match dequeueBuffer() with
                | Some b -> go <- false; enqueueChunk (chunk,b)
                | None -> checkState(); do! Async.Sleep 100
        }
        
    let readChunk() =
        task {
            let mutable chunk = Unchecked.defaultof<_>
            let! r =  channel.Value.Reader.WaitToReadAsync(_cancelToken.Value.Token)
            let  _ = channel.Value.Reader.TryRead(&chunk) 
            return chunk               
        }
        
    let primeBuffers() =
        task {
            for b in p.Value.buffers do
                let! chunk = readChunk()
                enqueueChunk(chunk,b)
        }
            
    let startPlayLoop() =
        if _cancelToken.IsNone then
            _cancelToken <- Some (new CancellationTokenSource())
            let comp = 
                async {
                    //at startup just queue all available buffers first
                    do! primeBuffers() |> Async.AwaitTask
                    p.Value.al.SourcePlay(p.Value.source)
                    p.Value.CheckError("play")
                    //then queue buffers as they become available from input data stream
                    do! 
                        asyncSeq {
                            let mutable chunk = [||]
                            while _cancelToken.IsSome && not _cancelToken.Value.Token.IsCancellationRequested do
                                let! r =  channel.Value.Reader.WaitToReadAsync(_cancelToken.Value.Token).AsTask() |> Async.AwaitTask
                                if r then
                                    let mutable buff : byte[] = Unchecked.defaultof<_>            
                                    let _ = channel.Value.Reader.TryRead(&chunk) 
                                    yield chunk
                        }
                        |> AsyncSeq.iterAsync processChunk
                }                                        
            async {
                match! Async.Catch comp with
                | Choice1Of2 _ -> printfn "play done"
                | Choice2Of2 ex -> Log.exn(ex,"Player.startPlayLoop")
            }
            |> Async.Start
        else
            Log.info "playLoop alreadyStarted"
              
    let cleanup()=
        if channel.IsValueCreated then channel.Value.Writer.TryComplete() |> ignore
        if p.IsValueCreated then (p.Value :> IDisposable).Dispose()
                
    let cancel() =
        match _cancelToken with
        | Some cts -> cts.Cancel(); _cancelToken <-None 
        | None -> ()

    let stop() =
        cancel()
        if p.IsValueCreated then p.Value.al.SourceStop(p.Value.source)
        cleanup()
    
    let play() =
        startPlayLoop()
        
    let pause() =
        cancel()
        if p.IsValueCreated then p.Value.al.SourcePause(p.Value.source)
    
    let check() = if p.IsValueCreated && p.Value.disposed then raise (ObjectDisposedException("Player"))
       
    member this.Stop = stop
    member this.Pause()  = check(); pause()
    member this.Play() = check(); play()
    member this.IsPlaying() = _cancelToken.IsSome
    member this.Channel = channel.Value

    interface IDisposable with
        member _.Dispose() = stop()