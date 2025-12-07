namespace RT.Assistant.WorkFlow
open Fabulous
open RT.Assistant
open RTFlow

module StateMachine =
    type SubState = {
        mailbox : System.Threading.Channels.Channel<Msg> //background messages        
        viewRef:ViewRef<Microsoft.Maui.Controls.HybridWebView>
        bus : WBus<FlowMsg,AgentMsg>
        usage : Map<string,Usage>
    }
    
    let rec (|Txn|M|)  (s_ret,ss:SubState,msg) = //common message processing for each state
        match msg with
        | W_Err e                       -> Txn(F(s_terminate false ss,[Ag_FlowError e]))                   //error: switch to s_terminate; send error to app
        | W_Msg (Fl_Usage (id,usg))     -> let ss = {ss with usage = Usage.combineUsage ss.usage}
                                            let ss = {ss with task = ss.task.appendUsage (id,usg)}       //accumulate usage and return to same state
                                            Txn(F(s_ret ss, [APo_Usage ss.task.usage]))                  //  - also send usage to app
        | W_Msg (APi_TerminateTask x)    -> Txn(F(s_terminate false ss,[APo_Done {|abnormal = x.abnormal; task = ss.task|}]))    //done: switch to s_terminate; send results to app
        | W_Msg msg                      -> M msg                                                        //to be handled by the state 

    and s_start ss msg = async {
        Log.info $"{nameof s_start}, '{ss.task.id}', {ss.cuaLoopCount}"
        match s_start,ss,msg with 
        | Txn s                         -> return s
        | M APi_Start                   -> do! ss.task.driver.start ss.task.target                             //start browser - this takes some time
                                           let! vs,dims,_ = ss.task.driver.snapshot()
                                           let ss = {ss with screenDimensions=dims}
                                           return! reset s_cua ss 
        | x                             -> Log.warn $"{nameof s_start}: expecting APi_Start but got {x}"
                                           return F(s_start ss,[])
    }
    
    //main cua loop control
    and s_cua ss msg = async {
        Log.info $"{nameof s_cua}, '{ss.task.id}', {ss.cuaLoopCount}"
        match s_cua,ss,msg with 
        | Txn s                            -> return s
        | M (CUAi_ComputerCall (_,_,_)) when ss.cuaLoopCount > C.MAX_CUA_CALLS_IN_TASK ->      
                                            Log.warn $"{nameof s_cua}: exceeded max cua calls {ss.cuaLoopCount}"
                                            return! reset s_cua ss
        | M (CUAi_ComputerCall (cc,text,msgs)) ->            
                                            text |> Option.iter Log.info
                                            let ss = {ss with task.cuaMessages = msgs}.incrCuaLoopCount()
                                            let cuaCalls,pendingCalls = splitCalls cc
                                            let! ss,cuaResults = performActions ss cuaCalls
                                            let req = cuaLoopRequest ss cuaResults pendingCalls
                                            let actMsgs = cuaCalls |> List.collect fst |> List.map (Actions.actionToString>>APo_Action)
                                            return F(s_cua ss,CUAo_Req req::actMsgs) //txn back to s_cua; send action to app and loop
        | M (CUAi_NoComputerCall (_,_))  -> return! reset s_cua ss
        | x                              -> return ignoreMsg (s_cua ss) x (nameof s_cua)
    }

    and s_terminate startedCancel ss msg = async {
        if not startedCancel then terminate ss.cts ss.task.bus
        Log.info $"{nameof s_terminate}, '{ss.task.id}', {ss.cuaLoopCount}"
        Log.info $"s_terminate: message ignored {msg}"
        return F(s_terminate true ss,[])
    }

