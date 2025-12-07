namespace RTFlow
open System
open System.Threading
open System.Threading.Channels
open FSharp.Control

type IFlow<'flowMsg,'agentMsg> = 
    abstract member PostToFlow: 'flowMsg -> unit
    abstract member PostToAgent: 'agentMsg -> unit
    abstract member Terminate : unit -> unit
    
type StepperHolder() =
    let sync = obj()
    let mutable handle : AutoResetEvent option = None
    member private _.Set(v) = lock sync (fun () -> handle <- v)
    member _.Value() = lock sync (fun () -> handle)
   
    member this.Reset() = 
        match this.Value() with
        | Some v -> try v.Set() |> ignore; v.Dispose() with _ -> Log.info "stepper disposed error"
        | None -> ()
        
    member this.MakeStepped(initVal) =
        this.Reset()
        this.Set(Some(new AutoResetEvent(initVal)))
        
    member this.MakeFree() =
        this.Reset()
        this.Set(None)

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

///A type that represents a state where 'state' is a function that takes an event and returns 
///the next state + a list output events
type F<'Event,'OutEvent> = F of ('Event -> Async<F<'Event,'OutEvent>>)*'OutEvent list

module Workflow =   
    ///accepts current state and input event,
    ///returns nextState and publishes any output events
    let private transition (bus:WBus<_,'output>) state event = async {
        let! (F(nextState,outEvents)) = state event
        //outEvents |> List.iter (fun m -> Log.info $"agnt: {m}"; bus.PostToAgent m)
        outEvents |> List.iter bus.PostToAgent
        return nextState
    }

    let run (token:CancellationToken) bus initState =
        let runner =  
            bus._flowChannel.Reader.ReadAllAsync(token)
            |> AsyncSeq.ofAsyncEnum
            //|> AsyncSeq.map(fun m -> Log.info $"Workflow message: {m.msgType}"; m)
            |> AsyncSeq.scanAsync (transition bus) initState
            |> AsyncSeq.iter (fun x -> ())

        let catcher = 
            async {
                match! Async.Catch runner with 
                | Choice1Of2 _   -> Log.info $"Workflow done"
                | Choice2Of2 exn -> (WE_Exn >> W_Err >> bus.PostToFlow) exn
                                    Log.exn(exn,"Workflow.run")                
            }
        Async.Start(catcher,token)
        
    ///Releases incoming messages one at a time - useful for debugging
    let runStepped (stepper:StepperHolder) printer (token:CancellationToken) bus initState =
        let runner =  
            bus._flowChannel.Reader.ReadAllAsync(token)
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.mapAsync(fun x -> async{
                let printed : string = printer x
                Log.info $"Queued: {printed}"
                match stepper.Value() with
                | Some h -> try do! Async.AwaitWaitHandle h |> Async.Ignore with _ -> Log.info $"Queue wait handle disposed" 
                | None -> ()
                return x
            })
            //|> AsyncSeq.map(fun m -> Log.info $"Workflow message: {m.msgType}"; m)
            |> AsyncSeq.scanAsync (transition bus) initState
            |> AsyncSeq.iter (fun x -> ())

        let catcher = 
            async {
                match! Async.Catch runner with 
                | Choice1Of2 _   -> Log.info $"Workflow done"
                | Choice2Of2 exn -> (WE_Exn >> W_Err >> bus.PostToFlow) exn
                                    Log.exn(exn,"Workflow.run")                
            }
        Async.Start(catcher,token)
