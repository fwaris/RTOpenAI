namespace RTOpenAI.WinUI

open System

module Program =
    [<EntryPoint; STAThread>]
    let main args =
        Microsoft.Windows.Foundation.UndockedRegFreeWinRTFS.Initializer.AccessWindowsAppSDK()
        do FSharp.Maui.WinUICompat.Program.Main(args, typeof<RTOpenAI.WinUI.App>)
        0
