namespace RT.Assistant.WorkFlow

open System.Text.Json
open System.Threading
open Fabulous
open Microsoft.Maui.ApplicationModel
open RT.Assistant
open RT.Assistant.PlanQuery

module QueryService =
            
    ///send generated code to prolog interpreter for evaluation
    let evalQuery (hybridView:ViewRef<Microsoft.Maui.Controls.HybridWebView>) (genCode:CodeGenResp) =         
      async {
        try
            if Utils.isEmpty genCode.query then failwith "query is empty"
            let genCode = if genCode.predicates = null then {genCode with predicates=""} else genCode
            let genCode = {genCode with predicates = fixCode genCode.predicates; query = fixCode genCode.query }
            use signal = new ManualResetEvent(false)
            let respHndlr = new JSResponseHandler(signal)
            let serCode = JsonSerializer.Serialize(genCode,Utils.serOptionsFSharp)
            do! MainThread.InvokeOnMainThreadAsync<unit>(withWebView hybridView respHndlr serCode) |> Async.AwaitTask
            Log.info ($"prolog query: {genCode}")
            let! r =  Async.AwaitWaitHandle(signal,2000)
            if r then
              Log.info $"query resp: '{respHndlr.Resp.result}'"
              if respHndlr.Resp.error then
                return raise (PrologError (respHndlr.Resp.result |> String.concat "\n"))
              else 
                return respHndlr.Resp.result
            else
              return raise TimeoutException
        with ex ->
          Log.exn(ex,"evalQuery")
          return raise ex
    }
    
