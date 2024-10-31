namespace RTOpenAI
open System

type Model = 
    { 
        player : Plugin.Maui.Audio.IAudioPlayer option
        audioManager : Lazy<Plugin.Maui.Audio.IAudioManager>
        recorder : Plugin.Maui.Audio.IAudioRecorder option
        mailbox : System.Threading.Channels.Channel<Msg>        
        audioPipe : System.Threading.Channels.Channel<byte[]> option
        log : string list
        stream : System.IO.Stream option
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
    | Recorder_Set of (Plugin.Maui.Audio.IAudioRecorder*System.IO.Stream * System.Threading.Channels.Channel<byte[]>) option

