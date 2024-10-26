namespace RTOpenAI
open Microsoft.Maui.Hosting
open Microsoft.Extensions.DependencyInjection
open Fabulous.Maui
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
                    .AddFont("MaterialIconsTwoTone-Regular.otf", "MaterialIconsTwoTone")
                |> ignore)
                |> ignore
        builder.Services.AddSingleton<Plugin.Maui.Audio.IAudioManager>(Plugin.Maui.Audio.AudioManager.Current) |> ignore
        builder.Build()
