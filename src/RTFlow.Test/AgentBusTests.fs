module RTFlow.Test.AgentBusTests

open System
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open NUnit.Framework
open RTFlow

type private FlowMsg =
    | Start

type private AgentMsg =
    | Message of int

let private readOne (reader: ChannelReader<'T>) =
    reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds 2.).GetAwaiter().GetResult()

let private waitTask (task: Task) =
    task.WaitAsync(TimeSpan.FromSeconds 2.).GetAwaiter().GetResult()

let private waitTaskWithResult (task: Task<'T>) =
    task.WaitAsync(TimeSpan.FromSeconds 2.).GetAwaiter().GetResult()

let private waitCanRead (reader: ChannelReader<'T>) =
    reader.WaitToReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds 2.).GetAwaiter().GetResult()

let private waitUntil (predicate: unit -> bool) =
    let deadline = DateTimeOffset.UtcNow.AddSeconds 2.
    let mutable doneWaiting = predicate()

    while not doneWaiting && DateTimeOffset.UtcNow < deadline do
        Thread.Sleep 20
        doneWaiting <- predicate()

    Assert.That(doneWaiting, Is.True)

[<Test>]
let ``AgentBus publishes to multiple named subscribers`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    let first = bus.Subscribe "first"
    let second = bus.Subscribe "second"

    bus.Publish 42

    Assert.That(readOne first, Is.EqualTo 42)
    Assert.That(readOne second, Is.EqualTo 42)
    bus.Close()

[<Test>]
let ``AgentBus rejects duplicate subscriber names`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    bus.Subscribe "agent" |> ignore

    Assert.Throws<InvalidOperationException>(fun () -> bus.Subscribe "agent" |> ignore)
    |> ignore

    bus.Close()

[<Test>]
let ``AgentBus unsubscribe completes reader and removes subscriber`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    let reader = bus.Subscribe "agent"

    bus.Unsubscribe "agent"

    Assert.That(reader.Completion.Wait(TimeSpan.FromSeconds 2.), Is.True)
    Assert.DoesNotThrow(fun () -> bus.Publish 1)
    bus.Close()

[<Test>]
let ``AgentBus unsubscribe drains pending messages and completes reader`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    let reader = bus.Subscribe "agent"

    bus.Publish 1
    Assert.That(waitCanRead reader, Is.True)

    bus.Unsubscribe "agent"

    Assert.That(reader.Completion.Wait(TimeSpan.FromSeconds 2.), Is.True)

    let mutable remaining = 0
    Assert.That(reader.TryRead(&remaining), Is.False)

    Assert.DoesNotThrow(fun () -> bus.Subscribe "agent" |> ignore)
    bus.Close()

[<Test>]
let ``AgentBus RunAsync processes messages and completes when closed`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    let processed = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)

    let update state msg =
        async {
            let next = state + msg

            if next >= 3 then
                processed.TrySetResult next |> ignore

            return next
        }

    let runner = Async.StartAsTask(bus.RunAsync("runner", 0, update), cancellationToken = cts.Token)

    waitUntil (fun () ->
        if not processed.Task.IsCompleted then
            bus.Publish 1

        processed.Task.IsCompleted)

    Assert.That(waitTaskWithResult processed.Task, Is.GreaterThanOrEqualTo 3)
    bus.Close()
    waitTask runner

[<Test>]
let ``AgentBus RunWithReadyAsync signals readiness after subscription`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    let ready = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)
    let processed = TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)

    let update _ msg =
        async {
            processed.TrySetResult msg |> ignore
            return msg
        }

    let runner =
        Async.StartAsTask(
            bus.RunWithReadyAsync("readyAgent", ready, 0, update),
            cancellationToken = cts.Token
        )

    waitTask ready.Task
    bus.Publish 99

    Assert.That(waitTaskWithResult processed.Task, Is.EqualTo 99)
    bus.Close()
    waitTask runner

[<Test>]
let ``AgentBus AwaitMessage returns first matching message`` () =
    use cts = new CancellationTokenSource()
    let bus = AgentBus<int>(cts.Token)
    let awaited = Async.StartAsTask(bus.AwaitMessage((fun msg -> msg > 1), timeoutMs = 1000), cancellationToken = cts.Token)

    waitUntil (fun () ->
        bus.Publish 1
        bus.Publish 2
        awaited.IsCompleted)

    Assert.That(waitTaskWithResult awaited, Is.EqualTo(Some 2))
    bus.Close()

[<Test>]
let ``WBus legacy agentChannel subscription receives PostToAgent messages`` () =
    let bus = WBus<FlowMsg, AgentMsg>.Create()
    let reader = bus.agentChannel.Subscribe "legacy"

    bus.PostToAgent(Message 7)

    Assert.That(readOne reader, Is.EqualTo(Message 7))
    bus.Close()

[<Test>]
let ``WBus AgentBus subscription receives PostToAgent messages`` () =
    let bus = WBus<FlowMsg, AgentMsg>.Create()
    let reader = bus.AgentBus.Subscribe "direct"

    bus.PostToAgent(Message 11)

    Assert.That(readOne reader, Is.EqualTo(Message 11))
    bus.Close()
