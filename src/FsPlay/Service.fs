namespace FsPlay

open Microsoft.Maui.Controls
open FsPlay.MauiWebViewDriver

module Service =
    let capture (wv:WebView) =           
#if IOS || MACCATALYST
         FsPlay.ios.Service.capture wv
#else
    #if ANDROID
        FsPlay.droid.Service.capture wv
    #else 
        task {failwith "unsupported platform"}
    #endif
#endif

    let dimensions (wv:WebView) =
#if IOS || MACCATALYST
        FsPlay.ios.Service.currentDimensions wv
#else
    #if ANDROID
        FsPlay.droid.Service.currentDimensions wv
    #else
        task {failwith "unsupported platform"}
    #endif
#endif

    let evalJs (wv:WebView) (js:string) = 
#if IOS || MACCATALYST
        FsPlay.ios.Service.evalJs wv js
#else
    #if ANDROID
        FsPlay.droid.Service.evalJs wv js
    #else 
        task {failwith "unsupported platform"}
    #endif
#endif

    let createWebViewWrapper(wv:WebView) =
            {new WebViewWrapper with
                member this.CaptureAsync() = capture(wv)
                member this.EvaluateJavaScriptAsync(js) =
#if IOS || MACCATALYST
                    FsPlay.ios.Service.evalJs wv js
#else
                    wv.EvaluateJavaScriptAsync(js)
#endif
                member this.GoBack() = wv.GoBack()
                member this.GoForward() = wv.GoForward()
                member this.CurrentDimensions() = dimensions(wv)
                member this.Reload() = wv.Reload()
                member this.CanGoBack = wv.CanGoBack
                member this.Source
                    with get() =
                        match wv.Source with
                        | :? UrlWebViewSource as s -> s.Url
                        | _ -> ""
                    and set(value) = wv.Source <- UrlWebViewSource(Url=value)
                member this.CanGoForward = wv.CanGoForward
            }
    