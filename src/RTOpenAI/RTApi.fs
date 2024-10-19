namespace rec RTOpenAI.Ws
open System

// Shared Types
type InputAudioTranscription =
    {
        model: string option
        enabled: bool option
    }

type TurnDetection =
    {
        ``type``: string
        threshold: float option
        prefix_padding_ms: int option
        silence_duration_ms: int option
    }

type Property =
    {
        ``type``: string
    }

type Parameters =
    {
        ``type``: string
        properties: Map<string, Property>
        required: string list
    }

type Tool =
    {
        ``type``: string
        name: string
        description: string
        parameters: Parameters
    }

type Session =
    {
        id: string option
        ``object``: string option
        model: string option
        modalities: string list option
        instructions: string option
        voice: string option
        input_audio_format: string option
        output_audio_format: string option
        input_audio_transcription: InputAudioTranscription option
        turn_detection: TurnDetection option
        tools: Tool list option
        tool_choice: string option
        temperature: float option
        max_output_tokens: int option
    }

type ErrorDetail =
    {
        ``type``: string
        code: string
        message: string
        param: string option
        event_id: string option
    }

type Usage =
    {
        total_tokens: int
        input_tokens: int
        output_tokens: int
    }

// Client Event Record Types
type SessionUpdateEvent =
    {
        event_id: string
        ``type``: string  // "session.update"
        session: Session
    }

type InputAudioBufferAppendEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.append"
        audio: string  // Base64 encoded audio data
    }

type InputAudioBufferCommitEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.commit"
    }

type InputAudioBufferClearEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.clear"
    }

type ConversationItemCreateEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.create"
        previous_item_id: string option
        item: ConversationItem
    }

type ConversationItemTruncateEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.truncate"
        item_id: string
        content_index: int
        audio_end_ms: int
    }

type ConversationItemDeleteEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.delete"
        item_id: string
    }

type ResponseCreateEvent =
    {
        event_id: string
        ``type``: string  // "response.create"
        response: Response
    }

type ResponseCancelEvent =
    {
        event_id: string
        ``type``: string  // "response.cancel"
    }

type ConversationItemContent =
    {
        ``type``: string
        text: string option
        transcript: string option
    }

type ConversationItem =
    {
        id: string
        ``type``: string  // e.g., "message"
        status: string
        role: string
        content: ConversationItemContent list
    }

type Response =
    {
        modalities: string list option
        instructions: string option
        voice: string option
        output_audio_format: string option
        tools: Tool list option
        tool_choice: string option
        temperature: float option
        max_output_tokens: int option
    }


type ClientEvent =
    | SessionUpdate of SessionUpdateEvent
    | InputAudioBufferAppend of InputAudioBufferAppendEvent
    | InputAudioBufferCommit of InputAudioBufferCommitEvent
    | InputAudioBufferClear of InputAudioBufferClearEvent
    | ConversationItemCreate of ConversationItemCreateEvent
    | ConversationItemTruncate of ConversationItemTruncateEvent
    | ConversationItemDelete of ConversationItemDeleteEvent
    | ResponseCreate of ResponseCreateEvent
    | ResponseCancel of ResponseCancelEvent

// Server Event Record Types
type ErrorEvent =
    {
        event_id: string
        ``type``: string  // "error"
        error: ErrorDetail
    }

type SessionCreatedEvent =
    {
        event_id: string
        ``type``: string  // "session.created"
        session: Session
    }

type SessionUpdatedEvent =
    {
        event_id: string
        ``type``: string  // "session.updated"
        session: Session
    }

type ConversationCreatedEvent =
    {
        event_id: string
        ``type``: string  // "conversation.created"
        conversation: Conversation
    }

type ConversationItemCreatedEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.created"
        previous_item_id: string option
        item: ConversationItem
    }

type ConversationItemInputAudioTranscriptionCompletedEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.input_audio_transcription.completed"
        item_id: string
        content_index: int
        transcript: string
    }

type ConversationItemInputAudioTranscriptionFailedEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.input_audio_transcription.failed"
        item_id: string
        content_index: int
        error: ErrorDetail
    }

type ConversationItemTruncatedEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.truncated"
        item_id: string
        content_index: int
        audio_end_ms: int
    }

type ConversationItemDeletedEvent =
    {
        event_id: string
        ``type``: string  // "conversation.item.deleted"
        item_id: string
    }

type InputAudioBufferCommittedEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.committed"
        previous_item_id: string option
        item_id: string
    }

type InputAudioBufferClearedEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.cleared"
    }

type InputAudioBufferSpeechStartedEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.speech_started"
        audio_start_ms: int
        item_id: string
    }

type InputAudioBufferSpeechStoppedEvent =
    {
        event_id: string
        ``type``: string  // "input_audio_buffer.speech_stopped"
        audio_end_ms: int
        item_id: string
    }

type ResponseCreatedEventServer =
    {
        event_id: string
        ``type``: string  // "response.created"
        response: ResponseDetails
    }

type ResponseDoneEvent =
    {
        event_id: string
        ``type``: string  // "response.done"
        response: ResponseDetails
    }

type ResponseOutputItemAddedEvent =
    {
        event_id: string
        ``type``: string  // "response.output_item.added"
        response_id: string
        output_index: int
        item: ResponseOutputItem
    }

type ResponseOutputItemDoneEvent =
    {
        event_id: string
        ``type``: string  // "response.output_item.done"
        response_id: string
        output_index: int
        item: ResponseOutputItem
    }

type ResponseContentPartAddedEvent =
    {
        event_id: string
        ``type``: string  // "response.content_part.added"
        response_id: string
        item_id: string
        output_index: int
        content_index: int
        part: ContentPart
    }

type ResponseContentPartDoneEvent =
    {
        event_id: string
        ``type``: string  // "response.content_part.done"
        response_id: string
        item_id: string
        output_index: int
        content_index: int
        part: ContentPart
    }

type ResponseTextDeltaEvent =
    {
        event_id: string
        ``type``: string  // "response.text.delta"
        response_id: string
        item_id: string
        output_index: int
        content_index: int
        delta: string
    }

type ResponseTextDoneEvent =
    {
        event_id: string
        ``type``: string  // "response.text.done"
        response_id: string
        item_id: string
        output_index: int
        content_index: int
        text: string
    }

type Conversation =
    {
        id: string
        ``object``: string
    }

type ResponseDetails =
    {
        id: string
        ``object``: string
        status: string
        status_details: string option
        output: ResponseOutputItem list
        usage: Usage option
    }

type ResponseOutputItem =
    {
        id: string
        ``object``: string
        ``type``: string
        status: string
        role: string
        content: ConversationItemContent list
    }

type ContentPart =
    {
        ``type``: string
        text: string option
        delta: string option
        transcript: string option
    }

type ServerEvent =
    | Error of ErrorEvent
    | SessionCreated of SessionCreatedEvent
    | SessionUpdated of SessionUpdatedEvent
    | ConversationCreated of ConversationCreatedEvent
    | ConversationItemCreated of ConversationItemCreatedEvent
    | ConversationItemInputAudioTranscriptionCompleted of ConversationItemInputAudioTranscriptionCompletedEvent
    | ConversationItemInputAudioTranscriptionFailed of ConversationItemInputAudioTranscriptionFailedEvent
    | ConversationItemTruncated of ConversationItemTruncatedEvent
    | ConversationItemDeleted of ConversationItemDeletedEvent
    | InputAudioBufferCommitted of InputAudioBufferCommittedEvent
    | InputAudioBufferCleared of InputAudioBufferClearedEvent
    | InputAudioBufferSpeechStarted of InputAudioBufferSpeechStartedEvent
    | InputAudioBufferSpeechStopped of InputAudioBufferSpeechStoppedEvent
    | ResponseCreated of ResponseCreatedEventServer
    | ResponseDone of ResponseDoneEvent
    | ResponseOutputItemAdded of ResponseOutputItemAddedEvent
    | ResponseOutputItemDone of ResponseOutputItemDoneEvent
    | ResponseContentPartAdded of ResponseContentPartAddedEvent
    | ResponseContentPartDone of ResponseContentPartDoneEvent
    | ResponseTextDelta of ResponseTextDeltaEvent
    | ResponseTextDone of ResponseTextDoneEvent
