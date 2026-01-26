namespace FsAICore.Anthropic
(*
Tooling need to support computer use agent with Anthropic
*)
open System
open System.Text.Json.Serialization
open Anthropic.SDK.Messaging
open FsAICore
open Microsoft.Extensions.AI
open System.Text.Json

#nowarn "3391"

type Coordinate = {x:uint; y:uint}
type ScrollDirection = Up | Down | Left | Right
///Parsed and structured computer call (obtained after parsing the raw Anthropic computer function call)
type AnthropicAction = 
    | Key of string list
    | Hold_Key of {|key:string; duration:uint|}
    | Type of string
    | MouseMove of Coordinate
    | Left_Click of Coordinate
    | Left_Click_Drag of Coordinate
    | Right_Click of Coordinate
    | Middle_Click of Coordinate
    | Double_Click of Coordinate
    | Triple_Click of Coordinate
    | Left_Mouse_Down
    | Left_Mouse_Up
    | Scroll of {|direction:ScrollDirection; amount:uint|}
    | Screenshot
    | Wait of uint
    | Unknown of string

///Maps to 'action' field in the Anthropic computer function call arguments (intermediate type to help in parsing and structuring)
[<RequireQualifiedAccess>]
[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type internal CuaAction =
    | key = 0
    | ``type`` = 1
    | mouse_move = 2
    | left_click = 3
    | left_click_drag = 4
    | right_click = 5
    | middle_click = 6
    | double_click = 7
    | screenshot = 8
    | cursor_position = 9
    | left_mouse_down = 10
    | scroll = 11
    | hold_key = 12
    | wait = 13
    | triple_click = 14

type internal CuaToolCall = 
    {
        action: CuaAction
        text : string option
        coordinate : uint list option
        scroll_direction : string option
        scroll_amount : uint option
        duration : uint option
        key : string option
    }

module Parser = 
    let coordinate2 (js:JsonElement) = 
        match js.TryGetProperty("coordinate") with 
        | true, j -> 
            let xs = j.EnumerateArray() |> Seq.toArray
            if xs.Length <> 2 then failwith "invalidate coordinate"
            let x = xs.[0].GetUInt32()
            let y = xs.[1].GetUInt32()
            {x=x; y=y}
        | _ -> 
            {x = 0u; y = 0u}        

    let direction (j:JsonElement) = 
        match j.GetString() with 
        | "up" -> Up
        | "down" -> Down
        | "left" -> Left
        | "right" -> Right
        | x -> failwith $"{x} is not expected for scroll direction"

    let duration (j:JsonElement) = 
        j.GetUInt32()

    ///Convert computer function call to structured Action
    let parseActionBase (js:JsonElement) : AnthropicAction option =
        try 
            let j = js.GetProperty("action")
            let str = System.Text.Json.JsonSerializer.Deserialize<CuaAction>(j)
            let coordinate = coordinate2 js
            match str with 
            | CuaAction.key -> Key [(js.GetProperty("text").GetString())]                
            | CuaAction.``type`` -> Type (js.GetProperty("text").GetString())
            | CuaAction.mouse_move -> MouseMove coordinate
            | CuaAction.left_click -> Left_Click coordinate
            | CuaAction.left_click_drag -> Left_Click_Drag coordinate
            | CuaAction.right_click -> Right_Click coordinate
            | CuaAction.middle_click -> Middle_Click coordinate
            | CuaAction.double_click -> Double_Click coordinate
            | CuaAction.screenshot -> Screenshot
            | CuaAction.cursor_position -> MouseMove (coordinate2 (js.GetProperty("coordinate")))
            | CuaAction.left_mouse_down -> Left_Mouse_Down
            | CuaAction.scroll -> Scroll {|amount=1u; direction=direction (js.GetProperty("scroll_direction")) |}
            | CuaAction.hold_key -> Hold_Key {|key=j.GetProperty("key").GetString(); duration=duration (js.GetProperty("duration")) |}
            | CuaAction.wait -> Wait (duration (js.GetProperty("duration")))
            | CuaAction.triple_click -> Triple_Click coordinate
            | x -> Unknown (string x) //failwith "action '{x}' is not understood for the 'computer' tool"
            |> Some
        with ex -> 
            Log.info $"cua acton not found {string js}"
            None 
    
    let parseActions js = 
        match parseActionBase js with 
        | Some (Key xs) -> [KeyUtils.canonicalize xs |> Key]
        | Some x         -> [x]
        | None           -> []

    let mapScroll amount dir =
        // Playwright scroll expects: position (x,y) and scroll delta (scrollX, scrollY)
        // Anthropic CUA provides amount as scroll "units" (typically 1-5)
        // Scale by ~100 pixels per unit for natural scrolling
        let pixelAmount = amount * 250
        // Use current mouse position (-1,-1) and translate direction to scroll delta
        match dir with 
        | Up -> Action.Scroll {|x = -1; y = -1; scroll_x = 0; scroll_y = -pixelAmount |}
        | Down -> Action.Scroll {|x = -1; y = -1; scroll_x = 0; scroll_y = pixelAmount |}
        | ScrollDirection.Left -> Action.Scroll {|x = -1; y = -1; scroll_x = -pixelAmount; scroll_y = 0 |}
        | ScrollDirection.Right -> Action.Scroll {|x = -1; y = -1; scroll_x = pixelAmount; scroll_y = 0 |}

    let point (x,y) = 
            {Point.x = x; Point.y = y}

    ///Convert Action to a representation that the IUIDriver (playwright) can understand
    let mapToUIDriverAction  = function
        | Key ks -> Action.Keypress {|keys=ks|}
        | Hold_Key parms -> Action.Keypress {|keys=[parms.key]|} //does not make sense for web pages
        | Type s -> Action.Type {|text=s|}
        | MouseMove c -> Action.Move {|x= int c.x; y= int c.y|}
        | Left_Click c -> Action.Click {|button=Button.Left; x = int c.x; y = int c.y|}
        | Left_Click_Drag c -> Action.Drag {path=[point(-1,-1); point(int c.x, int c.y)]}
        | Right_Click c -> Action.Click {|button=Button.Right; x = int c.x; y = int c.y|}
        | Middle_Click c -> Action.Click {|button=Button.Middle; x = int c.x; y = int c.y|}
        | Double_Click c -> Action.Double_click {|x = int c.x; y = int c.y|} // -1 indicates use current mouse position
        | Triple_Click c -> Action.Double_click {|x = int c.x; y = int c.y|} // -1 indicates use current mouse position
        | Left_Mouse_Down -> Log.warn "action ignored: left_mouse_down"; Action.Wait
        | Left_Mouse_Up -> Log.warn "action ignored: left_mouse_up"; Action.Wait
        | Scroll scrl -> mapScroll (int scrl.amount) scrl.direction
        | Screenshot -> Action.Screenshot
        | Wait duration -> Action.Wait
        | Unknown x -> failwith $"Unable to map to playwright action '{x}'"
        

module ToolUtils = 
    open System.Collections.Generic
    open Anthropic.SDK.Common
    
    let cuaTool width height : Tool = 
        let toolPrams = 
            [
                "display_width_px", width :> obj
                "display_height_px", height
                "display_number", 1 
            ]
            |> dict
            |> Dictionary
        Function("computer", "computer_20250124",toolPrams)      

    let toJsonElement (args:IDictionary<string,obj>) = 
        match args with 
        | null -> None
        | _ -> 
            let json = JsonSerializer.Serialize(args, Utility.openAIResponseSerOpts);
            use doc = JsonDocument.Parse(json)
            doc.RootElement.Clone()
            |> Some

    let thinking = lazy(
        let tp = ThinkingParameters()
        tp.BudgetTokens <- 2048
        tp
    )
    
    let addCuaAnthropicTool (width,height) (chatOptions:ChatOptions) =
        let metadata = Dictionary<string,obj>()
        metadata["display_width_px"] <- box width
        metadata["display_height_px"] <- box height
        metadata["display_number"] <- box 1

        let computerFunction = Function("computer", "computer_20250124", metadata)
        let computerTool = Anthropic.SDK.Common.Tool(computerFunction)

        let parameters = MessageParameters()
        parameters.MaxTokens <- 4096
        parameters.Thinking <-  thinking.Value
        let tools = ResizeArray<Anthropic.SDK.Common.Tool>()
        tools.Add(computerTool)
        parameters.Tools <- tools :> IList<_>        
        chatOptions.RawRepresentationFactory <- Func<IChatClient,obj>(fun _ -> parameters)
            
                
