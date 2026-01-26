namespace FsPlaySamples.Cua.Views

open System.Runtime.CompilerServices
open Fabulous
open Fabulous.Maui
open Microsoft.Maui.Controls

[<Extension>]
type HybridWebViewModifiers =

    [<Extension>]
    static member inline reference(this: WidgetBuilder<'msg, #IFabHybridWebView>, value: ViewRef<HybridWebView>) =
        this.AddScalar(ViewRefAttributes.ViewRef.WithValue(value.Unbox))


