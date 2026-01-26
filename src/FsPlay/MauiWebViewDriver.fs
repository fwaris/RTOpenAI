namespace FsPlay

open System
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Text.Encodings.Web
open System.Text.Json
open System.Threading.Tasks
open FsAICore
open Microsoft.Maui.ApplicationModel
open FsPlay.Abstractions
open Microsoft.Maui.Devices

module MauiWebViewDriver =
    let flSerOpts = lazy(
      let opts = JsonSerializerOptions(JsonSerializerDefaults.Web, WriteIndented=true, AllowTrailingCommas=true)        
      opts.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
      opts)            
   
    let private escapeScript_ (rawScript:string) =
    
      rawScript
        //.Replace("\\", "\\\\")     // Escape backslashes first
        //.Replace("\"", "\\\"")     // Escape double quotes
        .Replace("\r", " ")         // Remove carriage returns
        .Replace("\n", " ");     // Escape newlines
        
    let private escapeSomeChars rawScript =
      if DeviceInfo.Current.Platform = DevicePlatform.Android then
        rawScript
      else
        escapeScript_ rawScript
      
    type WebViewWrapper =
        abstract member EvaluateJavaScriptAsync : string -> Task<string>
        abstract member Source : string with get, set
        abstract member GoBack : unit -> unit
        abstract member GoForward : unit -> unit
        abstract member Reload : unit -> unit
        abstract member CaptureAsync : unit -> Task<byte[]*(int*int)*string>
        abstract member CurrentDimensions : unit -> Task<int*int>
        abstract member CanGoBack : bool
        abstract member CanGoForward : bool

    type DriverInstance = { driver : IUIDriver }

    type private DriverContext =
        { wrapper : WebViewWrapper
          mutable lastMouse : int * int
          mutable bootstrapped : bool
          mutable lastUrl : string option }

    let mutable private context : DriverContext option = None

    let private httpClient = lazy (new HttpClient())

    let private _ensureBootstrap (ctx: DriverContext) =
        async {
            if not ctx.bootstrapped then
                try
                    let! _ =
                        if MainThread.IsMainThread then
                            ctx.wrapper.EvaluateJavaScriptAsync(escapeSomeChars Bootstrap.bootstrapScript) |> Async.AwaitTask
                        else
                            MainThread.InvokeOnMainThreadAsync<string>(fun () -> ctx.wrapper.EvaluateJavaScriptAsync(escapeSomeChars Bootstrap.bootstrapScript))
                            |> Async.AwaitTask
                    ctx.bootstrapped <- true
                with ex ->
                    ctx.bootstrapped <- false
                    Debug.WriteLine($"[MauiWebViewDriver] bootstrap failed: {ex.Message}")
                    return raise ex
        }

    let private ensureBootstrap (ctx: DriverContext) =
        async {
            if not ctx.bootstrapped then
                ctx.bootstrapped <- true
                //do! _ensureBootstrap ctx
        }

    let private runScript (ctx: DriverContext) (script: string) =
        async {
            do! ensureBootstrap ctx
            try
              let! raw = MainThread.InvokeOnMainThreadAsync<string>(fun () -> ctx.wrapper.EvaluateJavaScriptAsync(escapeSomeChars script))
                         |> Async.AwaitTask
              //debug $"JS:\n{script}\nResp:\n{raw}"
              //let str = "\"" + raw + "\""
              //let js = JsonSerializer.Deserialize<JsonElement>(str,flSerOpts.Value)
              return raw
            with ex ->
                ctx.bootstrapped <- false
                Debug.WriteLine($"[MauiWebViewDriver] script failed: {ex.Message}")
                return raise ex
        }

    let private jsString (value: string) =
        if isNull value then
            "null"
        else
            JsonSerializer.Serialize(value)

    let private boolLiteral value = if value then "true" else "false"

    type private MetaSnapshot =
        { width: int
          height: int
          scrollX: int
          scrollY: int
          url: string option
          title: string option }

    let private metaScript = """
(function () {
  const meta = window.__fsDriver ? window.__fsDriver.meta() : {
    scrollX: window.scrollX,
    scrollY: window.scrollY,
    width: window.innerWidth,
    height: window.innerHeight,
    url: window.location.href || null,
    title: document.title || null
  };
  return JSON.stringify(meta);
})();
"""

    let private parseMeta (raw: string) : MetaSnapshot option =
      try
        use doc = JsonDocument.Parse(raw)
        let root = doc.RootElement

        let tryNumber (name: string) =
          let mutable prop = Unchecked.defaultof<JsonElement>
          if root.TryGetProperty(name, &prop) && prop.ValueKind = JsonValueKind.Number then
            match prop.TryGetDouble() with
            | true, value -> Some(int (Math.Round value))
            | _ -> None
          else
            None

        let tryString (name: string) =
          let mutable prop = Unchecked.defaultof<JsonElement>
          if root.TryGetProperty(name, &prop) then
            match prop.ValueKind with
            | JsonValueKind.Null -> None
            | JsonValueKind.String -> Some(prop.GetString())
            | _ -> Some(prop.ToString())
          else
            None

        Some
          { width = tryNumber "width" |> Option.defaultValue 0
            height = tryNumber "height" |> Option.defaultValue 0
            scrollX = tryNumber "scrollX" |> Option.defaultValue 0
            scrollY = tryNumber "scrollY" |> Option.defaultValue 0
            url = tryString "url"
            title = tryString "title" }
      with ex ->
        Debug.WriteLine($"[MauiWebViewDriver] parseMeta failed: {ex.Message}")
        None

    let private getMeta ctx =
        async {
            try
              let! raw = runScript ctx metaScript
              if String.IsNullOrWhiteSpace raw then
                return None
              else
                let meta = parseMeta raw
                meta |> Option.iter (fun m -> ctx.lastUrl <- m.url)
                return meta
            with ex ->
              Debug.WriteLine($"[MauiWebViewDriver] meta failed: {ex.Message}")
              return None
        }

    let initialize (wrapper: WebViewWrapper) =
        context <-
            Some
                { wrapper = wrapper
                  lastMouse = (0, 0)
                  bootstrapped = false
                  lastUrl = None }

    let private currentContext () =
        match context with
        | Some ctx -> ctx
        | None -> invalidOp "MauiWebViewDriver has not been initialized. Call initialize with a WebView wrapper first."

    let private resetBootstrap ctx = ctx.bootstrapped <- false

    let private runOnMainThreadUnit (work: unit -> unit) =
        if MainThread.IsMainThread then
            work()
            Task.CompletedTask
        else
          MainThread.InvokeOnMainThreadAsync(Action(fun () -> work()))

    let private capture (ctx: DriverContext) =
        if MainThread.IsMainThread then
            ctx.wrapper.CaptureAsync()
        else
            MainThread.InvokeOnMainThreadAsync<byte[]*(int*int)*string>(fun () -> ctx.wrapper.CaptureAsync())
            

    let private dimensions (ctx: DriverContext) =
        if MainThread.IsMainThread then
            ctx.wrapper.CurrentDimensions()
        else
            MainThread.InvokeOnMainThreadAsync<int*int>(fun () -> ctx.wrapper.CurrentDimensions())
                        

    let create () =
      let ctx = currentContext ()

      let driver =
        { new IUIDriver with
          member _.doubleClick (x, y) =
            async {
              ctx.lastMouse <- (x, y)
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.doubleClick({x}, {y})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }

          member _.click (x, y, button) =
            async {
              ctx.lastMouse <- (x, y)
              let buttonCode =
                match button with
                | MouseButton.Left -> 0
                | MouseButton.Middle -> 1
                | MouseButton.Right -> 2
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.clickAt({x}, {y}, {buttonCode})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }

          member _.typeText text =
            async {
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.typeText({jsString text})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }

          member _.wheel (deltaX, deltaY) =
            async {
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.scrollBy({deltaX}, {deltaY})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }

          member _.move (x, y) =
            async {
              ctx.lastMouse <- (x, y)
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.moveTo({x}, {y})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }
(*
          member _._scroll (x, y) (scrollX, scrollY) =
            async {
              ctx.lastMouse <- (x, y)
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.scrollBy({scrollX}, {scrollY})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }
*)
            
          member _.scroll (x, y) (scrollX, scrollY) =
            async {
              let x = if x < 0 then fst ctx.lastMouse else x
              let y = if y < 0 then snd ctx.lastMouse else y
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.touchScroll({x}, {y}, {scrollX}, {scrollY})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }
          member _.pressKeys keys =
            async {
              match keys with
              | [] -> return ()
              | _ ->
                let key = List.last keys
                let modifiers = keys |> List.take (List.length keys - 1)
                let ctrl = List.exists ((=) K.Control) modifiers
                let shift = List.exists ((=) K.Shift) modifiers
                let alt = List.exists ((=) K.Alt) modifiers
                let meta = List.exists ((=) K.Meta) modifiers
                let mods =
                  $"{{ ctrlKey: %s{boolLiteral ctrl}, shiftKey: %s{boolLiteral shift}, altKey: %s{boolLiteral alt}, metaKey: %s{boolLiteral meta} }}"

                let script = $"""
  (function () {{
    const driver = window.__fsDriver;
    if (!driver) return JSON.stringify({{ ok: false }});
    const keyValue = {jsString key};
    const result = driver.pressKey(keyValue, {mods});
    const normalized = (keyValue || '').toLowerCase();
    if (!result && {boolLiteral ctrl} && (normalized === 'a' || normalized === 'keya')) {{
    if (document.execCommand) {{
      document.execCommand('selectAll');
      return JSON.stringify({{ ok: true }});
    }}
    }}
    return JSON.stringify({{ ok: !!result }});
  }})();
  """
                let! _ = runScript ctx script
                return ()
            }

          member _.dragDrop (sourceX, sourceY) (targetX, targetY) =
            async {
              ctx.lastMouse <- (targetX, targetY)
              let script = $"""
  (function () {{
    return JSON.stringify({{ ok: !!(window.__fsDriver && window.__fsDriver.dragDrop({sourceX}, {sourceY}, {targetX}, {targetY})) }});
  }})();
  """
              let! _ = runScript ctx script
              return ()
            }
            
          member _.currentDimensions() = async {
            let! w,h = dimensions ctx |> Async.AwaitTask
            return (w,h)
          }
          
          member _.snapshot () = async {
            let! (img,_,_) as data = capture ctx |> Async.AwaitTask
            
            let fld =
              if DeviceInfo.Platform =  DevicePlatform.MacCatalyst then 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
              else
                Microsoft.Maui.Storage.FileSystem.AppDataDirectory
            let path = Path.Combine(fld,"screenshot_m.jpeg")
            System.Diagnostics.Debug.WriteLine(path)
            File.WriteAllBytes(path,img)
            return data
          }
          
          member _.goBack () =
            async {
              let! _ =
                runOnMainThreadUnit (fun () ->
                  if ctx.wrapper.CanGoBack then
                    ctx.wrapper.GoBack())
                |> Async.AwaitTask
              resetBootstrap ctx
              return ()
            }

          member _.goForward () =
            async {
              let! _ =
                runOnMainThreadUnit (fun () ->
                  if ctx.wrapper.CanGoForward then
                    ctx.wrapper.GoForward())
                |> Async.AwaitTask
              resetBootstrap ctx
              return ()
            }

          member _.goto target =
            async {
              let! _ =
                runOnMainThreadUnit (fun () -> ctx.wrapper.Source <- target)
                |> Async.AwaitTask
              ctx.lastUrl <- Some target
              resetBootstrap ctx
              return ()
            }

          member _.url () =
            async {
              let! meta = getMeta ctx
              match meta with
              | Some m ->
                match m.url with
                | Some url when not (String.IsNullOrWhiteSpace url) ->
                  ctx.lastUrl <- Some url
                  return Some url
                | _ -> return ctx.lastUrl
              | None -> return ctx.lastUrl
            }

          member _.environment = "browser"
          member _.start arg =
            async {
              if String.IsNullOrWhiteSpace arg then
                return ()
              else
                let! _ =
                  runOnMainThreadUnit (fun () -> ctx.wrapper.Source <- arg)
                  |> Async.AwaitTask
                ctx.lastUrl <- Some arg
                resetBootstrap ctx
                return ()
            }

          member _.saveState () = async.Return ()

          member _.clearCookies () =
            async {
              let script = """
  (function () {
    try {
    if (!document.cookie) return JSON.stringify({ ok: true });
    document.cookie.split(';').forEach(function (cookie) {
      const eqPos = cookie.indexOf('=');
      const name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
      document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/';
    });
    return JSON.stringify({ ok: true });
    } catch (err) {
    return JSON.stringify({ ok: false, message: err && err.message });
    }
  })();
  """
              let! _ = runScript ctx script
              return ()
            }

          member _.reload () =
            async {
              let! _ = runOnMainThreadUnit (fun () -> ctx.wrapper.Reload()) |> Async.AwaitTask
              resetBootstrap ctx
              return ()
            }

          member _.getUrlBytes () =
            async {
              let! meta = getMeta ctx
              let currentUrl =
                meta
                |> Option.bind (fun m -> m.url)
                |> Option.orElse ctx.lastUrl
              match currentUrl with
              | Some url when not (String.IsNullOrWhiteSpace url) ->
                try
                  let! data = httpClient.Value.GetByteArrayAsync(url) |> Async.AwaitTask
                  return data
                with ex ->
                  Debug.WriteLine($"[MauiWebViewDriver] getUrlBytes failed: {ex.Message}")
                  return Array.empty
              | _ -> return Array.empty
            }

          member _.evaluateJavaScript expression =
            async {
              let script = $"""
  (function () {{
    try {{
    const result = eval({jsString expression});
    if (result instanceof Promise) {{
      return result.then(x => JSON.stringify({{ ok: true, value: x }}));
    }}
    return JSON.stringify({{ ok: true, value: result }});
    }} catch (err) {{
    return JSON.stringify({{ ok: false, error: err && err.message }});
    }}
  }})();
  """
              let! raw = runScript ctx script
              return if String.IsNullOrWhiteSpace raw then "" else raw
            }
        }

      { driver = driver }
