namespace RTOpenAI.Sample
open Microsoft.Maui.Hosting
open Microsoft.Extensions.DependencyInjection
open Fabulous.Maui
open Microsoft.Extensions.Logging
open RTOpenAI.Sample.Navigation

type MauiProgram =

    static member CreateMauiApp() =
        let builder = MauiApp.CreateBuilder()
        let nav = NavigationController()
        let appMsgDispatcher = AppMessageDispatcher()        
        builder
            .UseFabulousApp(App.view nav appMsgDispatcher)
            .ConfigureFonts(fun fonts ->
                fonts
                    .AddFont("OpenSans-Regular.ttf", "OpenSansRegular")
                    .AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold")
                    .AddFont("MaterialSymbols.ttf", "MaterialSymbols")
                |> ignore)
                |> ignore
        builder.Services.AddLogging(fun x ->
            x.AddConsole() |> ignore
            x.AddFilter("RTOpenAILog", LogLevel.Information) |> ignore
            ) |> ignore
        builder.Build()
