namespace RT.Assistant.WorkFlow

open FSharp.Control
open Fabulous
open RT.Assistant.Plan
open RTFlow
open RTFlow.Functions

module QueryAgent =
    
    let getPlanDetails hybridWebView (planTile:string) =
        async {
            try
                let prolog = {Predicates=""; Query= $"plan('{planTile}',Category,Lines,Features)."}
                let! ans = PlanQuery.evalQuery hybridWebView prolog
                return ans
            with ex ->
                Log.exn (ex,"getPlanDetails")
                return raise ex
        }
     
    type internal State =
         {
            viewRef:ViewRef<Microsoft.Maui.Controls.HybridWebView>
            bus : WBus<FlowMsg,AgentMsg>
         }
     
    let internal update st msg = async {
         match msg with
         | Ag_GetPlanDetails (callId,title) ->
             let! ans = getPlanDetails st.viewRef title
             st.bus.PostToAgent(Ag_PlanDetails (callId,ans))
             return st
         | _ -> return st
     }

    let start viewRef (bus:WBus<FlowMsg, AgentMsg>) =
        let st0 = {viewRef = viewRef; bus=bus}
        let channel = bus.agentChannel.Subscribe("codegen")
        channel.Reader.ReadAllAsync()
        |> AsyncSeq.ofAsyncEnum
        |> AsyncSeq.scanAsync update st0
        |> AsyncSeq.iter(fun _ -> ())
        |> FlowUtils.catch bus.PostToFlow
   

