namespace RTFlow
open System
open System.Threading.Channels
open System.Threading
open FSharp.DI

type LogCategory = class end

module Log =
    ///allow for turning message loggin on/off at runtime
    let mutable debug_logging = false
    let private log = DI.loggerLazy<LogCategory>()
    let info (msg:string) = log.Value.info msg
    let warn (msg:string) = log.Value.warn msg
    let error (msg:string) = log.Value.error msg
    let exn (exn:exn,msg) = log.Value.exn (exn,msg)
    let trace (msg:string) = log.Value.trace msg

    let init (sp:IServiceProvider) =
        DI.init sp
        info "Initialized"

module C =
    let MAX_BUS_QUEUE_DEPTH = 10

type PubSub<'T>(cancellationToken:CancellationToken) =
    let bus = AgentBus<'T>(cancellationToken)
    
    /// Publishes a message to all subscribers
    member _.Publish(msg: 'T) =
        bus.Publish msg

    /// Subscribe and receive messages.
    member _.Subscribe(name:string) : ChannelReader<'T> =
        bus.Subscribe name
        
    member _.UnSubscribe(name:string) =
        bus.Unsubscribe name

    member _.Unsubscribe(name:string) =
        bus.Unsubscribe name
        
    member _.Close() =
        bus.Close()

    member _.AgentBus = bus
            


type WErrorType = WE_Error of string | WE_Exn of exn
    with member this.ErrorText with get() = match this with WE_Error s -> s | WE_Exn ex -> ex.Message

type W_Msg_In<'flowMsg> = 
    | W_Msg of 'flowMsg
    | W_Err of WErrorType

    with 
        member this.msgType = 
            match this with 
            | W_Msg t -> $"W_App {t}"
            | W_Err e -> $"W_Error {e}" 
            
type WBus<'flowMsg,'agentMsg> = 
    {
        ///Channel for messages going into the flow.
        ///Use PostToFlow function instead of directly using this property, for consistent logging
        _flowChannel  : Channel<W_Msg_In<'flowMsg>>

        ///Channel to send messages to non-flow actors; the app and zero or more agents
        agentChannel  : PubSub<'agentMsg>
        
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
            this.agentChannel.Close()
            this.tokenSource.Cancel()
        member this.PostToFlow msg = 
            match this._flowChannel.Writer.TryWrite msg with 
            | false -> Log.warn $"Bus dropped message {msg}"
            | true  -> ()
        member this.AgentBus = this.agentChannel.AgentBus
        member this.PostToAgent msg =
            this.AgentBus.Publish msg
        member this.AwaitAgentMsg(filter:('agentMsg->bool),?timeoutMs:int) : Async<'agentMsg option> =
            this.AgentBus.AwaitMessage(filter, ?timeoutMs=timeoutMs)
           
