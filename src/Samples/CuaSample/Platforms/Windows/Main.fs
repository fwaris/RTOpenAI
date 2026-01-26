namespace FsPlaySamples.Cua.WinUI

open System

module Program =
    [<EntryPoint; STAThread>]
    let main args =
        //Microsoft.Windows.Foundation.UndockedRegFreeWinRTFS.Initializer.AccessWindowsAppSDK()
        do FSharp.Maui.WinUICompat.Program.Main(args, typeof<RT.Assistant.WinUI.App>)
        0
