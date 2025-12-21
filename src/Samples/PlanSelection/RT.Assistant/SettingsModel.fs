namespace rec RT.Assistant.Settings
open Fabulous
open Microsoft.Maui.Storage
open RT.Assistant

type SettingsModel() =
    inherit EnvironmentObject()
    
    member this.OpenAIKey
        with get () = Values.openaiKey()
        and set (v:string) =
            Preferences.Default.Set(C.OPEN_API_KEY, v.Trim())
            this.NotifyChanged()
            
    member this.UseSonnet
        with get () = Values.useSonnet45()
        and set (v:bool) =
            Preferences.Default.Set(C.USE_SONNET, v)
            this.NotifyChanged()            
            
    member this.AnthropicKey
        with get () = Values.anthropicKey()
        and set (v:string) =
            Preferences.Default.Set(C.ANTHROPIC_API_KEY, v.Trim())
            this.NotifyChanged()
            
 [<RequireQualifiedAccess>]         
module Values =
    let settingsKey = EnvironmentKey<SettingsModel>(C.SETTINGS_KEY)
    let openaiKey () = Preferences.Default.Get(C.OPEN_API_KEY, "").Trim()
    let anthropicKey () = Preferences.Default.Get(C.ANTHROPIC_API_KEY, "").Trim()
    let useSonnet45 () = Preferences.Default.Get(C.USE_SONNET, false)
