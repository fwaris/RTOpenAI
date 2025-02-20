namespace Microsoft.Windows.Foundation.UndockedRegFreeWinRTFS

open System.Runtime.InteropServices

module private NativeMethods =

    [<DllImport("Microsoft.WindowsAppRuntime.dll", CharSet = CharSet.Unicode, ExactSpelling = true)>]
    extern int WindowsAppRuntime_EnsureIsLoaded()

[<AbstractClass; Sealed>]
type Initializer =

    static member AccessWindowsAppSDK () =
        let ret = NativeMethods.WindowsAppRuntime_EnsureIsLoaded ()
        if ret <> 0 then
            printfn "WindowsAppRuntime_EnsureIsLoaded failed with error code %d" ret
        ()
