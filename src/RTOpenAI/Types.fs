namespace RTOpenAI
open System

type Measure = { Date: DateTime; Weight: float }

type Model = 
    { 
        weights  : Measure list        
        audioManager : Lazy<Plugin.Maui.Audio.IAudioManager>
        recorder : Plugin.Maui.Audio.IAudioRecorder option
        isPlaying : bool
        audioSource : Plugin.Maui.Audio.IAudioSource option
        mailbox : System.Threading.Channels.Channel<Msg>
        log : string list
    }

and Msg = 
    | Export 
    | Play  
    | Play_Done
    | SetWeight of string
    | EventError of exn    
    | Session_Created
    | Session_Ended
    | Log_Append of string
    | Log_Clear
    | Recorder_StartStop
    | Recorder_Set of Plugin.Maui.Audio.IAudioRecorder option * Plugin.Maui.Audio.IAudioSource option

type CmdMsg = SemanticAnnounce of string 
