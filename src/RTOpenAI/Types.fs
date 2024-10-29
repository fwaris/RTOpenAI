namespace RTOpenAI
open System

type Model = 
    { 
        player : Player
        audioManager : Lazy<Plugin.Maui.Audio.IAudioManager>
        recorder : Plugin.Maui.Audio.IAudioRecorder option
        mailbox : System.Threading.Channels.Channel<Msg>        
        log : string list
    }

and Msg = 
    | Export 
    | Play_Stop  
    | EventError of exn    
    | Session_Created
    | Session_Ended
    | Log_Append of string
    | Log_Clear
    | Recorder_StartStop
    | Recorder_Set of Plugin.Maui.Audio.IAudioRecorder option

