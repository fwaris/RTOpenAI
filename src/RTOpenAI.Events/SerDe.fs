namespace RTOpenAI.Events
open System.Text.Json
open System.Text.Json.Serialization

///RTOpenAI.Api helpers
module SerDe =
    let _serOptionsFSharp = 
        let o = JsonSerializerOptions(JsonSerializerDefaults.General)
        o.Converters.Add(OutputTokensTypeConverter())
        o.WriteIndented <- true
        o.ReadCommentHandling <- JsonCommentHandling.Skip        
        let opts = JsonFSharpOptions.Default()
        opts
            //.WithSkippableOptionFields(true)
            // .WithAllowNullFields()
            // .WithUnionUnwrapRecordCases()
            .AddToJsonSerializerOptions(o)        
        o
        
    let serOpts =
        let opts =
            JsonFSharpOptions.Default()
                .WithUnionInternalTag()
                .WithUnionTagName("type")
                .WithUnionUnwrapRecordCases()
                //.WithUnionTagCaseInsensitive()
                .WithAllowNullFields()
                .WithAllowOverride()
                .WithUnionUnwrapFieldlessTags()
                .ToJsonSerializerOptions()
        opts.WriteIndented <- true
        opts
        
    let private serialize<'t> (message: 't) = JsonSerializer.Serialize(message,serOpts)
    let private deserialize<'t> (j:JsonDocument) = JsonSerializer.Deserialize<'t>(j)
   
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

    let toEvent (j: JsonDocument) =
        let eventType = j.RootElement.GetProperty("type").GetString()
        try
            match eventType with
            | "error" -> ServerEvent.Error (deserialize<Error> j)
            //session
            | "session.created" -> ServerEvent.SessionCreated (deserialize<SessionCreated> j)
            | "session.updated" -> ServerEvent.SessionUpdated (deserialize<SessionUpdated> j)
            //conversation item
            | "conversation.item.added" -> ServerEvent.ConversationItemAdded (deserialize<ConversationItemEvent> j)
            | "conversation.item.done" -> ServerEvent.ConversationItemDone (deserialize<ConversationItemEvent> j)
            | "conversation.item.retrieved" -> ServerEvent.ConversationItemRetrieved (deserialize<ConversationItemEvent> j)
            | "conversation.item.truncated" -> ServerEvent.ConversationItemTruncated (deserialize<ConversationItemTruncated> j)
            | "conversation.item.deleted" -> ServerEvent.ConversationItemDeleted (deserialize<ConversationItemDeleted> j)
            //conversation item input audio transcription
            | "conversation.item.input_audio_transcription.completed" -> ServerEvent.ConversationItemInputAudioTranscriptionCompleted(deserialize<ConversationItemInputAudioTranscriptionCompleted> j)
            | "conversation.item.input_audio_transcription.delta" -> ServerEvent.ConversationItemInputAudioTranscriptionDelta(deserialize<ConversationItemInputAudioTranscriptionDelta> j)
            | "conversation.item.input_audio_transcription.segment" -> ServerEvent.ConversationItemInputAudioTranscriptionSegment(deserialize<ConversationItemInputAudioTranscriptionSegment> j)
            | "conversation.item.input_audio_transcription.failed" -> ServerEvent.ConversationItemInputAudioTranscriptionFailed(deserialize<ConversationItemInputAudioTranscriptionFailed> j)
            //input audio buffer
            | "input_audio_buffer.committed" -> ServerEvent.InputAudioBufferCommitted(deserialize<InputAudioBufferCommitted> j)
            | "input_audio_buffer.dtmf_event_received" -> ServerEvent.InputAudioBufferDtmfEventReceived(deserialize<InputAudioBufferDtmfEventReceived> j)
            | "input_audio_buffer.cleared" -> ServerEvent.InputAudioBufferCleared(deserialize<InputAudioBufferCleared> j)
            | "input_audio_buffer.speech_started" -> ServerEvent.InputAudioBufferSpeechStarted(deserialize<InputAudioBufferSpeechStarted> j)
            | "input_audio_buffer.speech_stopped" -> ServerEvent.InputAudioBufferSpeechStopped(deserialize<InputAudioBufferSpeechStopped> j)
            | "input_audio_buffer.timeout_triggered" -> ServerEvent.InputAudioBufferTimeoutTriggered(deserialize<InputAudioBufferTimeoutTriggered> j)
            //output audio buffer
            | "output_audio_buffer.started" -> ServerEvent.OutputAudioBufferStarted(deserialize<OutputAudioBufferStarted> j)
            | "output_audio_buffer.stopped" -> ServerEvent.OutputAudioBufferStopped(deserialize<OutputAudioBufferStopped> j)
            | "output_audio_buffer.cleared" -> ServerEvent.OutputAudioBufferCleared(deserialize<OutputAudioBufferCleared> j)
            //response
            | "response.created" -> ServerEvent.ResponseCreated(deserialize<ResponseCreated> j)
            | "response.done"    -> ServerEvent.ResponseDone(deserialize<ResponseDone> j)
            | "response.output_item.added" -> ServerEvent.ResponseOutputItemAdded(deserialize<ResponseOutputItemEvent> j)
            | "response.output_item.done" -> ServerEvent.ResponseOutputItemDone(deserialize<ResponseOutputItemEvent> j)
            | "response.content_part.added" -> ServerEvent.ResponseContentPartAdded(deserialize<ResponseContentPartEvent> j)
            | "response.content_part.done" -> ServerEvent.ResponseContentPartDone(deserialize<ResponseContentPartEvent> j)            
            | "response.output_text.delta" -> ServerEvent.ResponseOutputTextDelta(deserialize<ResponseOutputTextDeltaEvent> j)
            | "response.output_text.done" -> ServerEvent.ResponseOutputTextDone(deserialize<ResponseOutputTextDoneEvent> j)
            | "response.output_audio_transcript.delta" -> ServerEvent.ResponseOutputAudioTranscriptDelta(deserialize<ResponseOutputAudioTranscriptDeltaEvent> j)
            | "response.output_audio_transcript.done" -> ServerEvent.ResponseOutputAudioTranscriptDone(deserialize<ResponseOutputAudioTranscriptDoneEvent> j)
            | "response.output_audio.delta" -> ServerEvent.ResponseOutputAudioDelta(deserialize<ResponseOutputAudioDeltaEvent> j)
            | "response.output_audio.done" -> ServerEvent.ResponseOutputAudioDone(deserialize<ResponseOutputAudioDoneEvent> j)
            | "response.function_call_arguments.delta" -> ServerEvent.ResponseFunctionCallArgumentsDelta(deserialize<ResponseFunctionCallArgumentsDeltaEvent> j)
            | "response.function_call_arguments.done" -> ServerEvent.ResponseFunctionCallArgumentsDone(deserialize<ResponseFunctionCallArgumentsDoneEvent> j)
            | "response.mcp_call_arguments.delta" -> ServerEvent.ResponseMcpCallArgumentsDelta(deserialize<ResponseMcpCallArgumentsDeltaEvent> j)
            | "response.mcp_call_arguments.done" -> ServerEvent.ResponseMcpCallArgumentsDone(deserialize<ResponseFunctionCallArgumentsDoneEvent> j)
            | "response.mcp_call.in_progress" -> ServerEvent.ResponseMcpCallInProgress(deserialize<ResponseMcpEvent> j)
            | "response.mcp_call.completed" -> ServerEvent.ResponseMcpCallCompleted(deserialize<ResponseMcpEvent> j)
            | "response.mcp_call.failed" -> ServerEvent.ResponseMcpCallFailed(deserialize<ResponseMcpEvent> j)
            | "mcp_list_tools.in_progress" -> ServerEvent.ResponseMcpListToolsInProgress(deserialize<ResponseMcpEvent> j)
            | "mcp_list_tools.completed" -> ServerEvent.ResponseMcpListToolsCompleted(deserialize<ResponseMcpEvent> j)
            | "mcp_list_tools.failed" -> ServerEvent.ResponseMcpListToolsFailed(deserialize<ResponseMcpEvent> j)
            //others
            | "rate_limits.updated" -> ServerEvent.RateLimitsUpdated (deserialize<RateLimitsUpdatedEvent> j)
            | x -> ServerEvent.UnknownEvent (eventType,j)
        with ex -> 
            ServerEvent.UnknownEvent (eventType,j)
