namespace FsPlay.Abstractions


type MouseButton =
    | Left
    | Right
    | Middle

type UITarget = BrowserTarget of string | AppTarget of string

type IUIDriver =
    interface
        abstract member doubleClick: x:int*y:int -> Async<unit>
        abstract member click : x:int*y:int*MouseButton -> Async<unit>
        abstract member typeText : string -> Async<unit>
        abstract member wheel : x:int*y:int -> Async<unit>
        abstract member move : x:int*y:int -> Async<unit>
        abstract member scroll : x:int*y:int -> scrollX:int*scrollY:int -> Async<unit>
        abstract member pressKeys : keys:string list -> Async<unit>
        abstract member dragDrop : sourceX:int*sourceY:int -> targetX:int*targetY:int -> Async<unit>
        abstract member currentDimensions : unit->Async<int*int>
        abstract member snapshot : unit -> Async<byte[]*(int*int)*string>
        abstract member goBack : unit -> Async<unit>
        abstract member goForward : unit -> Async<unit>
        abstract member goto : string -> Async<unit>
        abstract member url : unit -> Async<string option>
        abstract member environment : string //computer call environment
        abstract member start : string -> Async<unit>
        abstract member saveState: unit -> Async<unit>
        abstract member clearCookies : unit -> Async<unit>
        abstract member reload : unit -> Async<unit>
        abstract member getUrlBytes : unit -> Async<byte[]>
        abstract member evaluateJavaScript : string -> Async<string>

    end
