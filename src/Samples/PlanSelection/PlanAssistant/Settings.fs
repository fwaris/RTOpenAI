namespace RT.Assistant.Settings
open Fabulous
open Microsoft.Maui.Storage
open RT.Assistant

type Settings() =
    inherit EnvironmentObject()

    let mutable apiKey = Preferences.Default.Get(C.SETTINGS_KEY, "")

    member this.ApiKey
        with get () = apiKey
        and set (v:string) =
            apiKey <- v.Trim()
            Preferences.Default.Set(C.SETTINGS_KEY, v)
            this.NotifyChanged()
 
 [<RequireQualifiedAccess>]         
module Environment =
    let settingsKey = EnvironmentKey<Settings>(C.SETTINGS_KEY)
    let apiKey() = Preferences.Default.Get(C.SETTINGS_KEY, "").Trim()
