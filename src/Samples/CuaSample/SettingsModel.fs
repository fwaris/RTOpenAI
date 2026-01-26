namespace FsPlaySamples.Cua.Settings
open Fabulous
open Microsoft.Maui.Storage
open FsPlaySamples.Cua

///model to get/set settings via UI
type SettingsModel() =
    inherit EnvironmentObject()
    member this.ApiKey
        with get () = Preferences.Default.Get(C.API_KEY, "")
        and set (v:string) =
            let v = v.Trim()
            Preferences.Default.Set(C.API_KEY, v)
            this.NotifyChanged()
    member this.PreviewClicks
        with get() = Preferences.Default.Get<bool>(C.PREVIEW_CLICKS, true)
        and set(v:bool) =            
            Preferences.Default.Set(C.PREVIEW_CLICKS, v)
            this.NotifyChanged()
                        
 ///Environment module to access settings
 [<RequireQualifiedAccess>]         
module Environment =
    let settingsKey = EnvironmentKey<SettingsModel>(C.SETTINGS_KEY)
    let apiKey() = Preferences.Default.Get(C.API_KEY, "").Trim()
    let previewClicks() = Preferences.Default.Get<bool>(C.PREVIEW_CLICKS, false)
    
