namespace RTFlow
open System
open System.Threading
open System.Threading.Channels
open FSharp.Control

type IFlow<'flowMsg,'agentMsg> = 
    abstract member PostToFlow: 'flowMsg -> unit
    abstract member PostToAgent: 'agentMsg -> unit
    abstract member Terminate : unit -> unit
    
 ///Holds AutoResetEvent that can be used to 'step' the workflow.
 /// Message to flow is blocked until the event released. Usefule for debugging.
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
        
    ///Releases incoming messages when event in stepper is signaled. Useful for debugging
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
