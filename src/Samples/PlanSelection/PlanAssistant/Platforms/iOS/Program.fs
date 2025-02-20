namespace  RT.Assistant

open UIKit

module Program =
    [<EntryPoint>]
    let main args =
        UIApplication.Main(args, null, typeof<AppDelegate>)
        0
