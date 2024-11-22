namespace RTOpenAI.Audio
open System.Threading.Channels
open Silk.NET.OpenAL

type AudioFormat = {
    Frequency: int
    BitsPerSample: int
    Channels : int
}
    with
        member x.FrameSize = x.BitsPerSample * x.Channels / 8
        member x.ByteRate = x.Frequency * x.FrameSize
        member x.BufferFormat = 
            match x.Channels, x.BitsPerSample with
            | 1,8   -> BufferFormat.Mono8
            | 1,16  -> BufferFormat.Mono16
            | 2,8   -> BufferFormat.Stereo8
            | 2,16  -> BufferFormat.Stereo16
            | _     -> failwith "Unsupported audio format"
        static member Default =
            {
                Frequency = 22050   //44.1KHz
                BitsPerSample = 16
                Channels = 1                
            }
        static member RTApi =
            {
                Frequency = 12000   //24KHz
                BitsPerSample = 16
                Channels = 1                
            }        

type IPlayer = 
    abstract member Play : unit -> unit
    abstract member Stop : unit -> unit
    abstract member Pause : unit -> unit
    abstract member IsPlaying : unit -> bool
    abstract member Channel : Channel<byte[]>

type IRecorder = 
    abstract member Record : unit -> unit
    abstract member Mute :  unit -> unit            
    abstract member Unmute :  unit -> unit
    abstract member Stop : unit -> unit
    abstract Channel : Channel<byte[]>

type IAudioManager = 
    abstract member CreatePlayer : AudioFormat -> IPlayer
    abstract member CreateRecorder : AudioFormat -> IRecorder


