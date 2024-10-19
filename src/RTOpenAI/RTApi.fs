namespace rec RTOpenAI.Ws
open System

type ClientEvent =
| SessionUpdate             of SessionUpdate
| InputAudioBufferAppend    of InputAudioBufferAppend
| InputAudioBufferCommit    of InputAudioBufferCommit
| InputAudioBufferClear     of InputAudioBufferClear
| ConversationItemCreate    of ConversationItemCreate
| ConversationItemTruncate  of ConversationItemDelete
| ResponseCreate            of ResponseCreate
| ResponseCancel            of ResponseCancel

type SessionUpdate = {
    event_id: string
    ``type``: string
    session: Session
}

and Session = {
    modalities: string list
    instructions: string
    voice: string
    input_audio_format: string
    output_audio_format: string
    input_audio_transcription: InputAudioTranscription
    turn_detection: TurnDetection
    tools: Tool list
    tool_choice: string
    temperature: float
    max_output_tokens: int option
}

and InputAudioTranscription = {
    model: string
}

and TurnDetection = {
    ``type``: string
    threshold: float
    prefix_padding_ms: int
    silence_duration_ms: int
}

and Tool = {
    ``type``: string
    name: string
    description: string
    parameters: Parameters
}

and Parameters = {
    ``type``: string
    properties: Properties
    required: string list
}

and Properties = {
    location: Location option
    a: A option
    b: B option
}

and Location = {
    ``type``: string
}

and A = {
    ``type``: string
}

and B = {
    ``type``: string
}

type InputAudioBufferAppend = {
    event_id: string
    ``type``: string
    audio: string
}

type InputAudioBufferCommit = {
    event_id: string
    ``type``: string
}

type InputAudioBufferClear = {
    event_id: string
    ``type``: string
}

type ConversationItemCreate = {
    event_id: string
    ``type``: string
    previous_item_id: string option
    item: Item
}

and Item = {
    id: string
    ``type``: string
    status: string
    role: string
    content: Content list
}

and Content = {
    ``type``: string
    text: string
}

type ConversationItemTruncate = {
    event_id: string
    ``type``: string
    item_id: string
    content_index: int
    audio_end_ms: int
}

type ConversationItemDelete = {
    event_id: string
    ``type``: string
    item_id: string
}

type ResponseCreate = {
    event_id: string
    ``type``: string
    response: Response
}

and Response = {
    modalities: string list
    instructions: string
    voice: string
    output_audio_format: string
    tools: Tool list
    tool_choice: string
    temperature: float
    max_output_tokens: int
}

type ResponseCancel = {
    event_id: string
    ``type``: string
}


type ServerEvent =
| Error of ErrorDetail
| SessionCreated of SessionEvent
| SessionUpdated
| ConversationCreated
| ConversationItemCreated
| ConversationItemInputAudioTranscriptionCompleted
| ConversationItemInputAudioTranscriptionFailed
| ConversationItemTruncated
| ConversationItemDeleted
| InputAudioBufferCommitted
| InputAudioBufferCleared
| InputAudioBufferSpeechStarted
| InputAudioBufferSpeechStopped
| ResponseCreated
| ResponseDone
| ResponseOutputItemAdded
| ResponseOutputItemDone
| ResponseContentPartAdded
| ResponseContentPartDone
| ResponseTextDelta
| ResponseTextDone
| ResponseAudioTranscriptDelta
| ResponseAudioTranscriptDone
| ResponseAudioDelta
| ResponseAudioDone
| ResponseFunctionCallArgumentsDelta
| ResponseFunctionCallArgumentsDone
| RateLimitsUpdated

type ErrorDetail = {
    Type: string
    Code: string
    Message: string
    Param: string option
    EventId: string
}

type ErrorEvent = {
    EventId: string
    Type: string
    Error: ErrorDetail
}

type SessionEvent = {
    EventId: string
    Type: string
    Session: Session
}

type Conversation = {
    Id: string
    Object: string
}

type ConversationEvent = {
    EventId: string
    Type: string
    Conversation: Conversation
}

type ItemContent = {
    Type: string
    Transcript: string option
}


type ConversationItemEvent = {
    EventId: string
    Type: string
    PreviousItemId: string option
    Item: Item
}

type TranscriptionError = {
    Type: string
    Code: string
    Message: string
    Param: string option
}

type TranscriptionFailedEvent = {
    EventId: string
    Type: string
    ItemId: string
    ContentIndex: int
    Error: TranscriptionError
}

type TruncatedEvent = {
    EventId: string
    Type: string
    ItemId: string
    ContentIndex: int
    AudioEndMs: int
}

type DeletedEvent = {
    EventId: string
    Type: string
    ItemId: string
}

type InputAudioBufferEvent = {
    EventId: string
    Type: string
    PreviousItemId: string option
    ItemId: string option
    AudioStartMs: int option
    AudioEndMs: int option
}

type ResponseContent = {
    Type: string
    Text: string
}

type ResponseItem = {
    Id: string
    Object: string
    Type: string
    Status: string
    Role: string
    Content: ResponseContent list
}

type ResponseUsage = {
    TotalTokens: int
    InputTokens: int
    OutputTokens: int
}

type ResponseEvent = {
    EventId: string
    Type: string
    Response: Response
}

type RateLimit = {
    Name: string
    Limit: int
    Remaining: int
    ResetSeconds: int
}

type RateLimitsUpdatedEvent = {
    EventId: string
    Type: string
    RateLimits: RateLimit list
}
