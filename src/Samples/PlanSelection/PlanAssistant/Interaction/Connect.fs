namespace RT.Assistant
open System
open System.Text.Json
open System.Threading
open type Fabulous.Maui.View
open Microsoft.Extensions.DependencyInjection
open Microsoft.Maui.Controls
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open RT.Assistant.Plan
open RTOpenAI.Events  

[<RequireQualifiedAccess>]
module Connect =
        
    let promptForKey() =
        Application.Current.Windows.[0].Page
            .DisplayAlert("Key","Input OpenAI real-time API key and connect again","Ok")         
      
    let checkKeys() =
        Settings.Values.anthropicKey() |> notEmpty
        && Settings.Values.openaiKey() |> notEmpty                
       
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
            match model.flow with
            | None ->
                if checkKeys() then 
                    match! Audio.haveRecordPermission model with
                    | Some permission ->                       
                        try
                            let conn = RTOpenAI.Api.Connection.create()
                            let sub = conn.WebRtcClient.StateChanged.Subscribe(fun ev -> model.mailbox.Writer.TryWrite ((WebRTC_StateChanged) ev) |> ignore)
                            let conn = {conn with Disposables = sub::conn.Disposables }
                            let flow = WorkFlow.StateMachine.create model.mailbox model.hybridView conn
                            flow.PostToFlow WorkFlow.Fl_Start
                            return (Some flow)
                        with ex ->
                            Log.exn (ex,"startStopConnection")
                            return raise ex
                    | None -> return failwith "unable to create audio recorder"
                else
                    return failwith "API keys not set"
            | Some f ->
                f.Terminate()
                return None
        }
        
