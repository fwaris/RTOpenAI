namespace FsPlay

open System
open Microsoft.Maui.Controls
open System.Threading.Tasks
open System.Threading.Channels

type WvEvent =
    | WvScroll of float*float
    | WvTouch

module ServiceEvents =
    let mailbox = lazy(
        let opts = BoundedChannelOptions(30)
        opts.FullMode <- BoundedChannelFullMode.DropOldest
        Channel.CreateBounded<WvEvent>(opts)
    )

type IWebViewService =
    abstract Capture : mauiWebView:WebView -> Task<byte[]>
    abstract Click : mauiWebView:WebView * x:int * y:int -> Task<unit>

module Service =
    let capture (wv:WebView) : Task<byte[]> =
        Task.FromResult(Array.empty<byte>)

    let click (wv:WebView, x:int, y:int) : Task<unit> =
        Task.FromResult(() : unit)
