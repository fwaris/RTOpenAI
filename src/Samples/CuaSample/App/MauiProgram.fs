namespace  FsPlaySamples.Cua
open Microsoft.Extensions.Configuration
open Microsoft.Maui.Hosting
open Microsoft.Extensions.DependencyInjection
open Fabulous.Maui
open Microsoft.Extensions.Logging
open  FsPlaySamples.Cua.Navigation
open Syncfusion.Maui.Toolkit.Hosting

type MauiProgram =

    static member CreateMauiApp() =
        let builder = MauiApp.CreateBuilder()
        let nav = NavigationController()
        let appMsgDispatcher = AppMessageDispatcher()        
        builder
            .ConfigureSyncfusionToolkit()
            .UseFabulousApp(App.view nav appMsgDispatcher)
            .ConfigureMauiHandlers(fun handlers ->
#if IOS || MACCATALYST
                
                handlers.AddHandler(typeof<Microsoft.Maui.Controls.WebView>, typeof<FsPlay.ios.CreateHandler>)
                |> ignore
#endif
#if ANDROID
                handlers.AddHandler(typeof<Microsoft.Maui.Controls.WebView>, typeof<FsPlay.droid.CreateHandler>)
                |> ignore
#endif 
#if WINDOWS 
                failwith "platform not supported"
#endif
             )
            .ConfigureFonts(fun fonts ->
                fonts
                    .AddFont("OpenSans-Regular.ttf", C.FONT_REG)
                    .AddFont("OpenSans-Semibold.ttf", C.FONT_BOLD)
                    .AddFont("MaterialSymbols.ttf", C.FONT_SYMBOLS)
                    .AddFont("Font Awesome 7 Free-Solid-900.otf", C.FA)
                    .AddFont("Font Awesome 7 Free-Regular-400.otf", C.FAreg)
                    .AddFont("NotoSansSymbols2-Regular.ttf", C.FAreg)
                
                |> ignore)
            .Configuration.AddEnvironmentVariables() |> ignore
        builder.Services.AddLogging(fun x ->
            x.AddConsole() |> ignore
            x.AddFilter("RTOpenAILog", LogLevel.Information) |> ignore
            ) |> ignore
//#if DEBUG        
        builder.Logging.AddConsole() |> ignore
//#endif
       
        builder.Build()
