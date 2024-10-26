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
    }

and Msg = 
    | Export 
    | Play  
    | PlayDone
    | SetWeight of string
    | EventError of exn    
    | SessionCreated
    | SessionEnded
    | Log of string
    | RecordStartStop
    | SetRecorder of Plugin.Maui.Audio.IAudioRecorder option * Plugin.Maui.Audio.IAudioSource option
    | Error of exn

type CmdMsg = SemanticAnnounce of string 
