namespace  RT.Assistant
open Microsoft.Maui.Hosting
open Microsoft.Extensions.DependencyInjection
open Fabulous.Maui
open Microsoft.Extensions.Logging
open  RT.Assistant.Navigation

type MauiProgram =

    static member CreateMauiApp() =
        let builder = MauiApp.CreateBuilder()
        let nav = NavigationController()
        let appMsgDispatcher = AppMessageDispatcher()        
        builder
            .UseFabulousApp(App.view nav appMsgDispatcher)
            .ConfigureFonts(fun fonts ->
                fonts
                    .AddFont("OpenSans-Regular.ttf", C.FONT_REG)
                    .AddFont("OpenSans-Semibold.ttf", C.FONT_BOLD)
                    .AddFont("MaterialSymbols.ttf", C.FONT_SYMBOLS)
                |> ignore)
                |> ignore
        builder.Services.AddLogging(fun x ->
            x.AddConsole() |> ignore
            x.AddFilter("RTOpenAILog", LogLevel.Information) |> ignore
            ) |> ignore
//#if DEBUG
        builder.Services.AddHybridWebViewDeveloperTools() |> ignore
        builder.Logging.AddConsole() |> ignore
//#endif
       
        builder.Build()
