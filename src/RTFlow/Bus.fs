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
            central.Writer.Complete()
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


type WErrorType = WE_Error of string | WE_Exn of exn
    with member this.ErrorText with get() = match this with WE_Error s -> s | WE_Exn ex -> ex.Message

type W_Msg_In<'input> = 
    | W_Msg of 'input
    | W_Err of WErrorType

    with 
        member this.msgType = 
            match this with 
            | W_Msg t -> $"W_App {t}"
            | W_Err e -> $"W_Error {e}" 
            
type WBus<'input,'output> = 
    {
        ///Channel for messages going into the flow.
        ///Use PostToFlow function instead of directly using this property, for consistent logging
        _flowChannel  : Channel<W_Msg_In<'input>>

        ///Channel to send messages to non-flow actors; the app and zero or more agents
        agentChannel  : PubSub<'output>
        
        tokenSource : CancellationTokenSource
    }
    with 
        static member Create<'input,'output>(?maxQueue) =
            let cts = new CancellationTokenSource()
            let maxQueue = defaultArg maxQueue C.MAX_BUS_QUEUE_DEPTH
            {
                _flowChannel  = Channel.CreateBounded<W_Msg_In<'input>>(maxQueue)
                agentChannel = PubSub<'output>(cts.Token)
                tokenSource = cts
            }
        member this.Close() =
            this._flowChannel.Writer.TryComplete() |> ignore
            this.tokenSource.Cancel()
        member this.PostToFlow msg = 
            match this._flowChannel.Writer.TryWrite msg with 
            | false -> Log.warn $"Bus dropped message {msg}"
            | true  -> ()
        member this.PostToAgent msg =
            this.agentChannel.Publish msg
