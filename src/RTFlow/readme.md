# RTFlow

A library for realtime multi-agent systems.

See [write-up](https://github.com/fwaris/RTOpenAI/blob/master/src/Samples/PlanSelection/RT.Assistant/docs/writeup.md)

To understand how to use this library, see the [RT.Assistant](https://github.com/fwaris/RTOpenAI/tree/master/src/Samples/PlanSelection/RT.Assistant#readme) sample code.

## AgentBus

`AgentBus<'msg>` is the reusable agent pub/sub layer used by RTFlow. It can be used independently when you only need named agents sharing a message stream:

```fsharp
open System.Threading
open RTFlow

let bus = AgentBus<string>(CancellationToken.None)
let agent = bus.Subscribe("logger")

bus.Publish("hello")

async {
    let! msg = agent.ReadAsync().AsTask() |> Async.AwaitTask
    printfn "%s" msg
}
|> Async.Start
```

`WBus<'flowMsg,'agentMsg>` now composes flow routing with `AgentBus<'agentMsg>`. `bus.agentChannel.Subscribe(...)` and `bus.AgentBus.Subscribe(...)` both expose read-only message streams.
