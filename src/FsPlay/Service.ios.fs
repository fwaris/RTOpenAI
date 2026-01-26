namespace FsPlay.ios
#if IOS || MACCATALYST

open System
open System.Diagnostics
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open System.Collections.Concurrent
open System.Collections.Generic
open Foundation
open FsPlay
open Microsoft.Maui.Handlers
open Microsoft.Maui.Platform
open UIKit
open CoreGraphics
open WebKit
open Microsoft.Maui.Controls
open Microsoft.Maui.ApplicationModel
open Microsoft.FSharp.Control


module internal IosScripts =
    let js = """
document.addEventListener('touchend', function() {
    window.webkit.messageHandlers.touchHandler.postMessage("touchend");
});

document.addEventListener('input', function(e) {
    window.webkit.messageHandlers.touchHandler.postMessage(
        JSON.stringify({ type: "input", value: e.target.value })
    );
});
"""
    
    let userScript = new WKUserScript(
            new NSString(js),
            WKUserScriptInjectionTime.AtDocumentEnd,
            true // forMainFrameOnly
    )
    
    let bootstrapScript = new WKUserScript(
            new NSString(Bootstrap.bootstrapScript),
            WKUserScriptInjectionTime.AtDocumentStart,
            false,
            WKContentWorld.Page // forMainFrameOnly
    )

type EventPoster() =
    inherit NSObject()
    interface IWKScriptMessageHandler with 
        member this.DidReceiveScriptMessage (userContentController: WKUserContentController, message: WKScriptMessage): unit =
            ServiceEvents.mailbox.Value.Writer.TryWrite(WvTouch) |> ignore
            System.Diagnostics.Debug.WriteLine($"js handler called {message.Name}")
            
type CreateHandler() =
    inherit WebViewHandler()
    
    do
        System.Diagnostics.Debug.WriteLine("handler called")
            
    override this.CreatePlatformView (): WKWebView =
        let cfg = MauiWKWebView.CreateConfiguration()
        cfg.UserContentController.AddUserScript(IosScripts.userScript)
        cfg.UserContentController.AddUserScript(IosScripts.bootstrapScript)
        cfg.UserContentController.AddScriptMessageHandler(new EventPoster(),"touchHandler") 
        let wv = new MauiWKWebView(CGRect.Empty,this,cfg)
#if !IOS
        wv.CustomUserAgent <- "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) " +
                               "AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 " +
                               "Mobile/15E148 Safari/604.1";
#endif
        wv.Inspectable <- true
        wv.ScrollView.Scrolled.Add(fun (e:EventArgs) ->
            let offset = wv.ScrollView.ContentOffset
            let x = offset.X.Value
            let y = offset.Y.Value
            ServiceEvents.mailbox.Value.Writer.TryWrite(WvScroll(x,y))|>ignore
            System.Diagnostics.Debug.WriteLine($"ios scroll({x},{y})") |> ignore)       
        wv
        
    override this.ConnectHandler (platformView: WKWebView): unit =
        base.ConnectHandler(platformView)
        
       
module internal Service =
    
    let private capture2 (wv:WKWebView) : Task<byte[]*(int*int)*string> = task {
        try
            UIGraphics.BeginImageContextWithOptions(wv.Bounds.Size, false, 1.0f);
            let r = wv.DrawViewHierarchy(wv.Bounds, afterScreenUpdates = true)
            let image = UIGraphics.GetImageFromCurrentImageContext()
            UIGraphics.EndImageContext()
            let data = image.AsJPEG()
            return data.ToArray(),(int image.Size.Width, int image.Size.Height),"image/jpeg"
        with ex ->
            System.Diagnostics.Debug.WriteLine(ex.Message)
            return raise ex            
    }
    
    let private dimensions (wv:WKWebView) : Task<int*int> = task {
        let sz = wv.Bounds.Size
        return (sz.Width.Value |> int, sz.Height.Value |> int)
    }
        
    //alt method
    let private capture_ (wv:WKWebView) = 
        task {
            try
                let sz = wv.Bounds
                let rect = CGRect(0.,0.,sz.Width,sz.Height)
                use cfg = new WKSnapshotConfiguration(Rect = rect)
                use! sn  = wv.TakeSnapshotAsync(cfg)
                let data = sn.AsJPEG()
                return data.ToArray()
            with ex ->
                System.Diagnostics.Debug.WriteLine(ex.Message)
                return raise ex
        }
    
    let internal currentDimensions(wv:WebView) : Task<int*int> =task {
          match wv.Handler with
            | null -> return failwith "not ready"
            | h -> 
                match h.PlatformView with 
                | :? WKWebView as wv ->
                    do! MainThread.InvokeOnMainThreadAsync(wv.LayoutIfNeeded)
                    let f() = dimensions(wv)
                    let! data = MainThread.InvokeOnMainThreadAsync<int*int>(f)
                    return data
                | _ -> return failwith "webview not ready"                
        }
        
    let internal capture(wv:WebView) : Task<byte[]*(int*int)*string> = task {    
            match wv.Handler with
            | null -> return failwith "not ready"
            | h -> 
                match h.PlatformView with 
                | :? WKWebView as wv ->
                    do! MainThread.InvokeOnMainThreadAsync(wv.LayoutIfNeeded)
                    let f() = capture2 wv
                    let! data = MainThread.InvokeOnMainThreadAsync<byte[]*(int*int)*string>(f)
                    return data
                | _ -> return failwith "webview not ready"
        }
    
    let internal evalJs(wv:WebView) (js:string) : Task<string> = task {
            match wv.Handler with
            | null -> return failwith "not ready"
            | h ->
                match h.PlatformView with
                | :? WKWebView as wv ->
                    do! MainThread.InvokeOnMainThreadAsync(wv.LayoutIfNeeded)
                    let f() = wv.EvaluateJavaScriptAsync(js,null, WKContentWorld.Page)
                    let! data = MainThread.InvokeOnMainThreadAsync<NSObject>(f)
                    let data = data.ToString()
                    return data
                | _ -> return failwith "webview not ready"
        }

#endif

