namespace FsAICore

module K = 
    let [<Literal>] Enter = "Enter"
    let [<Literal>] Backspace = "Backspace"
    let [<Literal>] Escape = "Escape"
    let [<Literal>] Shift = "Shift"
    let [<Literal>] Control = "Control"
    let [<Literal>] Tab = "Tab"
    let [<Literal>] ArrowLeft = "ArrowLeft"
    let [<Literal>] ArrowRight = "ArrowRight"
    let [<Literal>] ArrowUp = "ArrowUp"
    let [<Literal>] ArrowDown = "ArrowDown"
    let [<Literal>] Alt = "Alt"
    let [<Literal>] AltGraph = "AltGraph"
    let [<Literal>] Meta = "Meta"
    let [<Literal>] PageUp = "PageUp"
    let [<Literal>] PageDown = "PageDown"
    let [<Literal>] Home = "Home"
    let [<Literal>] End = "End"
    let [<Literal>] Insert = "Insert"
    let [<Literal>] Delete = "Delete"

module KeyUtils = 
    open System
    
    let split (x:string) = x.Split("+") |> Array.toList
    
    let private (=*=) (a:string) (b:string) = a.Equals(b, StringComparison.OrdinalIgnoreCase)
    
    let canonicalize (keys:string list) =
        keys
        |> List.collect split
        |> List.map (fun k ->
            if k =*= "Enter" then K.Enter
            elif k =*= "Return" then K.Enter
            elif k =*= "space" then " "
            elif k =*= "backspace" then K.Backspace
            elif k =*= "ESC" then K.Escape
            elif k =*= "SHIFT" then K.Shift
            elif k =*= "CTRL" then K.Control
            elif k =*= "TAB" then K.Tab
            elif k =*= "ArrowLeft" then K.ArrowLeft
            elif k =*= "ArrowRight" then K.ArrowRight
            elif k =*= "ArrowUp" then K.ArrowUp
            elif k =*= "ArrowDown" then K.ArrowDown
            elif k =*= "Left" then K.ArrowLeft
            elif k =*= "Right" then K.ArrowRight
            elif k =*= "Up" then K.ArrowUp
            elif k =*= "Down" then K.ArrowDown
            elif k =*= "ALT" then K.Alt
            elif k =*= "ALTGR" then K.AltGraph
            elif k =*= "META" then K.Meta
            elif k =*= "PAGEUP" then K.PageUp
            elif k =*= "PAGE_UP" then K.PageUp
            elif k =*= "PAGEDOWN" then K.PageDown
            elif k =*= "PAGE_DOWN" then K.PageDown
            elif k =*= "HOME" then K.Home 
            elif k =*= "END" then K.End
            elif k =*= "INSERT" then K.Insert
            elif k =*= "DELETE" then K.Delete
            else k)
