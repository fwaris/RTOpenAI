namespace RTOpenAI.Api

open System.Text.Json.Serialization
open FSharp.Control
open System
open RTOpenAI
open RTOpenAI.Api
open RTOpenAI.Events

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
            let! resp = RTOpenAI.Api.Exts.callApi<_,RTOpenAI.Events.KeyResp>(apiKey,RTOpenAI.Api.C.OPENAI_SESSION_API,keyReq)
            let errmsg =  "Unable to get ephemeral key"
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
        |> SerDe.toJson
        |> fun j -> Log.info $">>> {j.ToString()}"; j
        |> connection.WebRtcClient.Send
        |> ignore
        
    let defaultHandleServerEvent (connection:Connection) (ev:ServerEvent) =
        match ev with
        | ServerEvent.SessionCreated s ->
            {SessionUpdateEvent.Default with
                event_id = Utils.newId()
                session =
                    {s.session with
                        audio = Include Audio.Default
                    }
            }
            |> ClientEvent.SessionUpdate
            |> sendClientEvent connection
        | _ -> ()
        
    let connect ephemeralKey (connection:Connection) =
        connection.WebRtcClient.Connect(ephemeralKey,C.OPENAI_RT_API_CALLS)

    let close (sess:Connection) = 
        sess.WebRtcClient.Dispose()     
        sess.Disposables |> List.iter _.Dispose()
     