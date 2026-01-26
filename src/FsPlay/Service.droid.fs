namespace FsPlay.droid
#if ANDROID

open System.Threading.Tasks
open Android.Graphics
open System.IO
open Android.Util
open Microsoft.Maui
open Microsoft.Maui.ApplicationModel
open Microsoft.Maui.Controls
open Microsoft.Maui.Handlers
open Microsoft.Maui.Platform
open FsPlay

type AWebView = Android.Webkit.WebView
type WsListener() =
    inherit Java.Lang.Object()
    interface Android.Views.View.IOnTouchListener with
        member this.OnTouch(v,e) =
            ServiceEvents.mailbox.Value.Writer.TryWrite(WvTouch) |> ignore
            System.Diagnostics.Debug.WriteLine("android touch")
            false

type BootstrapWebViewClient(handler: WebViewHandler, js1: string) =
    inherit MauiWebViewClient(handler)

    override _.OnPageFinished(view: AWebView, url: string) =
        base.OnPageFinished(view, url)
        try
            Log.Info("WebViewDriver","OnPageFinished") |> ignore
            view.EvaluateJavascript(js1, null)
        with ex ->
            Log.Error("WebViewDriver", ex.Message) |> ignore
        
type CreateHandler() =
    inherit WebViewHandler()

    static do
        WebViewHandler.Mapper.ModifyMapping<IWebView, IWebViewHandler>(
            "WebViewClient",
            fun handler view previous ->
                match handler with
                | :? CreateHandler as customHandler ->
                    let client = new BootstrapWebViewClient(customHandler, Bootstrap.bootstrapScript)
                    handler.PlatformView.SetWebViewClient(client)
                | _ when not (obj.ReferenceEquals(previous, null)) ->
                    previous.Invoke(handler, view)
                | _ -> ())

    do
        System.Diagnostics.Debug.WriteLine("android handler called")
        
    override this.ConnectHandler (platformView: AWebView) =
        base.ConnectHandler(platformView)
        AWebView.SetWebContentsDebuggingEnabled(true)
        
    override this.CreatePlatformView (): AWebView =
        let wv = base.CreatePlatformView()
        wv.ScrollChange.Add(fun v ->
            ServiceEvents.mailbox.Value.Writer.TryWrite(WvScroll(v.ScrollX,v.ScrollY)) |> ignore
            System.Diagnostics.Debug.WriteLine($"droid scroll ({v.ScrollX},{v.ScrollY})"))
        wv.SetOnTouchListener(new WsListener())
        wv
    
    
        
module internal Service = 
    let private dimensions(wv:AWebView)  : Task<(int*int)*(int*int)> = 
        task {
            try
                let metrics = wv.Resources.DisplayMetrics
                let density = metrics.Density
                let scale = 1.0f                
                let bw = int (float32 wv.Width * scale)
                let bh = int (float32 wv.Height * scale)
                if bw <= 0 || bh <= 0 then
                    invalidOp "webview size not ready"
                let rescale = scale / density
                let sw = max 1 (int (float32 bw * rescale))
                let sh = max 1 (int (float32 bh * rescale))
                let scaleX = float32 sw / float32 bw
                let scaleY = float32 sh / float32 bh
                return (sw,sh),(bw,bh)
            with ex ->
                System.Diagnostics.Debug.WriteLine(ex.Message)
                return raise ex
        }
    
    let private capture_(wv:AWebView)  : Task<byte[]*(int*int)*string> =
        task {
            try
                let! (sw,sh),(bw,bh) = dimensions(wv)
                let scaleX = float32 sw / float32 bw
                let scaleY = float32 sh / float32 bh
                use bmp = Bitmap.CreateBitmap(sw, sh, Bitmap.Config.Argb8888)
                use canvas = new Canvas(bmp)
                // Scale scroll offset to match the bitmap coordinate space
                // Without this, the captured region is wrong when page is scrolled
                canvas.Translate(-float32 wv.ScrollX * scaleX, -float32 wv.ScrollY * scaleY)
                canvas.Scale(scaleX, scaleY)
                wv.Draw(canvas)
                let ms = new MemoryStream()
                let! _ = bmp.CompressAsync(Bitmap.CompressFormat.Jpeg, 60, ms)
                ms.Position <- 0L
                bmp.Recycle()
                return ms.ToArray(),(sw,sh),"image/jpeg"
            with ex ->
                System.Diagnostics.Debug.WriteLine(ex.Message)
                return raise ex
        }
        
    
    let internal currentDimensions(wv:WebView) : Task<int*int> =
        task {
            match wv.Handler with
            | null -> return failwith "not ready"
            | h -> 
                match h.PlatformView with 
                | :? AWebView as wv ->
                    let f() = dimensions wv
                    let! (w,h),_ = MainThread.InvokeOnMainThreadAsync<(int*int)*(int*int)>(f)
                    return (w,h)
                | _ -> return failwith "webview not ready"
        }                   

    let internal capture(wv:WebView) : Task<byte[]*(int*int)*string> = 
        task {
            match wv.Handler with
            | null -> return failwith "not ready"
            | h -> 
                match h.PlatformView with 
                | :? AWebView as wv ->
                    let f() = capture_ wv
                    let! data = MainThread.InvokeOnMainThreadAsync<byte[]*(int*int)*string>(f)
                    return data
                | _ -> return failwith "webview not ready"
        }
        
    let internal evalJs(wv:WebView) (js:string) : Task<string> = task {
            let f() = wv.EvaluateJavaScriptAsync(js)
            return! MainThread.InvokeOnMainThreadAsync<string>(f)
        }

#endif
