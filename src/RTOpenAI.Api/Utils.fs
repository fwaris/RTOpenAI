namespace RTOpenAI.Api
open System
open System.IO
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Security.Cryptography

[<AutoOpen>]
module Utils =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)


    let notEmpty (s:string) = String.IsNullOrWhiteSpace s |> not
    let isEmpty (s:string) = String.IsNullOrWhiteSpace s 
    let contains (s:string) (ptrn:string) = s.Contains(ptrn,StringComparison.CurrentCultureIgnoreCase)
    let checkEmpty (s:string) = if isEmpty s then None else Some s

    let shorten len (s:string) = if s.Length > len then s.Substring(0,len) + "..." else s

    let (@@) a b = System.IO.Path.Combine(a,b)

    let logAndFail msg = Log.error msg; failwith msg
                
    ///send offer SDP to remote peer and obtain answer SDP (WebRTC protocol)
    let getAnswerSdp (ephemeralKey:string) (url:string) (offerSdp:string)  =
        task {            
            use wc = new HttpClient()
            wc.DefaultRequestHeaders.Clear()
            wc.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", ephemeralKey)
            use req = new HttpRequestMessage(HttpMethod.Post, url)
            let content = new StringContent(offerSdp,Encoding.UTF8)
            content.Headers.ContentType <- MediaTypeHeaderValue("application/sdp")
            req.Content <- content
            let! resp = wc.SendAsync(req)
            if not resp.IsSuccessStatusCode then
                Log.error $"Failed to get answer SDP. Status code {resp.StatusCode}"
                let! errResp = resp.Content.ReadAsStringAsync()
                Log.error errResp
                return failwith $"unable to connect"
            else
                let! answerSdp = resp.Content.ReadAsStringAsync()
                return answerSdp
        }
