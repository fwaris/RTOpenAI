namespace RTOpenAI.Api

open FSharp.Control
open System
open RTOpenAI
open RTOpenAI.Api
        
type Connection = {
        WebRtcClient : WebRTC.IWebRtcClient
        Disposables: IDisposable list
    }

module Connection =
        
    /// For production, the ephemeral key should be obtained from a server
    /// after authenticating the mobile client via say OAUTH or equivalent
    let getEphemeralKey apiKey (keyReq:KeyReq) =
        task {
            //use helper function in Api project to exchange 'real' OpenAI key for an ephemeral key for the RT api
            let! resp = Exts.callApi<_,KeyResp>(apiKey,C.OPENAI_SESSION_API,keyReq)
            return resp.value
        }       
    
    let create() = 
        let pc = RTOpenAI.WebRTC.WebRtc.create()
        {
            WebRtcClient = pc
            Disposables = []
        }
       
    let sendClientEvent connection ev =
        ev
        |> RTOpenAI.Events.SerDe.toJson
        |> fun j -> Log.info $">>> {j.ToString()}"; j
        |> connection.WebRtcClient.Send
        |> ignore
        
        
    let connect ephemeralKey (connection:Connection) =
        connection.WebRtcClient.Connect(ephemeralKey,C.OPENAI_RT_API_CALLS)

    let close (sess:Connection) = 
        sess.WebRtcClient.Dispose()     
        sess.Disposables |> List.iter _.Dispose()
     