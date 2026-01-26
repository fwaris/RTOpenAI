namespace FsPlay

open FsAICore
open FsPlay.Abstractions

module Actions =

    let mouseButton= function
        | "left"  | "Left"      -> Button.Left
        | "right" | "Right"     -> Button.Right
        | "middle" | "Middle"   -> Button.Middle
        | "back" | "Back"       -> Button.Back
        | "forward" | "Forward" -> Button.Forward
        | "wheel"   | "Wheel"   -> Button.Wheel
        | x -> Log.info $"cannot use '{x}' button"; Button.Unknown

    
    let perform (driver:IUIDriver) (action:Action)  =
        async {
            match action with
            | Action.Click p ->
                match p.button with
                | Button.Left -> do! driver.click(p.x, int p.y,Abstractions.MouseButton.Left)
                | Button.Back -> do! driver.goBack()
                | Button.Forward -> do! driver.goForward()
                | Button.Wheel  -> do! driver.wheel(p.x,p.y)
                | Button.Unknown -> do! Async.Sleep(500) //model is trying to use a button that is not supported
                | x -> Log.info $"Did not use {x} button (as it may cause issues on web pages)"
            | Action.Scroll p ->
                do! driver.scroll (p.x,p.y) (p.scroll_x,p.scroll_y)
            | Action.Keypress p -> do! driver.pressKeys p.keys
            | Action.Type p -> do! driver.typeText p.text
            | Action.Wait  ->  do! Async.Sleep(2000)
            | Action.Screenshot -> () //these are handled separately
            | Action.Move p -> do! driver.move(p.x,p.y)
            | Action.Double_click p -> do! driver.doubleClick(p.x,p.y)
            | Action.Drag p ->
                let s = p.path.Head
                let t = List.last p.path
                do! driver.dragDrop (s.x,s.y) (t.x,t.y)
                Log.info $"Drag and drop from {s} to{t}"
        }


