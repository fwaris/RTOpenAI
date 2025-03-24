namespace RTOpenAI.Sample

open System

module Program =
    [<EntryPoint; STAThread>]
    let main args =
        Microsoft.Windows.Foundation.UndockedRegFreeWinRTFS.Initializer.AccessWindowsAppSDK()
        do FSharp.Maui.WinUICompat.Program.Main(args, typeof<RTOpenAI.Sample.WinUI.App>)
        0
