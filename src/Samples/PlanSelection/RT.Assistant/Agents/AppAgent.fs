namespace RT.Assistant.WorkFlow
open FSharp.Control
open RT.Assistant
open RTFlow
open RTFlow.Functions

//sniff agent messages and notify app for selected ones
module AppAgent =
    type internal State = {
        mailbox : System.Threading.Channels.Channel<Msg> //background messages
        bus : WBus<FlowMsg,AgentMsg>     
    }
        with member this.Send (msg:Msg) =  this.mailbox.Writer.TryWrite msg |> ignore
    
    let internal update (st:State) msg = async {
        match msg with
        | Ag_Query codeGenReq -> st.Send(Log_Append codeGenReq.query)
        | Ag_PrologAnswer s -> st.Send(Log_Append s)
        | Ag_FlowError err -> st.Send(Log_Append (err.ErrorText))
        | Ag_FlowDone e -> st.Send(Log_Append $"Flow done abnormal={e.abnormal}")
        | Ag_Prolog code -> st.Send(SetCode code); st.Send(Log_Append $"Prolog Query:\r\npredicates:\r\n{code.predicates
                                                               }\r\nquery:\r\n{code.query}")
        | Ag_GetPlanDetails (_,t) -> st.Send(Log_Append $"Details requested for plan:\n{t}")
        | _ -> ()
        return st
    }

    let start mailbox (bus:WBus<FlowMsg, AgentMsg>) =
        let st0 = {mailbox = mailbox; bus=bus}
        let channel = bus.agentChannel.Subscribe("app")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update st0
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow
   



