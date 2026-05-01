namespace RTFlow

open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open FSharp.Control
open FSharp.DI

/// Reusable pub/sub bus for named realtime agents.
type AgentBus<'msg>(cancellationToken: CancellationToken) as this =
    let ingress = Channel.CreateUnbounded<'msg>()
    let subscribers = ConcurrentDictionary<string, Channel<'msg>>()
    let sync = obj()
    let mutable isClosed = false
    let log = DI.loggerLazy<AgentBus<'msg>>()

    let isBusClosed() = lock sync (fun () -> isClosed)

    let completeSubscriber (channel: Channel<'msg>) =
        channel.Writer.TryComplete() |> ignore

        let mutable msg = Unchecked.defaultof<'msg>

        while channel.Reader.TryRead(&msg) do
            msg <- Unchecked.defaultof<'msg>

    let markClosed() =
        lock sync (fun () ->
            if isClosed then
                false
            else
                isClosed <- true
                true)

    let subscribeChannel (name: string) =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Agent subscription name is required."

        if isBusClosed() then
            invalidOp "AgentBus is closed."

        let channel = Channel.CreateUnbounded<'msg>()

        if subscribers.TryAdd(name, channel) then
            channel
        else
            completeSubscriber channel
            invalidOp $"Agent '{name}' is already subscribed."

    let dispatch (msg: 'msg) =
        async {
            for entry in subscribers do
                try
                    do! entry.Value.Writer.WriteAsync(msg, cancellationToken).AsTask() |> Async.AwaitTask
                with
                | :? OperationCanceledException when cancellationToken.IsCancellationRequested ->
                    ()
                | :? ChannelClosedException ->
                    ()
        }

    do
        let worker =
            ingress.Reader.ReadAllAsync(cancellationToken)
            |> AsyncSeq.iterAsync dispatch

        Async.Start(
            async {
                try
                    try
                        do! worker
                    with
                    | :? OperationCanceledException when cancellationToken.IsCancellationRequested ->
                        ()
                    | ex ->
                        log.Value.exn(ex, "AgentBus dispatch failed.")
                finally
                    markClosed() |> ignore

                    for subscriber in subscribers.Values do
                        completeSubscriber subscriber

                    subscribers.Clear()
                    ingress.Writer.TryComplete() |> ignore
            },
            cancellationToken
        )

    /// Publishes a message to all current subscribers.
    member _.Publish(msg: 'msg) =
        if not (isBusClosed()) then
            ingress.Writer.TryWrite(msg) |> ignore

    /// Subscribe a named agent and receive the published message stream.
    member _.Subscribe(name: string) : ChannelReader<'msg> =
        (subscribeChannel name).Reader

    /// Removes a named subscription and completes its reader.
    member _.Unsubscribe(name: string) =
        match subscribers.TryRemove name with
        | true, channel ->
            completeSubscriber channel
        | _ ->
            ()

    /// Completes the bus and all subscriber streams.
    member _.Close() =
        if markClosed() then
            ingress.Writer.TryComplete() |> ignore

            for subscriber in subscribers.Values do
                completeSubscriber subscriber

            subscribers.Clear()

    /// Runs an agent as an async state machine subscribed to this bus.
    member _.RunAsync(name: string, initialState: 'state, update: 'state -> 'msg -> Async<'state>) =
        async {
            let reader = this.Subscribe(name)

            try
                do!
                    reader.ReadAllAsync(cancellationToken)
                    |> AsyncSeq.scanAsync update initialState
                    |> AsyncSeq.iter (fun _ -> ())
            finally
                this.Unsubscribe(name)
        }

    /// Runs an agent and signals readiness after the subscription is active.
    member _.RunWithReadyAsync
        (
            name: string,
            ready: TaskCompletionSource<unit>,
            initialState: 'state,
            update: 'state -> 'msg -> Async<'state>
        ) =
        async {
            let reader = this.Subscribe(name)
            ready.TrySetResult() |> ignore

            try
                do!
                    reader.ReadAllAsync(cancellationToken)
                    |> AsyncSeq.scanAsync update initialState
                    |> AsyncSeq.iter (fun _ -> ())
            finally
                this.Unsubscribe(name)
        }

    /// Waits for the first published message that matches the filter.
    member _.AwaitMessage(filter: 'msg -> bool, ?timeoutMs: int) : Async<'msg option> =
        async {
            let name = "temp_" + Guid.NewGuid().ToString("N")
            let reader = this.Subscribe(name)

            try
                let msgComp =
                    reader.ReadAllAsync(cancellationToken)
                    |> AsyncSeq.skipWhile (filter >> not)
                    |> AsyncSeq.tryFirst

                let! result =
                    async {
                        match timeoutMs with
                        | Some timeoutMs ->
                            let! finder = Async.StartChild(msgComp, timeoutMs)
                            return! finder
                        | None ->
                            return! msgComp
                    }
                    |> Async.Catch

                match result with
                | Choice1Of2 msg ->
                    return msg
                | Choice2Of2 (:? TimeoutException) ->
                    return None
                | Choice2Of2 (:? OperationCanceledException) when cancellationToken.IsCancellationRequested ->
                    return None
                | Choice2Of2 ex ->
                    log.Value.exn(ex, nameof this.AwaitMessage)
                    return None
            finally
                this.Unsubscribe(name)
        }
