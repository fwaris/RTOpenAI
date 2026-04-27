namespace RTOpenAI.Events
open System.Text.Json
open System.Text.Json.Serialization

/// String constants for every OpenAI Realtime API event "type" value
/// handled by <see cref="T:RTOpenAI.Events.SerDe"/>. Using these instead of
/// raw string literals gives callers compile-time protection against typos
/// when pattern matching on <see cref="T:RTOpenAI.Events.ServerEvent.UnknownEvent"/>
/// payloads or constructing test fixtures.
module EventTypes =
    let [<Literal>] Error                                              = "error"
    let [<Literal>] SessionCreated                                     = "session.created"
    let [<Literal>] SessionUpdated                                     = "session.updated"
    let [<Literal>] ConversationItemAdded                              = "conversation.item.added"
    let [<Literal>] ConversationItemDone                               = "conversation.item.done"
    let [<Literal>] ConversationItemRetrieved                          = "conversation.item.retrieved"
    let [<Literal>] ConversationItemTruncated                          = "conversation.item.truncated"
    let [<Literal>] ConversationItemDeleted                            = "conversation.item.deleted"
    let [<Literal>] ConversationItemInputAudioTranscriptionCompleted   = "conversation.item.input_audio_transcription.completed"
    let [<Literal>] ConversationItemInputAudioTranscriptionDelta       = "conversation.item.input_audio_transcription.delta"
    let [<Literal>] ConversationItemInputAudioTranscriptionSegment     = "conversation.item.input_audio_transcription.segment"
    let [<Literal>] ConversationItemInputAudioTranscriptionFailed      = "conversation.item.input_audio_transcription.failed"
    let [<Literal>] InputAudioBufferCommitted                          = "input_audio_buffer.committed"
    let [<Literal>] InputAudioBufferDtmfEventReceived                  = "input_audio_buffer.dtmf_event_received"
    let [<Literal>] InputAudioBufferCleared                            = "input_audio_buffer.cleared"
    let [<Literal>] InputAudioBufferSpeechStarted                      = "input_audio_buffer.speech_started"
    let [<Literal>] InputAudioBufferSpeechStopped                      = "input_audio_buffer.speech_stopped"
    let [<Literal>] InputAudioBufferTimeoutTriggered                   = "input_audio_buffer.timeout_triggered"
    let [<Literal>] OutputAudioBufferStarted                           = "output_audio_buffer.started"
    let [<Literal>] OutputAudioBufferStopped                           = "output_audio_buffer.stopped"
    let [<Literal>] OutputAudioBufferCleared                           = "output_audio_buffer.cleared"
    let [<Literal>] ResponseCreated                                    = "response.created"
    let [<Literal>] ResponseDone                                       = "response.done"
    let [<Literal>] ResponseOutputItemAdded                            = "response.output_item.added"
    let [<Literal>] ResponseOutputItemDone                             = "response.output_item.done"
    let [<Literal>] ResponseContentPartAdded                           = "response.content_part.added"
    let [<Literal>] ResponseContentPartDone                            = "response.content_part.done"
    let [<Literal>] ResponseOutputTextDelta                            = "response.output_text.delta"
    let [<Literal>] ResponseOutputTextDone                             = "response.output_text.done"
    let [<Literal>] ResponseOutputAudioTranscriptDelta                 = "response.output_audio_transcript.delta"
    let [<Literal>] ResponseOutputAudioTranscriptDone                  = "response.output_audio_transcript.done"
    let [<Literal>] ResponseOutputAudioDelta                           = "response.output_audio.delta"
    let [<Literal>] ResponseOutputAudioDone                            = "response.output_audio.done"
    let [<Literal>] ResponseFunctionCallArgumentsDelta                 = "response.function_call_arguments.delta"
    let [<Literal>] ResponseFunctionCallArgumentsDone                  = "response.function_call_arguments.done"
    let [<Literal>] ResponseMcpCallArgumentsDelta                      = "response.mcp_call_arguments.delta"
    let [<Literal>] ResponseMcpCallArgumentsDone                       = "response.mcp_call_arguments.done"
    let [<Literal>] ResponseMcpCallInProgress                          = "response.mcp_call.in_progress"
    let [<Literal>] ResponseMcpCallCompleted                           = "response.mcp_call.completed"
    let [<Literal>] ResponseMcpCallFailed                              = "response.mcp_call.failed"
    let [<Literal>] McpListToolsInProgress                             = "mcp_list_tools.in_progress"
    let [<Literal>] McpListToolsCompleted                              = "mcp_list_tools.completed"
    let [<Literal>] McpListToolsFailed                                 = "mcp_list_tools.failed"
    let [<Literal>] RateLimitsUpdated                                  = "rate_limits.updated"

/// Serialization helpers used by RTOpenAI.Api and its consumers to
/// convert between <see cref="T:RTOpenAI.Events.ClientEvent"/> /
/// <see cref="T:RTOpenAI.Events.ServerEvent"/> values and the JSON wire
/// format used by the OpenAI Realtime API.
module SerDe =

    /// Shared <see cref="T:System.Text.Json.JsonSerializerOptions"/> used
    /// for all client/server event (de)serialization. Exposed so that
    /// consumers which call <c>JsonSerializer.Deserialize</c> directly
    /// (e.g. for REST endpoints returning the same shapes) use identical
    /// settings.
    let serOpts =
        let opts =
            JsonFSharpOptions.Default()
                .WithUnionInternalTag()
                .WithUnionTagName("type")
                .WithUnionUnwrapRecordCases()
                .WithAllowNullFields()
                .WithAllowOverride()
                .WithUnionUnwrapFieldlessTags()
                .ToJsonSerializerOptions()
        opts.WriteIndented <- true
        opts.Converters.Insert(0, OutputTokensTypeConverter())
        opts

    let private serialize<'t> (message: 't) = JsonSerializer.Serialize(message, serOpts)
    let private deserialize<'t> (j: JsonDocument) = JsonSerializer.Deserialize<'t>(j.RootElement, serOpts)

    /// Serialize a <see cref="T:RTOpenAI.Events.ClientEvent"/> to a JSON string.
    /// Each case is unwrapped to its inner record before serialization; the inner
    /// record itself carries the literal "type" field expected by the OpenAI API
    /// (e.g. <c>"session.update"</c>), so we deliberately avoid round-tripping the
    /// union tag — otherwise FSharp.SystemTextJson would emit a second "type" field
    /// with the F# case name.
    let toJson (event: ClientEvent) =
        match event with
        | ClientEvent.SessionUpdate session          -> serialize session
        | ClientEvent.ConversationItemCreate item    -> serialize item
        | ClientEvent.ConversationItemDelete item    -> serialize item
        | ClientEvent.ConversationItemRetrieve item  -> serialize item
        | ClientEvent.ConversationItemTruncate item  -> serialize item
        | ClientEvent.InputAudioBufferAppend buffer  -> serialize buffer
        | ClientEvent.InputAudioBufferClear buffer   -> serialize buffer
        | ClientEvent.InputAudioBufferCommit buffer  -> serialize buffer
        | ClientEvent.ResponseCancel response        -> serialize response
        | ClientEvent.ResponseCreate response        -> serialize response
        | ClientEvent.OutputAudioBufferClear item    -> serialize item

    /// Lookup table mapping each OpenAI Realtime event-type string to the
    /// corresponding <see cref="T:RTOpenAI.Events.ServerEvent"/> constructor.
    /// Keeping this as a single table (rather than a long <c>match</c>) makes it
    /// obvious which events are covered and simplifies adding new ones.
    let private dispatch : Map<string, JsonDocument -> ServerEvent> =
        Map [
            EventTypes.Error,                                            fun j -> ServerEvent.Error (deserialize<Error> j)
            EventTypes.SessionCreated,                                   fun j -> ServerEvent.SessionCreated (deserialize<SessionCreated> j)
            EventTypes.SessionUpdated,                                   fun j -> ServerEvent.SessionUpdated (deserialize<SessionUpdated> j)
            EventTypes.ConversationItemAdded,                            fun j -> ServerEvent.ConversationItemAdded (deserialize<ConversationItemEvent> j)
            EventTypes.ConversationItemDone,                             fun j -> ServerEvent.ConversationItemDone (deserialize<ConversationItemEvent> j)
            EventTypes.ConversationItemRetrieved,                        fun j -> ServerEvent.ConversationItemRetrieved (deserialize<ConversationItemEvent> j)
            EventTypes.ConversationItemTruncated,                        fun j -> ServerEvent.ConversationItemTruncated (deserialize<ConversationItemTruncated> j)
            EventTypes.ConversationItemDeleted,                          fun j -> ServerEvent.ConversationItemDeleted (deserialize<ConversationItemDeleted> j)
            EventTypes.ConversationItemInputAudioTranscriptionCompleted, fun j -> ServerEvent.ConversationItemInputAudioTranscriptionCompleted (deserialize<ConversationItemInputAudioTranscriptionCompleted> j)
            EventTypes.ConversationItemInputAudioTranscriptionDelta,     fun j -> ServerEvent.ConversationItemInputAudioTranscriptionDelta (deserialize<ConversationItemInputAudioTranscriptionDelta> j)
            EventTypes.ConversationItemInputAudioTranscriptionSegment,   fun j -> ServerEvent.ConversationItemInputAudioTranscriptionSegment (deserialize<ConversationItemInputAudioTranscriptionSegment> j)
            EventTypes.ConversationItemInputAudioTranscriptionFailed,    fun j -> ServerEvent.ConversationItemInputAudioTranscriptionFailed (deserialize<ConversationItemInputAudioTranscriptionFailed> j)
            EventTypes.InputAudioBufferCommitted,                        fun j -> ServerEvent.InputAudioBufferCommitted (deserialize<InputAudioBufferCommitted> j)
            EventTypes.InputAudioBufferDtmfEventReceived,                fun j -> ServerEvent.InputAudioBufferDtmfEventReceived (deserialize<InputAudioBufferDtmfEventReceived> j)
            EventTypes.InputAudioBufferCleared,                          fun j -> ServerEvent.InputAudioBufferCleared (deserialize<InputAudioBufferCleared> j)
            EventTypes.InputAudioBufferSpeechStarted,                    fun j -> ServerEvent.InputAudioBufferSpeechStarted (deserialize<InputAudioBufferSpeechStarted> j)
            EventTypes.InputAudioBufferSpeechStopped,                    fun j -> ServerEvent.InputAudioBufferSpeechStopped (deserialize<InputAudioBufferSpeechStopped> j)
            EventTypes.InputAudioBufferTimeoutTriggered,                 fun j -> ServerEvent.InputAudioBufferTimeoutTriggered (deserialize<InputAudioBufferTimeoutTriggered> j)
            EventTypes.OutputAudioBufferStarted,                         fun j -> ServerEvent.OutputAudioBufferStarted (deserialize<OutputAudioBufferStarted> j)
            EventTypes.OutputAudioBufferStopped,                         fun j -> ServerEvent.OutputAudioBufferStopped (deserialize<OutputAudioBufferStopped> j)
            EventTypes.OutputAudioBufferCleared,                         fun j -> ServerEvent.OutputAudioBufferCleared (deserialize<OutputAudioBufferCleared> j)
            EventTypes.ResponseCreated,                                  fun j -> ServerEvent.ResponseCreated (deserialize<ResponseCreated> j)
            EventTypes.ResponseDone,                                     fun j -> ServerEvent.ResponseDone (deserialize<ResponseDone> j)
            EventTypes.ResponseOutputItemAdded,                          fun j -> ServerEvent.ResponseOutputItemAdded (deserialize<ResponseOutputItem> j)
            EventTypes.ResponseOutputItemDone,                           fun j -> ServerEvent.ResponseOutputItemDone (deserialize<ResponseOutputItem> j)
            EventTypes.ResponseContentPartAdded,                         fun j -> ServerEvent.ResponseContentPartAdded (deserialize<ResponseContentPart> j)
            EventTypes.ResponseContentPartDone,                          fun j -> ServerEvent.ResponseContentPartDone (deserialize<ResponseContentPart> j)
            EventTypes.ResponseOutputTextDelta,                          fun j -> ServerEvent.ResponseOutputTextDelta (deserialize<ResponseOutputTextDelta> j)
            EventTypes.ResponseOutputTextDone,                           fun j -> ServerEvent.ResponseOutputTextDone (deserialize<ResponseOutputTextDone> j)
            EventTypes.ResponseOutputAudioTranscriptDelta,               fun j -> ServerEvent.ResponseOutputAudioTranscriptDelta (deserialize<ResponseOutputAudioTranscriptDelta> j)
            EventTypes.ResponseOutputAudioTranscriptDone,                fun j -> ServerEvent.ResponseOutputAudioTranscriptDone (deserialize<ResponseOutputAudioTranscriptDone> j)
            EventTypes.ResponseOutputAudioDelta,                         fun j -> ServerEvent.ResponseOutputAudioDelta (deserialize<ResponseOutputAudioDelta> j)
            EventTypes.ResponseOutputAudioDone,                          fun j -> ServerEvent.ResponseOutputAudioDone (deserialize<ResponseOutputAudioDone> j)
            EventTypes.ResponseFunctionCallArgumentsDelta,               fun j -> ServerEvent.ResponseFunctionCallArgumentsDelta (deserialize<ResponseFunctionCallArgumentsDelta> j)
            EventTypes.ResponseFunctionCallArgumentsDone,                fun j -> ServerEvent.ResponseFunctionCallArgumentsDone (deserialize<ResponseFunctionCallArgumentsDone> j)
            EventTypes.ResponseMcpCallArgumentsDelta,                    fun j -> ServerEvent.ResponseMcpCallArgumentsDelta (deserialize<ResponseMcpCallArgumentsDelta> j)
            EventTypes.ResponseMcpCallArgumentsDone,                     fun j -> ServerEvent.ResponseMcpCallArgumentsDone (deserialize<ResponseMcpCallArgumentsDone> j)
            EventTypes.ResponseMcpCallInProgress,                        fun j -> ServerEvent.ResponseMcpCallInProgress (deserialize<ResponseMcp> j)
            EventTypes.ResponseMcpCallCompleted,                         fun j -> ServerEvent.ResponseMcpCallCompleted (deserialize<ResponseMcp> j)
            EventTypes.ResponseMcpCallFailed,                            fun j -> ServerEvent.ResponseMcpCallFailed (deserialize<ResponseMcp> j)
            EventTypes.McpListToolsInProgress,                           fun j -> ServerEvent.ResponseMcpListToolsInProgress (deserialize<ResponseMcp> j)
            EventTypes.McpListToolsCompleted,                            fun j -> ServerEvent.ResponseMcpListToolsCompleted (deserialize<ResponseMcp> j)
            EventTypes.McpListToolsFailed,                               fun j -> ServerEvent.ResponseMcpListToolsFailed (deserialize<ResponseMcp> j)
            EventTypes.RateLimitsUpdated,                                fun j -> ServerEvent.RateLimitsUpdated (deserialize<RateLimitsUpdated> j)
        ]

    /// Convert a JSON document received from the OpenAI Realtime API into a
    /// strongly-typed <see cref="T:RTOpenAI.Events.ServerEvent"/>. Never throws:
    /// malformed JSON (missing or non-string <c>"type"</c>) and unknown event
    /// type strings are surfaced as
    /// <see cref="T:RTOpenAI.Events.ServerEvent.EventHandlingError"/> and
    /// <see cref="T:RTOpenAI.Events.ServerEvent.UnknownEvent"/> respectively,
    /// so the caller can log or skip without crashing the connection loop.
    let toEvent (j: JsonDocument) =
        let mutable typeProp = Unchecked.defaultof<JsonElement>
        let eventType =
            if j.RootElement.ValueKind = JsonValueKind.Object
               && j.RootElement.TryGetProperty("type", &typeProp)
               && typeProp.ValueKind = JsonValueKind.String
            then typeProp.GetString()
            else ""
        if eventType = "" then
            ServerEvent.EventHandlingError (eventType, "missing or non-string 'type' property", j)
        else
            try
                match Map.tryFind eventType dispatch with
                | Some ctor -> ctor j
                | None -> ServerEvent.UnknownEvent (eventType, j)
            with ex ->
                ServerEvent.EventHandlingError (eventType, ex.Message, j)
