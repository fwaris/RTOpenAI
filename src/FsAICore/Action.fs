namespace FsAICore

type Point = {x:int; y:int}
type Path = {
    path : Point list
}

[<RequireQualifiedAccess>]
type Button = Left | Right | Middle | Back | Forward | Wheel | Unknown

[<RequireQualifiedAccess>]
type Action =
    | Click of {| button:Button; x:int; y:int|}
    | Scroll of {|x:int; y:int; scroll_x:int; scroll_y:int|}
    | Keypress of {| keys:string list;|} //ctrl, alt, shift
    | Type of {| text:string|}
    | Wait
    | Screenshot
    | Double_click of {|x:int; y:int|}
    | Drag of Path
    | Move of {| x:int; y:int |}
    
    with member this.toString() =  
            match this with
            | Click p -> $"click({p.x},{p.y},{p.button})"
            | Scroll p -> $"scroll {p.scroll_x},{p.scroll_y}@{p.x},{p.y}"
            | Double_click p -> $"dbl_click({p.x},{p.y})"
            | Keypress p -> $"keys {p.keys}"
            | Move p -> $"move({p.x},{p.y})"
            | Screenshot -> "screenshot"
            | Type p -> $"type {p.text}"
            | Wait  -> "wait"
            | Drag p ->
                let s = p.path.Head
                let t = List.last p.path
                $"drag {s.x},{s.y} -> {t.x},{t.y}"
