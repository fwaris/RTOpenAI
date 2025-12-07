namespace RT.Assistant.WorkFlow
open FSharp.Control
open RT.Assistant
open RTFlow
open RTFlow.Functions

module AppAgent =
    type internal State = {
        mailbox : System.Threading.Channels.Channel<Msg> //background messages
        bus : WBus<FlowMsg,AgentMsg>     
    }
        with member this.Send (msg:Msg) =  this.mailbox.Writer.TryWrite msg |> ignore
    
    let internal update (st:State) msg = async {
        match msg with
        | Ag_Query (_,q) -> st.Send(Log_Append q)
        | _ -> ()
        return st
    }

    let start mailbox (bus:WBus<FlowMsg, AgentMsg>) =
        let st0 = {mailbox = mailbox; bus=bus}
        let channel = bus.agentChannel.Subscribe("codegen")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update st0
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow
   



