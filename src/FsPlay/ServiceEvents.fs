namespace FsPlay

open System.Threading.Channels

type WvEvent =
    | WvScroll of float*float
    | WvTouch

module ServiceEvents =
    let mailbox = lazy(
        let opts = BoundedChannelOptions(30)
        opts.FullMode <- BoundedChannelFullMode.DropOldest  
        Channel.CreateBounded<WvEvent>(opts))
