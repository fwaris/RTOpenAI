module RTOpenAI.Api.Test.Serialization

open NUnit.Framework
open System.Text.Json
open RTOpenAI.Events

[<SetUp>]
let Setup () =
    ()

// Helper function to serialize and deserialize
let serializeDeserializeTest<'T> (event: 'T) (expectedType: string) =
    let json = JsonSerializer.Serialize(event, Exts.serOpts)
    printfn "Serialized JSON: %s" json
    
    // Verify that the json contains the type field
    Assert.That(json, Does.Contain($"\"type\":\"{expectedType}\""))
    
    // Deserialize back
    let doc = JsonDocument.Parse(json)
    let serverEvent = Exts.toEvent doc
    
    serverEvent

// Error Event Tests
[<Test>]
let ``Error event serialization and deserialization`` () =
    let errorJson = """
    {
        "event_id": "event_890",
        "type": "error",
        "error": {
            "type": "invalid_request_error",
            "code": "invalid_event",
            "message": "The 'type' field is missing.",
            "param": null,
            "event_id": "event_567"
        }
    }"""
    
    let doc = JsonDocument.Parse(errorJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.Error e ->
        Assert.That(e.event_id, Is.EqualTo("event_890"))
        Assert.That(e.``type``, Is.EqualTo("error"))
        Assert.That(e.error.``type``, Is.EqualTo("invalid_request_error"))
        Assert.That(e.error.code, Is.EqualTo("invalid_event"))
        Assert.That(e.error.message, Is.EqualTo("The 'type' field is missing."))
    | _ -> Assert.Fail("Expected Error event")

// Session Created Event Tests
[<Test>]
let ``SessionCreated event deserialization`` () =
    let sessionCreatedJson = """
    {
        "type": "session.created",
        "event_id": "event_123",
        "session": {
            "id": "sess_001",
            "object": "realtime.session",
            "model": "gpt-realtime-2025-08-28",
            "modalities": ["audio", "text"],
            "instructions": "You are a helpful assistant",
            "voice": "alloy",
            "input_audio_format": "pcm16",
            "output_audio_format": "pcm16",
            "input_audio_transcription": null,
            "turn_detection": null,
            "tools": [],
            "tool_choice": "auto",
            "temperature": 0.8,
            "max_output_tokens": null
        }
    }"""
    
    let doc = JsonDocument.Parse(sessionCreatedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.SessionCreated e ->
        Assert.That(e.event_id, Is.EqualTo("event_123"))
        Assert.That(e.``type``, Is.EqualTo("session.created"))
    | _ -> Assert.Fail("Expected SessionCreated event")

// Session Updated Event Tests  
[<Test>]
let ``SessionUpdated event deserialization`` () =
    let sessionUpdatedJson = """
    {
        "type": "session.updated",
        "event_id": "event_456",
        "session": {
            "id": "sess_001",
            "object": "realtime.session",
            "model": "gpt-realtime-2025-08-28",
            "modalities": ["audio"],
            "instructions": "Updated instructions",
            "voice": "marin",
            "input_audio_format": "pcm16",
            "output_audio_format": "pcm16",
            "input_audio_transcription": null,
            "turn_detection": null,
            "tools": [],
            "tool_choice": "auto",
            "temperature": 0.7,
            "max_output_tokens": null
        }
    }"""
    
    let doc = JsonDocument.Parse(sessionUpdatedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.SessionUpdated e ->
        Assert.That(e.event_id, Is.EqualTo("event_456"))
        Assert.That(e.``type``, Is.EqualTo("session.updated"))
    | _ -> Assert.Fail("Expected SessionUpdated event")

// Conversation Item Added Event Tests
[<Test>]
let ``ConversationItemAdded event deserialization`` () =
    let itemAddedJson = """
    {
        "type": "conversation.item.added",
        "event_id": "event_C9G8pjSJCfRNEhMEnYAVy",
        "previous_item_id": null,
        "item": {
            "id": "item_C9G8pGVKYnaZu8PH5YQ9O",
            "type": "message",
            "status": "completed",
            "role": "user",
            "content": [
                {
                    "type": "input_text",
                    "text": "hi"
                }
            ]
        }
    }"""
    
    let doc = JsonDocument.Parse(itemAddedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemAdded e ->
        Assert.That(e.event_id, Is.EqualTo("event_C9G8pjSJCfRNEhMEnYAVy"))
        Assert.That(e.``type``, Is.EqualTo("conversation.item.added"))
    | _ -> Assert.Fail("Expected ConversationItemAdded event")

// Conversation Item Done Event Tests
[<Test>]
let ``ConversationItemDone event deserialization`` () =
    let itemDoneJson = """
    {
        "type": "conversation.item.done",
        "event_id": "event_CCXLgMZPo3qioWCeQa4WH",
        "previous_item_id": "item_CCXLecNJVIVR2HUy3ABLj",
        "item": {
            "id": "item_CCXLfxmM5sXVJVz4mCa2S",
            "type": "message",
            "status": "completed",
            "role": "assistant",
            "content": [
                {
                    "type": "output_audio",
                    "transcript": "Oh, I can hear you loud and clear!"
                }
            ]
        }
    }"""
    
    let doc = JsonDocument.Parse(itemDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_CCXLgMZPo3qioWCeQa4WH"))
        Assert.That(e.``type``, Is.EqualTo("conversation.item.done"))
    | _ -> Assert.Fail("Expected ConversationItemDone event")

// Conversation Item Retrieved Event Tests
[<Test>]
let ``ConversationItemRetrieved event deserialization`` () =
    let itemRetrievedJson = """
    {
        "type": "conversation.item.retrieved",
        "event_id": "event_CCXGSizgEppa2d4XbKA7K",
        "item": {
            "id": "item_CCXGRxbY0n6WE4EszhF5w",
            "object": "realtime.item",
            "type": "message",
            "status": "completed",
            "role": "assistant",
            "content": [
                {
                    "type": "audio",
                    "transcript": "Yes, I can hear you loud and clear.",
                    "audio": "8//2//v/9//q/+//+P/s...",
                    "format": "pcm16"
                }
            ]
        }
    }"""
    
    let doc = JsonDocument.Parse(itemRetrievedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemRetrieved e ->
        Assert.That(e.event_id, Is.EqualTo("event_CCXGSizgEppa2d4XbKA7K"))
        Assert.That(e.``type``, Is.EqualTo("conversation.item.retrieved"))
    | _ -> Assert.Fail("Expected ConversationItemRetrieved event")

// Input Audio Transcription Completed Event Tests
[<Test>]
let ``ConversationItemInputAudioTranscriptionCompleted event deserialization`` () =
    let transcriptionCompletedJson = """
    {
        "type": "conversation.item.input_audio_transcription.completed",
        "event_id": "event_CCXGRvtUVrax5SJAnNOWZ",
        "item_id": "item_CCXGQ4e1ht4cOraEYcuR2",
        "content_index": 0,
        "transcript": "Hey, can you hear me?",
        "usage": {
            "type": "tokens",
            "total_tokens": 22,
            "input_tokens": 13,
            "input_token_details": {
                "text_tokens": 0,
                "audio_tokens": 13
            },
            "output_tokens": 9
        }
    }"""
    
    let doc = JsonDocument.Parse(transcriptionCompletedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemInputAudioTranscriptionCompleted e ->
        Assert.That(e.event_id, Is.EqualTo("event_CCXGRvtUVrax5SJAnNOWZ"))
        Assert.That(e.transcript, Is.EqualTo("Hey, can you hear me?"))
    | _ -> Assert.Fail("Expected ConversationItemInputAudioTranscriptionCompleted event")

// Input Audio Transcription Delta Event Tests
[<Test>]
let ``ConversationItemInputAudioTranscriptionDelta event deserialization`` () =
    let transcriptionDeltaJson = """
    {
        "type": "conversation.item.input_audio_transcription.delta",
        "event_id": "event_CCXGRxsAimPAs8kS2Wc7Z",
        "item_id": "item_CCXGQ4e1ht4cOraEYcuR2",
        "content_index": 0,
        "delta": "Hey",
        "obfuscation": "aLxx0jTEciOGe"
    }"""
    
    let doc = JsonDocument.Parse(transcriptionDeltaJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemInputAudioTranscriptionDelta e ->
        Assert.That(e.event_id, Is.EqualTo("event_CCXGRxsAimPAs8kS2Wc7Z"))
        Assert.That(e.delta, Is.EqualTo("Hey"))
    | _ -> Assert.Fail("Expected ConversationItemInputAudioTranscriptionDelta event")

// Input Audio Transcription Segment Event Tests
[<Test>]
let ``ConversationItemInputAudioTranscriptionSegment event deserialization`` () =
    let transcriptionSegmentJson = """
    {
        "event_id": "event_6501",
        "type": "conversation.item.input_audio_transcription.segment",
        "item_id": "msg_011",
        "content_index": 0,
        "text": "hello",
        "id": "seg_0001",
        "speaker": "spk_1",
        "start": 0.0,
        "end": 0.4
    }"""
    
    let doc = JsonDocument.Parse(transcriptionSegmentJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemInputAudioTranscriptionSegment e ->
        Assert.That(e.event_id, Is.EqualTo("event_6501"))
        Assert.That(e.text, Is.EqualTo("hello"))
    | _ -> Assert.Fail("Expected ConversationItemInputAudioTranscriptionSegment event")

// Input Audio Transcription Failed Event Tests
[<Test>]
let ``ConversationItemInputAudioTranscriptionFailed event deserialization`` () =
    let transcriptionFailedJson = """
    {
        "event_id": "event_2324",
        "type": "conversation.item.input_audio_transcription.failed",
        "item_id": "msg_003",
        "content_index": 0,
        "error": {
            "type": "transcription_error",
            "code": "audio_unintelligible",
            "message": "The audio could not be transcribed.",
            "param": null
        }
    }"""
    
    let doc = JsonDocument.Parse(transcriptionFailedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemInputAudioTranscriptionFailed e ->
        Assert.That(e.event_id, Is.EqualTo("event_2324"))
        Assert.That(e.error.code, Is.EqualTo("audio_unintelligible"))
    | _ -> Assert.Fail("Expected ConversationItemInputAudioTranscriptionFailed event")

// Conversation Item Truncated Event Tests
[<Test>]
let ``ConversationItemTruncated event deserialization`` () =
    let itemTruncatedJson = """
    {
        "event_id": "event_2526",
        "type": "conversation.item.truncated",
        "item_id": "msg_004",
        "content_index": 0,
        "audio_end_ms": 1500
    }"""
    
    let doc = JsonDocument.Parse(itemTruncatedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemTruncated e ->
        Assert.That(e.event_id, Is.EqualTo("event_2526"))
        Assert.That(e.audio_end_ms, Is.EqualTo(1500))
    | _ -> Assert.Fail("Expected ConversationItemTruncated event")

// Conversation Item Deleted Event Tests
[<Test>]
let ``ConversationItemDeleted event deserialization`` () =
    let itemDeletedJson = """
    {
        "event_id": "event_2728",
        "type": "conversation.item.deleted",
        "item_id": "msg_005"
    }"""
    
    let doc = JsonDocument.Parse(itemDeletedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ConversationItemDeleted e ->
        Assert.That(e.event_id, Is.EqualTo("event_2728"))
        Assert.That(e.item_id, Is.EqualTo("msg_005"))
    | _ -> Assert.Fail("Expected ConversationItemDeleted event")

// Input Audio Buffer Committed Event Tests
[<Test>]
let ``InputAudioBufferCommitted event deserialization`` () =
    let bufferCommittedJson = """
    {
        "event_id": "event_1121",
        "type": "input_audio_buffer.committed",
        "previous_item_id": "msg_001",
        "item_id": "msg_002"
    }"""
    
    let doc = JsonDocument.Parse(bufferCommittedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.InputAudioBufferCommitted e ->
        Assert.That(e.event_id, Is.EqualTo("event_1121"))
        Assert.That(e.item_id, Is.EqualTo("msg_002"))
    | _ -> Assert.Fail("Expected InputAudioBufferCommitted event")

// Input Audio Buffer DTMF Event Received Tests
[<Test>]
let ``InputAudioBufferDtmfEventReceived event deserialization`` () =
    let dtmfEventJson = """
    {
        "type": "input_audio_buffer.dtmf_event_received",
        "event": "9",
        "received_at": 1763605109
    }"""
    
    let doc = JsonDocument.Parse(dtmfEventJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.InputAudioBufferDtmfEventReceived e ->
        Assert.That(e.event, Is.EqualTo("9"))
        Assert.That(e.received_at, Is.EqualTo(1763605109))
    | _ -> Assert.Fail("Expected InputAudioBufferDtmfEventReceived event")

// Input Audio Buffer Cleared Event Tests
[<Test>]
let ``InputAudioBufferCleared event deserialization`` () =
    let bufferClearedJson = """
    {
        "event_id": "event_1314",
        "type": "input_audio_buffer.cleared"
    }"""
    
    let doc = JsonDocument.Parse(bufferClearedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.InputAudioBufferCleared e ->
        Assert.That(e.event_id, Is.EqualTo("event_1314"))
    | _ -> Assert.Fail("Expected InputAudioBufferCleared event")

// Input Audio Buffer Speech Started Event Tests
[<Test>]
let ``InputAudioBufferSpeechStarted event deserialization`` () =
    let speechStartedJson = """
    {
        "event_id": "event_1516",
        "type": "input_audio_buffer.speech_started",
        "audio_start_ms": 1000,
        "item_id": "msg_003"
    }"""
    
    let doc = JsonDocument.Parse(speechStartedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.InputAudioBufferSpeechStarted e ->
        Assert.That(e.event_id, Is.EqualTo("event_1516"))
        Assert.That(e.audio_start_ms, Is.EqualTo(1000))
    | _ -> Assert.Fail("Expected InputAudioBufferSpeechStarted event")

// Input Audio Buffer Speech Stopped Event Tests
[<Test>]
let ``InputAudioBufferSpeechStopped event deserialization`` () =
    let speechStoppedJson = """
    {
        "event_id": "event_1718",
        "type": "input_audio_buffer.speech_stopped",
        "audio_end_ms": 2000,
        "item_id": "msg_003"
    }"""
    
    let doc = JsonDocument.Parse(speechStoppedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.InputAudioBufferSpeechStopped e ->
        Assert.That(e.event_id, Is.EqualTo("event_1718"))
        Assert.That(e.audio_end_ms, Is.EqualTo(2000))
    | _ -> Assert.Fail("Expected InputAudioBufferSpeechStopped event")

// Input Audio Buffer Timeout Triggered Event Tests
[<Test>]
let ``InputAudioBufferTimeoutTriggered event deserialization`` () =
    let timeoutTriggeredJson = """
    {
        "type": "input_audio_buffer.timeout_triggered",
        "event_id": "event_CEKKrf1KTGvemCPyiJTJ2",
        "audio_start_ms": 13216,
        "audio_end_ms": 19232,
        "item_id": "item_CEKKrWH0GiwN0ET97NUZc"
    }"""
    
    let doc = JsonDocument.Parse(timeoutTriggeredJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.InputAudioBufferTimeoutTriggered e ->
        Assert.That(e.event_id, Is.EqualTo("event_CEKKrf1KTGvemCPyiJTJ2"))
        Assert.That(e.audio_start_ms, Is.EqualTo(13216))
        Assert.That(e.audio_end_ms, Is.EqualTo(19232))
    | _ -> Assert.Fail("Expected InputAudioBufferTimeoutTriggered event")

// Output Audio Buffer Started Event Tests
[<Test>]
let ``OutputAudioBufferStarted event deserialization`` () =
    let bufferStartedJson = """
    {
        "event_id": "event_abc123",
        "type": "output_audio_buffer.started",
        "response_id": "resp_abc123"
    }"""
    
    let doc = JsonDocument.Parse(bufferStartedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.OutputAudioBufferStarted e ->
        Assert.That(e.event_id, Is.EqualTo("event_abc123"))
        Assert.That(e.response_id, Is.EqualTo("resp_abc123"))
    | _ -> Assert.Fail("Expected OutputAudioBufferStarted event")

// Output Audio Buffer Stopped Event Tests
[<Test>]
let ``OutputAudioBufferStopped event deserialization`` () =
    let bufferStoppedJson = """
    {
        "event_id": "event_abc123",
        "type": "output_audio_buffer.stopped",
        "response_id": "resp_abc123"
    }"""
    
    let doc = JsonDocument.Parse(bufferStoppedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.OutputAudioBufferStopped e ->
        Assert.That(e.event_id, Is.EqualTo("event_abc123"))
        Assert.That(e.response_id, Is.EqualTo("resp_abc123"))
    | _ -> Assert.Fail("Expected OutputAudioBufferStopped event")

// Output Audio Buffer Cleared Event Tests
[<Test>]
let ``OutputAudioBufferCleared event deserialization`` () =
    let bufferClearedJson = """
    {
        "event_id": "event_abc123",
        "type": "output_audio_buffer.cleared",
        "response_id": "resp_abc123"
    }"""
    
    let doc = JsonDocument.Parse(bufferClearedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.OutputAudioBufferCleared e ->
        Assert.That(e.event_id, Is.EqualTo("event_abc123"))
        Assert.That(e.response_id, Is.EqualTo("resp_abc123"))
    | _ -> Assert.Fail("Expected OutputAudioBufferCleared event")

// Response Created Event Tests
[<Test>]
let ``ResponseCreated event deserialization`` () =
    let responseCreatedJson = """
    {
        "type": "response.created",
        "event_id": "event_C9G8pqbTEddBSIxbBN6Os",
        "response": {
            "object": "realtime.response",
            "id": "resp_C9G8p7IH2WxLbkgPNouYL",
            "status": "in_progress",
            "status_details": null,
            "output": [],
            "conversation_id": "conv_C9G8mmBkLhQJwCon3hoJN",
            "output_modalities": ["audio"],
            "max_output_tokens": "inf",
            "audio": {
                "output": {
                    "format": {
                        "type": "audio/pcm",
                        "rate": 24000
                    },
                    "voice": "marin"
                }
            },
            "usage": null,
            "metadata": null
        }
    }"""
    
    let doc = JsonDocument.Parse(responseCreatedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseCreated e ->
        Assert.That(e.event_id, Is.EqualTo("event_C9G8pqbTEddBSIxbBN6Os"))
    | _ -> Assert.Fail("Expected ResponseCreated event")

// Response Done Event Tests
[<Test>]
let ``ResponseDone event deserialization`` () =
    let responseDoneJson = """
    {
        "type": "response.done",
        "event_id": "event_CCXHxcMy86rrKhBLDdqCh",
        "response": {
            "object": "realtime.response",
            "id": "resp_CCXHw0UJld10EzIUXQCNh",
            "status": "completed",
            "status_details": null,
            "output": [],
            "conversation_id": "conv_CCXHsurMKcaVxIZvaCI5m",
            "output_modalities": ["audio"],
            "max_output_tokens": "inf",
            "audio": {
                "output": {
                    "format": {
                        "type": "audio/pcm",
                        "rate": 24000
                    },
                    "voice": "alloy"
                }
            },
            "usage": {
                "total_tokens": 253,
                "input_tokens": 132,
                "output_tokens": 121,
                "input_token_details": {
                    "text_tokens": 119,
                    "audio_tokens": 13
                }
            },
            "metadata": null
        }
    }"""
    
    let doc = JsonDocument.Parse(responseDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_CCXHxcMy86rrKhBLDdqCh"))
    | _ -> Assert.Fail("Expected ResponseDone event")

// Response Output Item Added Event Tests
[<Test>]
let ``ResponseOutputItemAdded event deserialization`` () =
    let itemAddedJson = """
    {
        "event_id": "event_3334",
        "type": "response.output_item.added",
        "response_id": "resp_001",
        "output_index": 0,
        "item": {
            "id": "msg_007",
            "object": "realtime.item",
            "type": "message",
            "status": "in_progress",
            "role": "assistant",
            "content": []
        }
    }"""
    
    let doc = JsonDocument.Parse(itemAddedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputItemAdded e ->
        Assert.That(e.event_id, Is.EqualTo("event_3334"))
        Assert.That(e.response_id, Is.EqualTo("resp_001"))
    | _ -> Assert.Fail("Expected ResponseOutputItemAdded event")

// Response Output Item Done Event Tests
[<Test>]
let ``ResponseOutputItemDone event deserialization`` () =
    let itemDoneJson = """
    {
        "event_id": "event_3536",
        "type": "response.output_item.done",
        "response_id": "resp_001",
        "output_index": 0,
        "item": {
            "id": "msg_007",
            "object": "realtime.item",
            "type": "message",
            "status": "completed",
            "role": "assistant",
            "content": [
                {
                    "type": "text",
                    "text": "Sure, I can help with that."
                }
            ]
        }
    }"""
    
    let doc = JsonDocument.Parse(itemDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputItemDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_3536"))
        Assert.That(e.response_id, Is.EqualTo("resp_001"))
    | _ -> Assert.Fail("Expected ResponseOutputItemDone event")

// Response Content Part Added Event Tests
[<Test>]
let ``ResponseContentPartAdded event deserialization`` () =
    let contentPartAddedJson = """
    {
        "event_id": "event_3738",
        "type": "response.content_part.added",
        "response_id": "resp_001",
        "item_id": "msg_007",
        "output_index": 0,
        "content_index": 0,
        "part": {
            "type": "text",
            "text": ""
        }
    }"""
    
    let doc = JsonDocument.Parse(contentPartAddedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseContentPartAdded e ->
        Assert.That(e.event_id, Is.EqualTo("event_3738"))
        Assert.That(e.response_id, Is.EqualTo("resp_001"))
    | _ -> Assert.Fail("Expected ResponseContentPartAdded event")

// Response Content Part Done Event Tests
[<Test>]
let ``ResponseContentPartDone event deserialization`` () =
    let contentPartDoneJson = """
    {
        "event_id": "event_3940",
        "type": "response.content_part.done",
        "response_id": "resp_001",
        "item_id": "msg_007",
        "output_index": 0,
        "content_index": 0,
        "part": {
            "type": "text",
            "text": "Sure, I can help with that."
        }
    }"""
    
    let doc = JsonDocument.Parse(contentPartDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseContentPartDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_3940"))
        Assert.That(e.response_id, Is.EqualTo("resp_001"))
    | _ -> Assert.Fail("Expected ResponseContentPartDone event")

// Response Output Text Delta Event Tests
[<Test>]
let ``ResponseOutputTextDelta event deserialization`` () =
    let textDeltaJson = """
    {
        "event_id": "event_4142",
        "type": "response.output_text.delta",
        "response_id": "resp_001",
        "item_id": "msg_007",
        "output_index": 0,
        "content_index": 0,
        "delta": "Sure, I can h"
    }"""
    
    let doc = JsonDocument.Parse(textDeltaJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputTextDelta e ->
        Assert.That(e.event_id, Is.EqualTo("event_4142"))
        Assert.That(e.delta, Is.EqualTo("Sure, I can h"))
    | _ -> Assert.Fail("Expected ResponseOutputTextDelta event")

// Response Output Text Done Event Tests
[<Test>]
let ``ResponseOutputTextDone event deserialization`` () =
    let textDoneJson = """
    {
        "event_id": "event_4344",
        "type": "response.output_text.done",
        "response_id": "resp_001",
        "item_id": "msg_007",
        "output_index": 0,
        "content_index": 0,
        "text": "Sure, I can help with that."
    }"""
    
    let doc = JsonDocument.Parse(textDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputTextDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_4344"))
        Assert.That(e.text, Is.EqualTo("Sure, I can help with that."))
    | _ -> Assert.Fail("Expected ResponseOutputTextDone event")

// Response Output Audio Transcript Delta Event Tests
[<Test>]
let ``ResponseOutputAudioTranscriptDelta event deserialization`` () =
    let audioTranscriptDeltaJson = """
    {
        "event_id": "event_4546",
        "type": "response.output_audio_transcript.delta",
        "response_id": "resp_001",
        "item_id": "msg_008",
        "output_index": 0,
        "content_index": 0,
        "delta": "Hello, how can I a"
    }"""
    
    let doc = JsonDocument.Parse(audioTranscriptDeltaJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputAudioTranscriptDelta e ->
        Assert.That(e.event_id, Is.EqualTo("event_4546"))
        Assert.That(e.delta, Is.EqualTo("Hello, how can I a"))
    | _ -> Assert.Fail("Expected ResponseOutputAudioTranscriptDelta event")

// Response Output Audio Transcript Done Event Tests
[<Test>]
let ``ResponseOutputAudioTranscriptDone event deserialization`` () =
    let audioTranscriptDoneJson = """
    {
        "event_id": "event_4748",
        "type": "response.output_audio_transcript.done",
        "response_id": "resp_001",
        "item_id": "msg_008",
        "output_index": 0,
        "content_index": 0,
        "transcript": "Hello, how can I assist you today?"
    }"""
    
    let doc = JsonDocument.Parse(audioTranscriptDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputAudioTranscriptDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_4748"))
        Assert.That(e.transcript, Is.EqualTo("Hello, how can I assist you today?"))
    | _ -> Assert.Fail("Expected ResponseOutputAudioTranscriptDone event")

// Response Output Audio Delta Event Tests
[<Test>]
let ``ResponseOutputAudioDelta event deserialization`` () =
    let audioDeltaJson = """
    {
        "event_id": "event_4950",
        "type": "response.output_audio.delta",
        "response_id": "resp_001",
        "item_id": "msg_008",
        "output_index": 0,
        "content_index": 0,
        "delta": "Base64EncodedAudioDelta"
    }"""
    
    let doc = JsonDocument.Parse(audioDeltaJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputAudioDelta e ->
        Assert.That(e.event_id, Is.EqualTo("event_4950"))
        Assert.That(e.delta, Is.EqualTo("Base64EncodedAudioDelta"))
    | _ -> Assert.Fail("Expected ResponseOutputAudioDelta event")

// Response Output Audio Done Event Tests
[<Test>]
let ``ResponseOutputAudioDone event deserialization`` () =
    let audioDoneJson = """
    {
        "event_id": "event_5152",
        "type": "response.output_audio.done",
        "response_id": "resp_001",
        "item_id": "msg_008",
        "output_index": 0,
        "content_index": 0
    }"""
    
    let doc = JsonDocument.Parse(audioDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseOutputAudioDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_5152"))
    | _ -> Assert.Fail("Expected ResponseOutputAudioDone event")

// Response Function Call Arguments Delta Event Tests
[<Test>]
let ``ResponseFunctionCallArgumentsDelta event deserialization`` () =
    let functionCallDeltaJson = """
    {
        "event_id": "event_5354",
        "type": "response.function_call_arguments.delta",
        "response_id": "resp_002",
        "item_id": "fc_001",
        "output_index": 0,
        "call_id": "call_001",
        "delta": "{\"location\": \"San\""
    }"""
    
    let doc = JsonDocument.Parse(functionCallDeltaJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseFunctionCallArgumentsDelta e ->
        Assert.That(e.event_id, Is.EqualTo("event_5354"))
        Assert.That(e.call_id, Is.EqualTo("call_001"))
    | _ -> Assert.Fail("Expected ResponseFunctionCallArgumentsDelta event")

// Response Function Call Arguments Done Event Tests
[<Test>]
let ``ResponseFunctionCallArgumentsDone event deserialization`` () =
    let functionCallDoneJson = """
    {
        "event_id": "event_5556",
        "type": "response.function_call_arguments.done",
        "response_id": "resp_002",
        "item_id": "fc_001",
        "output_index": 0,
        "call_id": "call_001",
        "arguments": "{\"location\": \"San Francisco\"}"
    }"""
    
    let doc = JsonDocument.Parse(functionCallDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseFunctionCallArgumentsDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_5556"))
        Assert.That(e.arguments, Is.EqualTo("{\"location\": \"San Francisco\"}"))
    | _ -> Assert.Fail("Expected ResponseFunctionCallArgumentsDone event")

// Response MCP Call Arguments Delta Event Tests
[<Test>]
let ``ResponseMcpCallArgumentsDelta event deserialization`` () =
    let mcpCallDeltaJson = """
    {
        "event_id": "event_6201",
        "type": "response.mcp_call_arguments.delta",
        "response_id": "resp_001",
        "item_id": "mcp_call_001",
        "output_index": 0,
        "delta": "{\"partial\":true}"
    }"""
    
    let doc = JsonDocument.Parse(mcpCallDeltaJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpCallArgumentsDelta e ->
        Assert.That(e.event_id, Is.EqualTo("event_6201"))
        Assert.That(e.delta, Is.EqualTo("{\"partial\":true}"))
    | _ -> Assert.Fail("Expected ResponseMcpCallArgumentsDelta event")

// Response MCP Call Arguments Done Event Tests
[<Test>]
let ``ResponseMcpCallArgumentsDone event deserialization`` () =
    let mcpCallDoneJson = """
    {
        "event_id": "event_6202",
        "type": "response.mcp_call_arguments.done",
        "response_id": "resp_001",
        "item_id": "mcp_call_001",
        "output_index": 0,
        "arguments": "{\"q\":\"docs\"}"
    }"""
    
    let doc = JsonDocument.Parse(mcpCallDoneJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpCallArgumentsDone e ->
        Assert.That(e.event_id, Is.EqualTo("event_6202"))
        Assert.That(e.arguments, Is.EqualTo("{\"q\":\"docs\"}"))
    | _ -> Assert.Fail("Expected ResponseMcpCallArgumentsDone event")

// Response MCP Call In Progress Event Tests
[<Test>]
let ``ResponseMcpCallInProgress event deserialization`` () =
    let mcpCallInProgressJson = """
    {
        "event_id": "event_6301",
        "type": "response.mcp_call.in_progress",
        "output_index": 0,
        "item_id": "mcp_call_001"
    }"""
    
    let doc = JsonDocument.Parse(mcpCallInProgressJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpCallInProgress e ->
        Assert.That(e.event_id, Is.EqualTo("event_6301"))
        Assert.That(e.item_id, Is.EqualTo("mcp_call_001"))
    | _ -> Assert.Fail("Expected ResponseMcpCallInProgress event")

// Response MCP Call Completed Event Tests
[<Test>]
let ``ResponseMcpCallCompleted event deserialization`` () =
    let mcpCallCompletedJson = """
    {
        "event_id": "event_6302",
        "type": "response.mcp_call.completed",
        "output_index": 0,
        "item_id": "mcp_call_001"
    }"""
    
    let doc = JsonDocument.Parse(mcpCallCompletedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpCallCompleted e ->
        Assert.That(e.event_id, Is.EqualTo("event_6302"))
        Assert.That(e.item_id, Is.EqualTo("mcp_call_001"))
    | _ -> Assert.Fail("Expected ResponseMcpCallCompleted event")

// Response MCP Call Failed Event Tests
[<Test>]
let ``ResponseMcpCallFailed event deserialization`` () =
    let mcpCallFailedJson = """
    {
        "event_id": "event_6303",
        "type": "response.mcp_call.failed",
        "output_index": 0,
        "item_id": "mcp_call_001"
    }"""
    
    let doc = JsonDocument.Parse(mcpCallFailedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpCallFailed e ->
        Assert.That(e.event_id, Is.EqualTo("event_6303"))
        Assert.That(e.item_id, Is.EqualTo("mcp_call_001"))
    | _ -> Assert.Fail("Expected ResponseMcpCallFailed event")

// MCP List Tools In Progress Event Tests
[<Test>]
let ``ResponseMcpListToolsInProgress event deserialization`` () =
    let mcpListToolsInProgressJson = """
    {
        "event_id": "event_6101",
        "type": "mcp_list_tools.in_progress",
        "item_id": "mcp_list_tools_001"
    }"""
    
    let doc = JsonDocument.Parse(mcpListToolsInProgressJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpListToolsInProgress e ->
        Assert.That(e.event_id, Is.EqualTo("event_6101"))
        Assert.That(e.item_id, Is.EqualTo("mcp_list_tools_001"))
    | _ -> Assert.Fail("Expected ResponseMcpListToolsInProgress event")

// MCP List Tools Completed Event Tests
[<Test>]
let ``ResponseMcpListToolsCompleted event deserialization`` () =
    let mcpListToolsCompletedJson = """
    {
        "event_id": "event_6102",
        "type": "mcp_list_tools.completed",
        "item_id": "mcp_list_tools_001"
    }"""
    
    let doc = JsonDocument.Parse(mcpListToolsCompletedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpListToolsCompleted e ->
        Assert.That(e.event_id, Is.EqualTo("event_6102"))
        Assert.That(e.item_id, Is.EqualTo("mcp_list_tools_001"))
    | _ -> Assert.Fail("Expected ResponseMcpListToolsCompleted event")

// MCP List Tools Failed Event Tests
[<Test>]
let ``ResponseMcpListToolsFailed event deserialization`` () =
    let mcpListToolsFailedJson = """
    {
        "event_id": "event_6103",
        "type": "mcp_list_tools.failed",
        "item_id": "mcp_list_tools_001"
    }"""
    
    let doc = JsonDocument.Parse(mcpListToolsFailedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.ResponseMcpListToolsFailed e ->
        Assert.That(e.event_id, Is.EqualTo("event_6103"))
        Assert.That(e.item_id, Is.EqualTo("mcp_list_tools_001"))
    | _ -> Assert.Fail("Expected ResponseMcpListToolsFailed event")

// Rate Limits Updated Event Tests
[<Test>]
let ``RateLimitsUpdated event deserialization`` () =
    let rateLimitsUpdatedJson = """
    {
        "event_id": "event_5758",
        "type": "rate_limits.updated",
        "rate_limits": [
            {
                "name": "requests",
                "limit": 1000,
                "remaining": 999,
                "reset_seconds": 60
            },
            {
                "name": "tokens",
                "limit": 50000,
                "remaining": 49950,
                "reset_seconds": 60
            }
        ]
    }"""
    
    let doc = JsonDocument.Parse(rateLimitsUpdatedJson)
    let serverEvent = Exts.toEvent doc
    
    match serverEvent with
    | ServerEvent.RateLimitsUpdated e ->
        Assert.That(e.event_id, Is.EqualTo("event_5758"))
        Assert.That(e.rate_limits.Length, Is.EqualTo(2))
        Assert.That(e.rate_limits.[0].name, Is.EqualTo("requests"))
        Assert.That(e.rate_limits.[1].name, Is.EqualTo("tokens"))
    | _ -> Assert.Fail("Expected RateLimitsUpdated event")
