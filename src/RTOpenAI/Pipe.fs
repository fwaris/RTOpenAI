namespace RTOpenAI
open System
open System.Net
open System.Threading
open System.Net.WebSockets
open System.Text
open System.Text.Json
open FSharp.Control.Websockets

module Pipe = 

    let sendRaw (client:ThreadSafeWebSocket.ThreadSafeWebSocket) (message: string) = async {
        return! ThreadSafeWebSocket.sendMessageAsUTF8 client message
    }

    let receiveRaw (client:ThreadSafeWebSocket.ThreadSafeWebSocket) = async {
        return! ThreadSafeWebSocket.receiveMessageAsUTF8 client
    }

    let send<'t> (client:ThreadSafeWebSocket.ThreadSafeWebSocket) (message: 't) = async {
        let json = JsonSerializer.Serialize(message)
        return! sendRaw client json
    }
    
    let sendSilent<'t> ws msg =
        async {
            match! send<'t> ws msg with
            | Result.Ok _ -> ()
            | Result.Error e -> Log.exn(e.SourceException,"sendSilent")            
        }

    let receive<'t> (client:ThreadSafeWebSocket.ThreadSafeWebSocket) = async {
        let! rslt = receiveRaw client
        return 
            rslt
            |> Result.map (function 
                | WebSocket.ReceiveUTF8Result.String text -> JsonSerializer.Deserialize<'t>(text) |> Some
                | _ -> None) //socket closed
    }

    let sendEvent (client:ThreadSafeWebSocket.ThreadSafeWebSocket) (event: Events.ClientEvent) = async {
        match event with
        | Events.ClientEvent.SessionUpdate session ->             
            return! send client session
        | Events.ClientEvent.ConversationItemCreate item ->          
            return! send client item
        | Events.ClientEvent.ConversationItemDelete item -> 
            return! send client item
        | Events.ClientEvent.ConversationItemTruncate item -> 
            return! send client item
        | Events.ClientEvent.InputAudioBufferAppend buffer -> 
            return! send client buffer
        | Events.ClientEvent.InputAudioBufferClear buffer -> 
            return! send client buffer
        | Events.ClientEvent.InputAudioBufferCommit buffer -> 
            return! send client buffer
        | Events.ClientEvent.ResponseCancel response -> 
            return! send client response
        | Events.ClientEvent.ResponseCreate response -> 
            return! send client response
    }

    let toEvent (j: JsonDocument) =
        let eventType = j.RootElement.GetProperty("type").GetString()
        match eventType with
        | "error" -> Events.Error (JsonSerializer.Deserialize<Events.ErrorEvent>(j))
        | "session.created" -> Events.SessionCreated (JsonSerializer.Deserialize<Events.SessionCreatedEvent>(j))
        | "session.updated" -> Events.SessionUpdated (JsonSerializer.Deserialize<Events.SessionUpdatedEvent>(j))
        | "conversation.created" -> Events.ConversationCreated (JsonSerializer.Deserialize<Events.ConversationCreatedEvent>(j))
        | "conversation.item.created" -> Events.ConversationItemCreated (JsonSerializer.Deserialize<Events.ConversationItemCreatedEvent>(j))
        | "conversation.item.input_audio_transcription.completed" -> Events.ConversationItemInputAudioTranscriptionCompleted (JsonSerializer.Deserialize<Events.ConversationItemInputAudioTranscriptionCompletedEvent>(j))
        | "conversation.item.input_audio_transcription.failed" -> Events.ConversationItemInputAudioTranscriptionFailed (JsonSerializer.Deserialize<Events.ConversationItemInputAudioTranscriptionFailedEvent>(j))
        | "conversation.item.truncated" -> Events.ConversationItemTruncated (JsonSerializer.Deserialize<Events.ConversationItemTruncatedEvent>(j))
        | "conversation.item.deleted" -> Events.ConversationItemDeleted (JsonSerializer.Deserialize<Events.ConversationItemDeletedEvent>(j))
        | "input_audio_buffer.committed" -> Events.InputAudioBufferCommitted (JsonSerializer.Deserialize<Events.InputAudioBufferCommittedEvent>(j))
        | "input_audio_buffer.cleared" -> Events.InputAudioBufferCleared (JsonSerializer.Deserialize<Events.InputAudioBufferClearedEvent>(j))
        | "input_audio_buffer.speech_started" -> Events.InputAudioBufferSpeechStarted (JsonSerializer.Deserialize<Events.InputAudioBufferSpeechStartedEvent>(j))
        | "input_audio_buffer.speech_stopped" -> Events.InputAudioBufferSpeechStopped (JsonSerializer.Deserialize<Events.InputAudioBufferSpeechStoppedEvent>(j))
        | "response.created" -> Events.ResponseCreated (JsonSerializer.Deserialize<Events.ResponseCreatedEventServer>(j))
        | "response.done" -> Events.ResponseDone (JsonSerializer.Deserialize<Events.ResponseDoneEvent>(j))
        | "response.output_item.added" -> Events.ResponseOutputItemAdded (JsonSerializer.Deserialize<Events.ResponseOutputItemAddedEvent>(j))
        | "response.output_item.done" -> Events.ResponseOutputItemDone (JsonSerializer.Deserialize<Events.ResponseOutputItemDoneEvent>(j))
        | "response.content_part.added" -> Events.ResponseContentPartAdded (JsonSerializer.Deserialize<Events.ResponseContentPartAddedEvent>(j))
        | "response.content_part.done" -> Events.ResponseContentPartDone (JsonSerializer.Deserialize<Events.ResponseContentPartDoneEvent>(j))
        | "response.text.delta" -> Events.ResponseTextDelta (JsonSerializer.Deserialize<Events.ResponseTextDeltaEvent>(j))
        | "response.text.done" -> Events.ResponseTextDone (JsonSerializer.Deserialize<Events.ResponseTextDoneEvent>(j))
        | x -> failwith $"Unknown event type '{x}'"

    let receiveEvent (client:ThreadSafeWebSocket.ThreadSafeWebSocket) = async {
            let!  rslt = receive<JsonDocument> client
            return rslt |> Result.map (Option.map toEvent)
        }
