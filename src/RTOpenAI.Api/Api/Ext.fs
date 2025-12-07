namespace RTOpenAI.Api
open System
open System.Text.Json.Serialization
open RTOpenAI.Api.Events
open System.Net.Http
open System.Net.Http.Json
open System.Net.Http.Headers
open System.Text.Json

///RTOpenAI.Api helpers
module Exts =
    let serOptionsFSharp = 
        let o = JsonSerializerOptions(JsonSerializerDefaults.General)
        o.Converters.Add(ContentTypeConverter())
        o.Converters.Add(ConversationItemTypeConverter())
        o.WriteIndented <- true
        o.ReadCommentHandling <- JsonCommentHandling.Skip        
        let opts = JsonFSharpOptions.Default()
        opts
            //.WithSkippableOptionFields(true)
            .WithAllowNullFields()
            .WithUnionUnwrapRecordCases()
            .AddToJsonSerializerOptions(o)        
        o
        
    let private serialize<'t> (message: 't) = JsonSerializer.Serialize(message,serOptionsFSharp)
   
    let toJson (event: ClientEvent) = 
        match event with
        | ClientEvent.SessionUpdate session          -> serialize session
        | ClientEvent.ConversationItemCreate item    -> serialize item            
        | ClientEvent.ConversationItemDelete item    -> serialize item
        | ClientEvent.ConversationItemTruncate item  -> serialize item
        | ClientEvent.InputAudioBufferAppend buffer  -> serialize buffer
        | ClientEvent.InputAudioBufferClear buffer   -> serialize buffer
        | ClientEvent.InputAudioBufferCommit buffer  -> serialize buffer
        | ClientEvent.ResponseCancel response        -> serialize response
        | ClientEvent.ResponseCreate response        -> serialize response

    let toEvent (j: JsonDocument) =
        let eventType = j.RootElement.GetProperty("type").GetString()
        debug $"Event type: {eventType}"
        try
            match eventType with
            | "error" -> Error (JsonSerializer.Deserialize<ErrorEvent>(j))
            | "session.created" -> SessionCreated (JsonSerializer.Deserialize<SessionCreatedEvent>(j,serOptionsFSharp))
            | "session.updated" -> SessionUpdated (JsonSerializer.Deserialize<SessionUpdatedEvent>(j,serOptionsFSharp))
            | "conversation.created" -> ConversationCreated (JsonSerializer.Deserialize<ConversationCreatedEvent>(j,serOptionsFSharp))
            | "conversation.item.created" -> ConversationItemCreated (JsonSerializer.Deserialize<ConversationItemCreatedEvent>(j,serOptionsFSharp))
            | "conversation.item.input_audio_transcription.completed" -> ConversationItemInputAudioTranscriptionCompleted (JsonSerializer.Deserialize<ConversationItemInputAudioTranscriptionCompletedEvent>(j,serOptionsFSharp))
            | "conversation.item.input_audio_transcription.failed" -> ConversationItemInputAudioTranscriptionFailed (JsonSerializer.Deserialize<ConversationItemInputAudioTranscriptionFailedEvent>(j,serOptionsFSharp))
            | "conversation.item.truncated" -> ConversationItemTruncated (JsonSerializer.Deserialize<ConversationItemTruncatedEvent>(j,serOptionsFSharp))
            | "conversation.item.deleted" -> ConversationItemDeleted (JsonSerializer.Deserialize<ConversationItemDeletedEvent>(j,serOptionsFSharp))
            | "input_audio_buffer.committed" -> InputAudioBufferCommitted (JsonSerializer.Deserialize<InputAudioBufferCommittedEvent>(j,serOptionsFSharp))
            | "input_audio_buffer.cleared" -> InputAudioBufferCleared (JsonSerializer.Deserialize<InputAudioBufferClearedEvent>(j,serOptionsFSharp))
            | "input_audio_buffer.speech_started" -> InputAudioBufferSpeechStarted (JsonSerializer.Deserialize<InputAudioBufferSpeechStartedEvent>(j,serOptionsFSharp))
            | "input_audio_buffer.speech_stopped" -> InputAudioBufferSpeechStopped (JsonSerializer.Deserialize<InputAudioBufferSpeechStoppedEvent>(j,serOptionsFSharp))
            | "response.created" -> ResponseCreated (JsonSerializer.Deserialize<ResponseCreatedEvent>(j,serOptionsFSharp))
            | "response.done" -> ResponseDone (JsonSerializer.Deserialize<ResponseDoneEvent>(j,serOptionsFSharp))
            | "response.output_item.added" -> ResponseOutputItemAdded (JsonSerializer.Deserialize<ResponseOutputItemAddedEvent>(j,serOptionsFSharp))
            | "response.output_item.done" -> ResponseOutputItemDone (JsonSerializer.Deserialize<ResponseOutputItemDoneEvent>(j,serOptionsFSharp))
            | "response.content_part.added" -> ResponseContentPartAdded (JsonSerializer.Deserialize<ResponseContentPartAddedEvent>(j,serOptionsFSharp))
            | "response.content_part.done" -> ResponseContentPartDone (JsonSerializer.Deserialize<ResponseContentPartDoneEvent>(j,serOptionsFSharp))
            | "response.text.delta" -> ResponseTextDelta (JsonSerializer.Deserialize<ResponseTextDeltaEvent>(j,serOptionsFSharp))
            | "response.text.done" -> ResponseTextDone (JsonSerializer.Deserialize<ResponseTextDoneEvent>(j,serOptionsFSharp))
            | "response.audio_transcript.delta" -> ResponseAudioTranscriptDelta(JsonSerializer.Deserialize<ResponseAudioTranscriptDeltaEvent>(j,serOptionsFSharp))
            | "rate_limits.updated" -> RateLimitsUpdated (JsonSerializer.Deserialize<RateLimitsUpdatedEvent>(j,serOptionsFSharp))
            | "response.audio.done" -> ResponseAudioDone (JsonSerializer.Deserialize<ResponseAudioDoneEvent>(j,serOptionsFSharp))
            | "response.audio_transcript.done" -> ResponseAudioTranscriptDone (JsonSerializer.Deserialize<ResponseAudioTranscriptDoneEvent>(j,serOptionsFSharp))
            | "response.function_call_arguments.delta" -> ResponseFunctionCallArgumentsDelta (JsonSerializer.Deserialize<ResponseFunctionCallArgumentsDeltaEvent>(j,serOptionsFSharp))
            | "response.function_call_arguments.done" -> ResponseFunctionCallArgumentsDone(JsonSerializer.Deserialize<ResponseFunctionCallArgumentsDoneEvent>(j,serOptionsFSharp))
            | "output_audio_buffer.started" -> UnknownEvent (eventType,j)
            | "output_audio_buffer.stopped" -> UnknownEvent (eventType,j)
            | "output_audio_buffer.cleared" -> UnknownEvent (eventType,j)
            | x -> Log.warn $"Unknown event type '{x}'"; UnknownEvent (eventType,j)
        with ex -> 
            Log.exn (ex, $"Error deserializing event {eventType}")
            UnknownEvent (eventType,j)





    let callApi<'input,'output>(key:string,url:string,input:'input) =
        task {
            use wc = new HttpClient()
            wc.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", key)
            let! resp = wc.PostAsJsonAsync<'input>(Uri url, input)
            return! resp.Content.ReadFromJsonAsync<'output>()
        }

    //request to get the ephemeral key    
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
  
    let getOpenAIEphemKey apiKey (keyRequest:KeyReq) =
        task {
            let! resp = callApi<_,RTOpenAI.Api.Events.Session>(apiKey,RTOpenAI.Api.C.OPENAI_SESSION_API,keyRequest) |> Async.AwaitTask
            return
                resp.client_secret
                |> Option.map _.value
                |> Option.defaultWith (fun _ -> failwith "Unable to get ephemeral key")
        }        
