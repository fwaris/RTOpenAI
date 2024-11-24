namespace RTOpenAI.Audio

#if ANDROID
type AudioManager() = 
    interface IAudioManager with
        member this.CreatePlayer(audioFormat) = new Android.Player(audioFormat)
        member this.CreateRecorder(audioFormat) = new Android.Recorder(audioFormat)
#else
    #if IOS
type AudioManager() = 
    interface IAudioManager with
        member this.CreatePlayer(audioFormat) = new Al.Player(audioFormat)
        member this.CreateRecorder(audioFormat) = new Al.Recorder(audioFormat, Some Ios.RecordingSession.activate, Some Ios.RecordingSession.release)
    #else
type AudioManager() = 
    interface IAudioManager with
        member this.CreatePlayer(audioFormat) = new Al.Player(audioFormat)
        member this.CreateRecorder(audioFormat) = new Al.Recorder(audioFormat,None,None)
    #endif
#endif
