namespace RTOpenAI
open System
open System.Net
open System.Threading
open System.Net.WebSockets
open System.Text
open System.Text.Json

module Pipe = 

    let sendRaw (client:ClientWebSocket) (message: string) = async {
        let bytes = Encoding.UTF8.GetBytes(message)
        let buffer = ArraySegment<byte>(bytes)
        do! client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None) |> Async.AwaitTask
    }

    let receiveRaw (client:ClientWebSocket) = async {
        let buffer = ArraySegment<byte>(Array.zeroCreate 1024)
        let! result = client.ReceiveAsync(buffer, CancellationToken.None) |> Async.AwaitTask
        let message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count)
        return message
    }

    let send<'t> (client:ClientWebSocket) (message: 't) = async {
        let json = JsonSerializer.Serialize(message)
        do! sendRaw client json
    }

    let receive<'t> (client:ClientWebSocket) = async {
        let! message = receiveRaw client
        let result = JsonSerializer.Deserialize<'t>(message)
        return result
    }

    let sendEvent (client:ClientWebSocket) (event: Events.ClientEvent) = async {
        match event with
        | Events.ClientEvent.SessionUpdate session -> 
            let message = JsonSerializer.Serialize(session)
            do! sendRaw client message
        | Events.ClientEvent.ConversationItemCreate item -> 
            let message = JsonSerializer.Serialize(item)
            do! sendRaw client message
        | Events.ClientEvent.ConversationItemDelete item -> 
            let message = JsonSerializer.Serialize(item)
            do! sendRaw client message
        | Events.ClientEvent.ConversationItemTruncate item -> 
            let message = JsonSerializer.Serialize(item)
            do! sendRaw client message
        | Events.ClientEvent.InputAudioBufferAppend buffer -> 
            let message = JsonSerializer.Serialize(buffer)
            do! sendRaw client message
        | Events.ClientEvent.InputAudioBufferClear buffer -> 
            let message = JsonSerializer.Serialize(buffer)
            do! sendRaw client message
        | Events.ClientEvent.InputAudioBufferCommit buffer -> 
            let message = JsonSerializer.Serialize(buffer)
            do! sendRaw client message
        | Events.ClientEvent.ResponseCancel response -> 
            let message = JsonSerializer.Serialize(response)
            do! sendRaw client message
        | Events.ClientEvent.ResponseCreate response -> 
            let message = JsonSerializer.Serialize(response)
            do! sendRaw client message
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

    let receiveEvent (client:ClientWebSocket) = async {
            let!  message = receive<JsonDocument> client
            return toEvent message
        }
