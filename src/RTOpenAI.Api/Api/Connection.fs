namespace RTOpenAI.Api
open FSharp.Control
open System
open RTOpenAI
open RTOpenAI.Api
open RTOpenAI.Api.Events

type Connection = {
        WebRtcClient : WebRTC.IWebRtcClient
        Diposables : IDisposable list
    }

module Connection =
    
    let create() = 
        let pc = RTOpenAI.WebRTC.WebRtc.create()
        {
            WebRtcClient = pc
            Diposables = []
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
        let codec : obj option = 
        #if WINDOWS 
            let codec = new Opus.Maui.Graph() 
            codec.InitializeAsync().Wait() //initializing audio graph in RTOpenAI causes 'window handle not valid' type errors so doing it here for now
            Some codec
        #else
            None
        #endif
        connection.WebRtcClient.Connect(ephemeralKey,C.OPENAI_RT_API,codec)

    let close (sess:Connection) = 
        sess.WebRtcClient.Dispose()     
        sess.Diposables |> List.iter _.Dispose()
     