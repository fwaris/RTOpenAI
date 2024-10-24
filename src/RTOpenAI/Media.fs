namespace RTOpenAI.FabulousExtensions

open System.Runtime.CompilerServices
open Fabulous
open Microsoft.Maui.Controls
open Microsoft.Maui.Graphics
open Fabulous.Maui
open CommunityToolkit.Maui.Views

type IFabBoxView =
    inherit IFabView

module MediaElement =
    let WidgetKey = Widgets.register<MediaElement>()

    let AspectProperty = Attributes.defineBindableWithEquality<Microsoft.Maui.Aspect> MediaElement.AspectProperty


[<AutoOpen>]
module MedaiElementBuilders =
    type Fabulous.Maui.View with

        static member inline MediaElement<'msg>(aspect:Microsoft.Maui.Aspect) =
            WidgetBuilder<'msg, IFabBoxView>(MediaElement.WidgetKey, MediaElement.AspectProperty.WithValue(aspect) )

[<Extension>]
type MediaElementModifiers =
    [<Extension>]
    static member inline aspect(this: WidgetBuilder<'msg, #IFabBoxView>, value: Microsoft.Maui.Aspect) =
        this.AddScalar(MediaElement.AspectProperty.WithValue(value))

    [<Extension>]
    static member inline reference(this: WidgetBuilder<'msg, IFabBoxView>, value: ViewRef<MediaElement>) =
        this.AddScalar(ViewRefAttributes.ViewRef.WithValue(value.Unbox))
