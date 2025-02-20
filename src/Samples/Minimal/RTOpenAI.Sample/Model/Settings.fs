namespace RTOpenAI.Sample.Settings
open RTOpenAI.Sample
open Fabulous
open type Fabulous.Maui.View
open type Fabulous.Context
open Microsoft.Maui.Storage

type Settings() =
    inherit EnvironmentObject()

    let mutable apiKey = Preferences.Default.Get(C.SETTINGS_KEY, "")

    member this.ApiKey
        with get () = apiKey
        and set v =
            apiKey <- v
            Preferences.Default.Set(C.SETTINGS_KEY, v)
            this.NotifyChanged()
 
 [<RequireQualifiedAccess>]         
module Environment =
    let settingsKey = EnvironmentKey<Settings>(C.SETTINGS_KEY)
    let apiKey() = Preferences.Default.Get(C.SETTINGS_KEY, "").Trim()
