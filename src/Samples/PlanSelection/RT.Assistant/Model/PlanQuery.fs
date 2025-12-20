namespace RT.Assistant
open FSharp.Control
open RT.Assistant

exception TimeoutException
exception PrologError of string

module PlanQuery =
    open System.Threading
    open Fabulous
    
    type JSResp =
        {
            error : bool
            result : string list
        }
        with static member Default = {error=false; result=[]}

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
            