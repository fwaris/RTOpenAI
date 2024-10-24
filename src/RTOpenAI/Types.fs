namespace RTOpenAI
open System

type Measure = { Date: DateTime; Weight: float }

type Model = 
    { 
        weights  : Measure list        
        audioManager : Lazy<Plugin.Maui.Audio.IAudioManager>
    }

type Msg = 
    | Export 
    | Clicked  
    | SetWeight of string
    | EventError of exn    
    | SessionCreated
    | SessionEnded
    | Log of string

type CmdMsg = SemanticAnnounce of string 
