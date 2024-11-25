namespace RTOpenAI
open RTOpenAI.Audio
open System

type Model = 
    {
        audioFormat : AudioFormat
        player : IPlayer option
        recorder : IRecorder option
        outputFile : string option
        mailbox : System.Threading.Channels.Channel<Msg>                
        log : string list
    }

and Msg = 
    | Export 
    | Play_StartStop
    | Play_Started of IPlayer option
    | EventError of exn    
    | Session_Created
    | Session_Ended
    | Log_Append of string
    | Log_Clear
    | Recorder_StartStop
    | Recorder_Set of (IRecorder * string) option 

