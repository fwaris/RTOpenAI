namespace RTOpenAI.Api
open System
open System.Net.Http
open System.Net.Http.Json
open System.Net.Http.Headers
open System.Text.Json
open System.Text.Json.Serialization
open RTOpenAI.Events

[<JsonFSharpConverter>]
type ExpiresAfter = {anchor: string option; seconds: Skippable<int option>}

[<JsonFSharpConverter>]
type KeyReq = {
    expires_at : Skippable<ExpiresAfter> 
    session : Session
}
with static member Default = {
        expires_at = Skip
        session = { Session.Default with
                     model = Some C.OPENAI_RT_MODEL_GPT_REALTIME
                     instructions = Some "You are a helpful AI assistant"
                    }
    }

type KeyResp = {
   value : string
   expires_at : int64
   session : Session
}
///RTOpenAI.Api helpers
module Exts =

    let callApiOptimized<'input,'output>(key:string,url:string,input:'input) =
        task {
            use wc = new HttpClient()
            wc.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", key)
            let! resp = wc.PostAsJsonAsync<'input>(Uri url, input, options=SerDe.serOpts)
            return! resp.Content.ReadFromJsonAsync<'output>(options=SerDe.serOpts)
        }
        
    let callApi<'input,'output>(key:string,url:string,input:'input) =
        task {
            use wc = new HttpClient()
            wc.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", key)
            let reqstr = JsonSerializer.Serialize(input,SerDe.serOpts)
            use strContent = new StringContent(reqstr,MediaTypeHeaderValue("application/json"))
            use! resp = wc.PostAsync(Uri url,strContent)
            if resp.StatusCode = Net.HttpStatusCode.OK || resp.StatusCode = Net.HttpStatusCode.Accepted then
                let! str = resp.Content.ReadAsStringAsync()
                if Log.debug_logging then Log.info $"Response: {str} "
                return JsonSerializer.Deserialize<'output>(str,SerDe.serOpts)
            else
                let! err = resp.Content.ReadAsStringAsync()
                Log.error err
                return failwith err
        }
  
    let getOpenAIEphemKey apiKey (keyRequest:KeyReq) =
        task {
            let! resp = callApi<_,KeyResp>(apiKey,RTOpenAI.Api.C.OPENAI_SESSION_API,keyRequest) |> Async.AwaitTask
            return resp.value
        }        
