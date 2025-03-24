namespace RT.Assistant.Plan
open System.Text.Json
open FSharp.Control
open Microsoft.Maui.ApplicationModel
open RT.Assistant

exception TimeoutException
exception PrologError of string

module PlanQuery =
    open System.Threading
    open Fabulous
    
    type JSResp =
        {
            error : bool
            result : string
        }
        with static member Default = {error=false; result=""}

    type JSResponseHandler(w:ManualResetEvent) =
      member val Resp : JSResp = JSResp.Default with get, set
      member x.GotMessage(resp:JSResp) =
        x.Resp <- resp
        Utils.debug resp.result
        w.Set()
                    
    let fixCode (s:string) =
        s
            .Replace('\u2019','\'') //change 'smart' single quotes to regular ones
            .Replace('\u2018','\'')
            .Replace("?-","")        
        
    let withWebView(hybridView:ViewRef<Microsoft.Maui.Controls.HybridWebView>) respHndlr serCode ()=
        task{
                hybridView.Value.SetInvokeJavaScriptTarget(respHndlr)
                hybridView.Value.SendRawMessage(serCode)            
        }
            
    ///send generated code to prolog interpreter for evaluation
    let evalQuery (hybridView:ViewRef<Microsoft.Maui.Controls.HybridWebView>) (genCode:CodeGenResp) =         
      async {
        try
            if Utils.isEmpty genCode.Query then failwith "query is empty"
            let genCode = if genCode.Predicates = null then {genCode with Predicates=""} else genCode
            let genCode = {genCode with Predicates = fixCode genCode.Predicates; Query = fixCode genCode.Query}
            use signal = new ManualResetEvent(false)
            let respHndlr = new JSResponseHandler(signal)
            let serCode = JsonSerializer.Serialize(genCode,Utils.serOptionsFSharp)
            do! MainThread.InvokeOnMainThreadAsync<unit>(withWebView hybridView respHndlr serCode) |> Async.AwaitTask
            Log.info ($"prolog query: {genCode}")
            let! r =  Async.AwaitWaitHandle(signal,2000)
            if r then
              Log.info $"query resp: '{respHndlr.Resp.result}'"
              if respHndlr.Resp.error then
                return raise (PrologError respHndlr.Resp.result)
              else 
                return respHndlr.Resp.result
            else
              return raise TimeoutException
        with ex ->
          Log.exn(ex,"evalQuery")
          return raise ex
    }
