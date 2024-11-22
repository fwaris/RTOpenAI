namespace Microsoft.Windows.Foundation.UndockedRegFreeWinRTFS

open System.Runtime.InteropServices

module private NativeMethods =

    [<DllImport("Microsoft.WindowsAppRuntime.dll", CharSet = CharSet.Unicode, ExactSpelling = true)>]
    extern int WindowsAppRuntime_EnsureIsLoaded()

[<AbstractClass; Sealed>]
type Initializer =

    static member AccessWindowsAppSDK () =
        NativeMethods.WindowsAppRuntime_EnsureIsLoaded ()
        |> ignore // return value not required
