namespace RTOpenAI.Api

open FSharp.Control
open System
open RTOpenAI
open RTOpenAI.Api
open RTOpenAI.WebRTC
        
type Connection = {
        WebRtcClient : WebRTC.IWebRtcClient
        Config : WebRtcClientConfig
        Disposables: IDisposable list
    }

module Connection =
        
    /// For production, the ephemeral key should be obtained from a server
    /// after authenticating the mobile client via say OAUTH or equivalent
    let getEphemeralKey apiKey (keyReq:KeyReq) =
        task {
            //use helper function in Api project to exchange 'real' OpenAI key for an ephemeral key for the RT api
            let! resp = Exts.callApi<_,KeyResp>(apiKey,Env.OPENAI_SESSION_API.Value,keyReq)
            return resp.value
        }       
    
    let createWithConfig config =
        let pc = RTOpenAI.WebRTC.WebRtc.create()
        {
            WebRtcClient = pc
            Config = WebRtcClientConfigHelpers.normalize config
            Disposables = []
        }

    let create() = 
        createWithConfig WebRtcClientConfig.Default
       
    let sendClientEvent connection ev =
        ev
        |> RTOpenAI.Events.SerDe.toJson
        |> fun j -> Log.info $">>> {j.ToString()}"; j
        |> connection.WebRtcClient.Send
        |> ignore
        
        
    let connect ephemeralKey (connection:Connection) =
        connection.WebRtcClient.Connect(ephemeralKey,Env.OPENAI_RT_API_CALLS.Value,connection.Config)

    let connectTo url key (connection: Connection) =
        connection.WebRtcClient.Connect(key,url,connection.Config)

    let close (sess:Connection) = 
        sess.WebRtcClient.Dispose()     
        sess.Disposables |> List.iter _.Dispose()
     
