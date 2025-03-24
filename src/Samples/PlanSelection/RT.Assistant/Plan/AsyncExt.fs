module AsyncExts
open System
open FSharp.Control
open System.Threading
open System.Threading.Tasks

module Async =
    let map f a = async.Bind(a, f >> async.Return)

module AsyncSeq =
    let mapAsyncParallelThrottled (parallelism:int) (f:'a -> Async<'b>) (s:AsyncSeq<'a>) : AsyncSeq<'b> = asyncSeq {
        use mb = MailboxProcessor.Start (ignore >> async.Return)
        use sm = new SemaphoreSlim(parallelism)
        let! err =
            s
            |> AsyncSeq.iterAsync (fun a -> async {
            let! _ = sm.WaitAsync () |> Async.AwaitTask
            let! b = Async.StartChild (async {
                try return! f a
                finally sm.Release () |> ignore })
            mb.Post (Some b) })
            |> Async.map (fun _ -> mb.Post None)
            |> Async.StartChildAsTask
        yield!
            AsyncSeq.unfoldAsync (fun (t:Task) -> async{
            if t.IsFaulted then
                return None
            else
                let! d = mb.Receive()
                match d with
                | Some c ->
                    let! d' = c
                    return Some (d',t)
                | None -> return None
            })
            err
    }
(*
      //implementation possible within AsyncSeq, with the supporting code available there
      let mapAsyncParallelThrottled (parallelism:int) (f:'a -> Async<'b>) (s:AsyncSeq<'a>) : AsyncSeq<'b> = asyncSeq {
        use mb = MailboxProcessor.Start (ignore >> async.Return)
        use sm = new SemaphoreSlim(parallelism)
        let! err =
          s
          |> iterAsync (fun a -> async {
            do! sm.WaitAsync () |> Async.awaitTaskUnitCancellationAsError
            let! b = Async.StartChild (async {
              try return! f a
              finally sm.Release () |> ignore })
            mb.Post (Some b) })
          |> Async.map (fun _ -> mb.Post None)
          |> Async.StartChildAsTask
        yield!
          replicateUntilNoneAsync (Task.chooseTask (err |> Task.taskFault) (async.Delay mb.Receive))
          |> mapAsync id }
*)

    let private _mapAsyncParallelUnits (units:string) (maxConcurrent:int) (unitsPerMinute:float) (f:'a -> Async<'b>) (s:AsyncSeq<int64*'a>) : AsyncSeq<'b> = asyncSeq {
        use mb = MailboxProcessor.Start (ignore >> async.Return)
        let resolution = 0.01 //minutes
        let mutable markTime = DateTime.Now.Ticks
        let mutable unitCount = 0L
        let mutable _inplay = 0L

        let incrUnits i = Interlocked.Add(&unitCount,i)
        let incrInPlay() = Interlocked.Increment(&_inplay) |> ignore
        let decInPlay() = Interlocked.Decrement(&_inplay) |> ignore
        let getUnits() = Interlocked.Read(&unitCount)
        let inPlay() = Interlocked.Read(&_inplay)

        let reset i t =
            Interlocked.Exchange(&unitCount,i) |> ignore
            Interlocked.Exchange(&markTime,t) |> ignore
            
        let currentRate() =
            let load = getUnits() |> float
            let inplay = inPlay()
            let elapsed = (DateTime.Now - DateTime(markTime)).TotalMinutes
            let rate =
                if load = 0.0 || elapsed = 0.001 then 
                    0.0
                else 
                    load / elapsed
            printfn $"rate %0.02f{rate} / in play {inplay}"
            rate
        

        let! err =
            s
            |> AsyncSeq.iterAsync (fun (load,a) -> async {
                while currentRate() > unitsPerMinute || inPlay() > maxConcurrent do
                    do! Async.Sleep (int (resolution * 60000.0))
                incrUnits load |> ignore
                incrInPlay()

                let! b = Async.StartChild (async {
                    try
                        let! b = f a
                        incrUnits -load |> ignore
                        decInPlay()
                        return b
                    finally () })
                mb.Post (Some b) })
            |> Async.map (fun _ -> mb.Post None)
            |> Async.StartChildAsTask
        yield!
            AsyncSeq.unfoldAsync (fun (t:Task) -> async{
            if t.IsFaulted then
                return None
            else
                let! d = mb.Receive()
                match d with
                | Some c ->
                    let! d' = c
                    return Some (d',t)
                | None -> return None
            })
            err
    }

    ///Invoke f in parallel while maintaining the tokens per minute rate.
    ///Input is a sequence of (tokens:unint64 *'a) where the tokens is the number of input tokens associated with value 'a.
    ///Note: ordering is not maintained
    let mapAsyncParallelTokenLimit maxConcurrent (tokensPerMinute:float) (f:'a -> Async<'b>) (s:AsyncSeq<int64*'a>) : AsyncSeq<'b> = 
        _mapAsyncParallelUnits "tokens" maxConcurrent tokensPerMinute f s

    ///Invoke f in parallel while maintaining opsPerSecond rate.
    ///Note: ordering is not maintained
    let mapAsyncParallelRateLimit maxConcurrent (opsPerSecond:float) (f:'a -> Async<'b>) (s:AsyncSeq<'a>) : AsyncSeq<'b> =
        _mapAsyncParallelUnits "ops" maxConcurrent (opsPerSecond * 60.0) f (s |> AsyncSeq.map (fun a -> 1L,a))

