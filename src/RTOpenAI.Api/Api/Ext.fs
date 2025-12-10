namespace RTOpenAI.Api
open System
open System.Net.Http
open System.Net.Http.Json
open System.Net.Http.Headers
open System.Text.Json
open RTOpenAI.Events

///RTOpenAI.Api helpers
module Exts =

    let callApiOptimized<'input,'output>(key:string,url:string,input:'input) =
        task {
            use wc = new HttpClient()
            wc.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", key)
            let! resp = wc.PostAsJsonAsync<'input>(Uri url, input)
            return! resp.Content.ReadFromJsonAsync<'output>()
        }
        
    let callApi<'input,'output>(key:string,url:string,input:'input) =
        task {
            use wc = new HttpClient()
            wc.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", key)
            let reqstr = JsonSerializer.Serialize(input)
            use strContent = new StringContent(reqstr,MediaTypeHeaderValue("application/json"))
            use! resp = wc.PostAsync(Uri url,strContent)
            if resp.StatusCode = Net.HttpStatusCode.OK || resp.StatusCode = Net.HttpStatusCode.Accepted then
                let! str = resp.Content.ReadAsStringAsync()
                if Log.debug_logging then Log.info $"Response: {str} "
                return JsonSerializer.Deserialize<'output>(str)
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
