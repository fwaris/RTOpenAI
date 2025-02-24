namespace RT.Assistant

open Fabulous
open RT.Assistant.Plan
open RTOpenAI.Api.Events
          
exception InputKeyExn
type Role = User | Assistant
type Message = {Role : Role; Content: string}

type Model = 
    {
        mailbox : System.Threading.Channels.Channel<Msg> //background messages        
        connection : RTOpenAI.Api.Connection option
        sessionState : RTOpenAI.WebRTC.State
        log : string list
        isActive : bool
        conversation : string list
        modelId : string
        item : string
        hybridView : ViewRef<Microsoft.Maui.Controls.HybridWebView>
        code : CodeGenResp
        fontSize : float
    }

and Msg =
    | EventError of exn        
    | WebRTC_StateChanged of RTOpenAI.WebRTC.State
    | Cn_Set of RTOpenAI.Api.Connection option
    | Cn_StartStop of unit
    | Cn_Connect of RTOpenAI.Api.Connection
    | Cn_EnsureKey_Start    
    | Cn_Started of Session
    | Log_Append of string
    | Log_Clear
    | Nop
    | Settings_Show
    | Active
    | InActive
    | BackButtonPressed
    | InputKey of exn
    | ItemStarted
    | ItemAdded of string
    | SubmitCode
    | SetQuery of string
    | SetConsult of string
    | SetCode of CodeGenResp
    | FontLarger
    | FontSmaller
