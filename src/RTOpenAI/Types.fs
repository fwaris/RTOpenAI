namespace RTOpenAI
open System

type Model = 
    {
        audioFormat : AudioFormat
        player : Player option
        recorder : Recorder option
        outputFile : string option
        mailbox : System.Threading.Channels.Channel<Msg>                
        log : string list
    }

and Msg = 
    | Export 
    | Play_Start
    | Play_Started of Player option
    | Play_Stop  
    | EventError of exn    
    | Session_Created
    | Session_Ended
    | Log_Append of string
    | Log_Clear
    | Recorder_StartStop
    | Recorder_Set of (Recorder * string) option 

