namespace RTOpenAI.Audio.Android
open RTOpenAI.Audio
#if ANDROID

type Player(audioFormat) = 
    interface IPlayer with
        member this.Channel: System.Threading.Channels.Channel<byte array> = 
            raise (System.NotImplementedException())
        member this.IsPlaying(): bool = 
            raise (System.NotImplementedException())
        member this.Pause(): unit = 
            raise (System.NotImplementedException())
        member this.Play(): unit = 
            raise (System.NotImplementedException())
        member this.Stop(): unit = 
            raise (System.NotImplementedException())

#endif

