namespace RTOpenAI.Sample
open System
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open type Fabulous.Maui.View
open Microsoft.Maui.Controls
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open RTOpenAI.Api
open RTOpenAI.Events  


[<RequireQualifiedAccess>]
module Connect =
                         
    let promptForKey() =
        Application.Current.Windows.[0].Page
            .DisplayAlert("Key","Input OpenAI real-time API key and connect again","Ok")
            
    let lookForKey() =
        task {
            try
                let key = Settings.Environment.apiKey() |> checkEmpty
                match key with
                | Some k -> return ()
                | None ->                 
                    do! MainThread.InvokeOnMainThreadAsync(promptForKey)
                    return raise InputKeyExn
            with ex ->
                Log.exn (ex,"lookForKey")
                return raise InputKeyExn 
        }
    
    let connect (model:Model) =
        task{
            match model.connection with
            | Some conn ->                
                let! ephemeraKey = Connection.getEphemeralKey (Settings.Environment.apiKey()) KeyReq.Default
                do! RTOpenAI.Api.Connection.connect ephemeraKey conn
            | None -> failwith "connection not set"
        }
       
    //log all incoming server events to UI
    let logServerEvents (model:Model) (channel:Channels.Channel<JsonDocument>) =
        let comp = 
            channel.Reader.ReadAllAsync()
            |> AsyncSeq.ofAsyncEnum// listen to RT OpenAI server events coming over the WebRTC data channel
            |> AsyncSeq.map SerDe.toEvent//covert JSON to strongly typed event
            |> AsyncSeq.iter (fun msg -> model.mailbox.Writer.TryWrite(Log_Append $"{DateTime.Now}: {msg}") |> ignore) // send events to UI to show in Log view
        async {
            match! Async.Catch comp with
            | Choice1Of2 _ -> Log.info "Done listening ot server events"
            | Choice2Of2 ex -> Log.exn(ex,"Error: logServerEvents")
        }
        |> Async.Start
        
    let startStopConnection (model:Model) =
        async {
            match model.connection with
            | None -> 
                match checkEmpty (Settings.Environment.apiKey()) with
                | None -> return failwith "No api key" 
                | Some _ ->
                    match! Audio.haveRecordPermission model with
                    | Some permission ->                       
                        try
                            let conn = RTOpenAI.Api.Connection.create()
                            let sub = conn.WebRtcClient.StateChanged.Subscribe(fun ev -> model.mailbox.Writer.TryWrite ((WebRTC_StateChanged) ev) |> ignore)
                            logServerEvents model conn.WebRtcClient.OutputChannel
                            let conn = {conn with Disposables = sub::conn.Disposables }                            
                            return (Some conn)
                        with ex ->
                            Log.exn (ex,"startStopSession")
                            return raise ex
                    | None -> return failwith "unable to create audio recorder"

            | Some conn ->                                
                RTOpenAI.Api.Connection.close conn
                return None
        }
        
