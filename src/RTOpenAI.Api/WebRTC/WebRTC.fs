namespace RTOpenAI.WebRTC
// open Microsoft.Maui.Devices
// open Microsoft.Maui.Controls.PlatformConfiguration

module WebRtc = 

    let create() =

#if IOS || MACCATALYST 
        new  RTOpenAI.WebRTC.IOS.WebRtcClientIOS()
#else
        #if ANDROID 
                new RTOpenAI.WebRTC.Android.WebRtcClientAndroid()
        #else
                #if WINDOWS
                    new RTOpenAI.WebRTC.Win.WebRtcClientWin()
                #endif
        #endif
#endif
