namespace RTOpenAI
open System
open LibVLCSharp.Shared

type PlayState = {Lib:LibVLC; Player:MediaPlayer; Resources:IDisposable list; Pipe:System.Threading.Channels.Channel<byte[]>}

type Model = 
    {         
        player : PlayState option
        audioManager : Lazy<Plugin.Maui.Audio.IAudioManager>
        recorder : Plugin.Maui.Audio.IAudioRecorder option
        mailbox : System.Threading.Channels.Channel<Msg>                
        log : string list
    }

and Msg = 
    | Export 
    | Play_Start
    | Play_Started of PlayState
    | Play_Stop  
    | EventError of exn    
    | Session_Created
    | Session_Ended
    | Log_Append of string
    | Log_Clear
    | Recorder_StartStop
    | Recorder_Set of (Plugin.Maui.Audio.IAudioRecorder*System.IO.Stream * System.Threading.Channels.Channel<byte[]>) option

