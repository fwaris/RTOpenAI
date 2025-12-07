namespace RTFlow
open System
open System.Threading.Channels
open System.Threading
open FSharp.Control

module C =
    let MAX_BUS_QUEUE_DEPTH = 10

type PubSub<'T>(cancellationToken:CancellationToken) =
    let central = Channel.CreateBounded<'T>(C.MAX_BUS_QUEUE_DEPTH)
    let subscribers = System.Collections.Concurrent.ConcurrentDictionary<string, Channel<'T>>()
    
    do
        let comp = 
            central.Reader.ReadAllAsync(cancellationToken)
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.iterAsync(fun m -> async {
                for kvp in subscribers do
                    let r = kvp.Value.Writer.TryWrite(m)
                    if not r then
                        Log.info $"Dropped msg {m} to {kvp.Key}"
            })
        async {
            match! Async.Catch(comp) with
            | Choice1Of2 _ -> Log.info "Bus stopped"
            | Choice2Of2 ex -> Log.exn(ex,"Error message dispatch for bus")
            for kvp in subscribers.Values do
                kvp.Writer.Complete()
        }
        |> Async.Start
    
    /// Publishes a message to all subscribers
    member _.Publish(msg: 'T) =
        central.Writer.TryWrite(msg) |> ignore

    /// Subscribe and receive messages; returns a Subscription
    member _.Subscribe(name:string) =
        if subscribers.ContainsKey name then
            failwith $"{name} is already registered in bus"
        let channel = Channel.CreateBounded<'T>(C.MAX_BUS_QUEUE_DEPTH)
        subscribers[name] <- channel
        channel
