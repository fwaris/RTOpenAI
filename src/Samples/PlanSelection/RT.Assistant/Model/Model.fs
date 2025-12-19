namespace RT.Assistant
open Fabulous
open RT.Assistant.WorkFlow
open RTFlow
open RTOpenAI.Events
          
exception InputKeyExn
type Role = User | Assistant
type Message = {Role : Role; Content: string}

type Model = 
    {
        mailbox : System.Threading.Channels.Channel<Msg> //background messages        
        sessionState : RTOpenAI.WebRTC.State
        log : string list
        isActive : bool
        hybridView : ViewRef<Microsoft.Maui.Controls.HybridWebView>
        code : CodeGenResp
        fontSize : float
        flow : IFlow<FlowMsg,AgentMsg> option
    }

and Msg =
    | EventError of exn        
    | WebRTC_StateChanged of RTOpenAI.WebRTC.State
    | Cn_Set of IFlow<FlowMsg,AgentMsg> option
    | Cn_StartStop of unit
    | Cn_EnsureKey_Start    
    | Cn_Started of Session
    | Log_Append of string
    | Log_Clear
    | Nop
    | Settings_Show
    | Active
    | InActive
    | BackButtonPressed
    | SubmitCode
    | SetQuery of string
    | SetConsult of string
    | SetCode of CodeGenResp
    | FontLarger
    | FontSmaller
