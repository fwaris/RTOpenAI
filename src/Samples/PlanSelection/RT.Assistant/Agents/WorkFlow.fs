namespace RT.Assistant.WorkFlow

open System.Threading
open Fabulous
open RT.Assistant
open RTFlow
open RTOpenAI.Events
open FsAICore

module StateMachine =
    type SubState = {
        mailbox : System.Threading.Channels.Channel<Msg> //background messages        
        viewRef:ViewRef<Microsoft.Maui.Controls.HybridWebView>
        bus : WBus<FlowMsg,AgentMsg>
        usage : FsAICore.UsageMap
        conn : RTOpenAI.Api.Connection
    }
    
    let startAgents (ss:SubState) = async {
        AppAgent.start ss.mailbox ss.bus
        QueryAgent.start ss.viewRef ss.bus
        CodeGenAgent.start ss.viewRef ss.bus
        VoiceAgent.start ss.conn ss.bus
    }
    
    /// log that a message was ignored in some state
    let ignoreMsg s msg name =
        Log.warn $"{name}: ignored message {msg}"
        F(s,[])
    
    let rec terminate isAbnormal (ss:SubState) =
        async {
            Log.info "terminating flow ..."
            RTOpenAI.Api.Connection.close ss.conn
            do! Async.Sleep(1000)        
            ss.bus.Close()
        }
        |> Async.Start
        F(s_terminate ss, [Ag_FlowDone {|abnormal=isAbnormal|}])
          
    and (|Txn|M|)  (s_ret,ss:SubState,msg) = //common message processing for each state
        match msg with
        | W_Err e                       -> Txn(terminate true ss)          //error: switch to s_terminate; send error to app
        | W_Msg (Fl_Usage usg)          -> let ss = {ss with usage = FsAICore.Usage.combineUsage ss.usage usg}  //accumulate token usage
                                           Txn(F(s_ret ss, []))                  
        | W_Msg (Fl_Terminate x)        -> Txn(terminate x.abnormal ss)    //done: switch to s_terminate; send results to app
        | W_Msg msg                     -> M msg                           //to be handled by the current state 

    and s_start ss msg = async {
        Log.info $"{nameof s_start}"
        match s_start,ss,msg with 
        | Txn s                         -> return s
        | M Fl_Start                    -> do! startAgents ss 
                                           return F(s_run ss,[])
        | x                             -> Log.warn $"{nameof s_start}: expecting APi_Start but got {x}"
                                           return F(s_start ss,[])
    }
        
    and s_run ss msg = async {
       Log.info $"{nameof s_run}"
       match s_run,ss,msg with 
       | Txn s                          -> return s
       | x                              -> return ignoreMsg (s_run ss) x (nameof s_run)
    }

    and s_terminate ss msg = async {
        Log.info $"s_terminate: message ignored {msg}"
        return F(s_terminate ss,[])
    }

    let create mailbox viewRef conn =
        let bus = WBus.Create()
        let ss = {mailbox=mailbox; viewRef=viewRef; conn=conn; usage=Map.empty; bus=bus}
        let s0 = s_start ss
        RTFlow.Workflow.run CancellationToken.None bus s0 //start the state machine with initial state
        {new IFlow<FlowMsg,AgentMsg> with
            member _.PostToFlow msg = bus.PostToFlow (W_Msg msg)
            member _.PostToAgent msg = bus.PostToAgent msg
            member _.Terminate() = bus.PostToFlow (W_Msg (Fl_Terminate {|abnormal=false|}))
        }
        