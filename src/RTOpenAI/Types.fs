namespace RTOpenAI
open RTOpenAI.Audio
open System

type Model = 
    {
        audioFormat : AudioFormat
        session : Session option
        player : IPlayer option
        recorder : IRecorder option                        
        mailbox : System.Threading.Channels.Channel<Msg>
        log : string list
    }

and Msg = 
    | Export 
    | Play_StartStop
    | Play_Started of IPlayer option
    | EventError of exn        
    | Session_Set of (Session*IRecorder*IPlayer) option
    | Session_Connect of Session
    | Session_StartStop
    | Log_Append of string
    | Log_Clear
    | Recorder_StartStop
    | Recorder_Set of (IRecorder * string) option
    | MachineMsg of MachineMsg
    | Key_Get
    | Key_Value of string option
    | Key_Set
    | Nop

