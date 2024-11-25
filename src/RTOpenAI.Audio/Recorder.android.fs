namespace RTOpenAI.Audio.Android
open RTOpenAI.Audio

#if ANDROID 
open Android.Media
type Recorder(audioFormat) = 
    interface IRecorder with
        member this.Channel: System.Threading.Channels.Channel<byte array> = 
            raise (System.NotImplementedException())
        member this.Mute(): unit = 
            raise (System.NotImplementedException())
        member this.Record(): unit = 
            raise (System.NotImplementedException())
        member this.Stop(): unit = 
            raise (System.NotImplementedException())
        member this.Unmute(): unit = 
            raise (System.NotImplementedException()) 

#endif


