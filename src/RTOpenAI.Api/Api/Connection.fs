namespace RTOpenAI.Api
open FSharp.Control
open System
open RTOpenAI
open RTOpenAI.Api
open RTOpenAI.Api.Events

type KeyReq = {
    model : string
    modalities : string list
    instructions : string
}
with static member Default = {
        model = ""
        modalities = ["audio"; "text"]
        instructions = "You are a friendly assistant"
    }

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
            let! resp = RTOpenAI.Api.Exts.callApi<_,RTOpenAI.Api.Events.Session>(apiKey,RTOpenAI.Api.C.OPENAI_SESSION_API,keyReq)
            return
                resp.client_secret
                |> Option.map _.value
                |> Option.defaultWith (fun _ -> failwith "Unable to get ephemeral key")
        }       
    
    let create() = 
        let pc = RTOpenAI.WebRTC.WebRtc.create()
        {
            WebRtcClient = pc
            Disposables = []
        }
       
    let sendClientEvent connection ev =
        ev
        |> Exts.toJson
        |> fun j -> Log.info $">>> {j.ToString()}"; j
        |> connection.WebRtcClient.Send
        |> ignore
        
    let defaultHandleServerEvent (connection:Connection) (ev:ServerEvent) =
        match ev with
        | SessionCreated s ->
            {SessionUpdateEvent.Default with
                event_id = Utils.newId()
                session =
                    {s.session with
                        id=None
                        object=None
                        input_audio_transcription = Some InputAudioTranscription.Default}}
            |> ClientEvent.SessionUpdate
            |> sendClientEvent connection
        | _ -> ()
        
    let connect ephemeralKey (connection:Connection) =
        connection.WebRtcClient.Connect(ephemeralKey,C.OPENAI_RT_API)

    let close (sess:Connection) = 
        sess.WebRtcClient.Dispose()     
        sess.Disposables |> List.iter _.Dispose()
     