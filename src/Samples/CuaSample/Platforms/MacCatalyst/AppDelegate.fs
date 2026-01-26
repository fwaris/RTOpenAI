namespace FsPlaySamples.Cua

open Foundation
open Microsoft.Maui

[<Register("AppDelegate")>]
type AppDelegate() =
    inherit MauiUIApplicationDelegate()

    override this.CreateMauiApp() = 
        //System.Diagnostics.Debugger.Break()
        MauiProgram.CreateMauiApp()
