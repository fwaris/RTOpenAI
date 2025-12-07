namespace RT.Assistant
open System
open System.Text.Json
open System.Threading
open type Fabulous.Maui.View
open Microsoft.Maui.Controls
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open RT.Assistant.Plan
open RTOpenAI.Api.Events  

[<RequireQualifiedAccess>]
module Connect =
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
        
    //for production, the ephemeral key should be obtained from a server
    //after authenticating the mobile client via, say OAUTH
    let getOpenAIEphemKey apiKey (modelId:string) =
        task {
            let input = {KeyReq.Default with model = modelId}
            //use helper function in Api project to exchange 'real' OpenAI key for an ephemeral key for the RT api
            let! resp = RTOpenAI.Api.Exts.callApi<_,RTOpenAI.Api.Events.Session>(apiKey,RTOpenAI.Api.C.OPENAI_SESSION_API,input)
            return
                resp.client_secret
                |> Option.map _.value
                |> Option.defaultWith (fun _ -> failwith "Unable to get ephemeral key")
        }        
        
    let getEphemeralKey modelId =
        let apiKey = 
            Settings.Values.openaiKey()
            |> checkEmpty
            |> Option.defaultWith (fun () -> failwith "api key not set")
        getOpenAIEphemKey apiKey modelId
        
    let promptForKey() =
        Application.Current.Windows.[0].Page
            .DisplayAlert("Key","Input OpenAI real-time API key and connect again","Ok")
            
    let lookForKey() =
        task {
            try
                let key = Settings.Values.openaiKey() |> checkEmpty
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
                let! ephemeraKey = getEphemeralKey model.modelId
                do! RTOpenAI.Api.Connection.connect ephemeraKey conn
            | None -> failwith "connection not set"
        }
       
    //log all incoming server events to UI
    let logServerEvents (model:Model) (channel:Channels.Channel<JsonDocument>) =
        let comp = 
            channel.Reader.ReadAllAsync()
            |> AsyncSeq.ofAsyncEnum// listen to RT OpenAI server events coming over the WebRTC data channel
            |> AsyncSeq.map RTOpenAI.Api.Exts.toEvent//covert JSON to strongly typed event
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
                match checkEmpty (Settings.Values.openaiKey()) with
                | None -> return failwith "No api key" 
                | Some _ ->
                    match! Audio.haveRecordPermission model with
                    | Some permission ->                       
                        try
                            let conn = RTOpenAI.Api.Connection.create()
                            let sub = conn.WebRtcClient.StateChanged.Subscribe(fun ev -> model.mailbox.Writer.TryWrite ((WebRTC_StateChanged) ev) |> ignore)
                            let conn = {conn with Diposables = sub::conn.Diposables}                            
                            let dispatch = model.mailbox.Writer.TryWrite>>ignore
                            Plan.Machine.run model.hybridView conn dispatch
                            return (Some conn)
                        with ex ->
                            Log.exn (ex,"startStopConnection")
                            return raise ex
                    | None -> return failwith "unable to create audio recorder"
            | Some conn ->                                
                RTOpenAI.Api.Connection.close conn
                return None
        }
        
