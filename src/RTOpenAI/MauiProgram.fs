namespace RTOpenAI
open System
open Microsoft.Maui.Hosting
open Microsoft.Extensions.DependencyInjection
open Fabulous.Maui
open Microsoft.Extensions.Logging

//open Fabulous.Maui.MediaElement

type MauiProgram =
    static member CreateMauiApp() =
        let builder = MauiApp.CreateBuilder()
        builder
            .UseFabulousApp(App.program)
            //.UseFabulousMediaElement()
            .ConfigureFonts(fun fonts ->
                fonts
                    .AddFont("OpenSans-Regular.ttf", "OpenSansRegular")
                    .AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold")
                    .AddFont("MaterialSymbols.ttf", "MaterialSymbols")
                |> ignore)
                |> ignore
        builder.Services.AddLogging(fun x -> x.AddConsole() |> ignore) |> ignore
        builder.Build()
