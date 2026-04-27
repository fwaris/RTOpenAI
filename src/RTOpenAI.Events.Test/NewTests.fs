module RTOpenAI.Api.Test.NewTests

open NUnit.Framework
open System.Text.Json
open System.Text.Json.Serialization
open RTOpenAI.Events

// ---------------------------------------------------------------------------
// P2.1 — coverage for ServerEvent cases not explicitly exercised elsewhere.
// ---------------------------------------------------------------------------

let private parse (json: string) = JsonDocument.Parse(json)

[<Test>]
let ``response.mcp_call.in_progress deserializes to ResponseMcpCallInProgress`` () =
    let json =
        """{
            "type": "response.mcp_call.in_progress",
            "event_id": "evt_mcp_1",
            "item_id": "item_mcp_1",
            "output_index": 0
        }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.ResponseMcpCallInProgress payload ->
        Assert.That(payload.event_id, Is.EqualTo("evt_mcp_1"))
        Assert.That(payload.item_id, Is.EqualTo("item_mcp_1"))
    | other -> Assert.Fail(sprintf "Unexpected: %A" other)

[<Test>]
let ``response.mcp_call.completed deserializes to ResponseMcpCallCompleted`` () =
    let json =
        """{
            "type": "response.mcp_call.completed",
            "event_id": "evt_mcp_2",
            "item_id": "item_mcp_2"
        }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.ResponseMcpCallCompleted payload ->
        Assert.That(payload.item_id, Is.EqualTo("item_mcp_2"))
    | other -> Assert.Fail(sprintf "Unexpected: %A" other)

[<Test>]
let ``response.mcp_call.failed deserializes to ResponseMcpCallFailed`` () =
    let json =
        """{
            "type": "response.mcp_call.failed",
            "event_id": "evt_mcp_3",
            "item_id": "item_mcp_3"
        }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.ResponseMcpCallFailed payload ->
        Assert.That(payload.item_id, Is.EqualTo("item_mcp_3"))
    | other -> Assert.Fail(sprintf "Unexpected: %A" other)

[<Test>]
let ``mcp_list_tools lifecycle deserializes into the three ResponseMcpListTools cases`` () =
    let mk stage =
        sprintf
            """{ "type": "mcp_list_tools.%s", "event_id": "evt", "item_id": "item" }"""
            stage
    match SerDe.toEvent (parse (mk "in_progress")) with
    | ServerEvent.ResponseMcpListToolsInProgress _ -> ()
    | other -> Assert.Fail(sprintf "in_progress: %A" other)
    match SerDe.toEvent (parse (mk "completed")) with
    | ServerEvent.ResponseMcpListToolsCompleted _ -> ()
    | other -> Assert.Fail(sprintf "completed: %A" other)
    match SerDe.toEvent (parse (mk "failed")) with
    | ServerEvent.ResponseMcpListToolsFailed _ -> ()
    | other -> Assert.Fail(sprintf "failed: %A" other)

[<Test>]
let ``input_audio_buffer.timeout_triggered deserializes into InputAudioBufferTimeoutTriggered`` () =
    let json =
        """{
            "type": "input_audio_buffer.timeout_triggered",
            "event_id": "evt_timeout",
            "item_id": "item_timeout",
            "audio_start_ms": 100,
            "audio_end_ms": 2500
        }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.InputAudioBufferTimeoutTriggered payload ->
        Assert.That(payload.audio_start_ms, Is.EqualTo(100))
        Assert.That(payload.audio_end_ms, Is.EqualTo(2500))
    | other -> Assert.Fail(sprintf "Unexpected: %A" other)

[<Test>]
let ``response.content_part.added deserializes into ResponseContentPartAdded`` () =
    let json =
        """{
            "type": "response.content_part.added",
            "event_id": "evt_cp",
            "item_id": "item_cp",
            "response_id": "resp_cp",
            "output_index": 0,
            "content_index": 0,
            "part": { "type": "text", "text": "hi" }
        }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.ResponseContentPartAdded payload ->
        Assert.That(payload.response_id, Is.EqualTo("resp_cp"))
    | other -> Assert.Fail(sprintf "Unexpected: %A" other)

[<Test>]
let ``response.content_part.done deserializes into ResponseContentPartDone`` () =
    let json =
        """{
            "type": "response.content_part.done",
            "event_id": "evt_cp2",
            "item_id": "item_cp2",
            "response_id": "resp_cp2",
            "output_index": 0,
            "content_index": 0,
            "part": { "type": "text", "text": "bye" }
        }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.ResponseContentPartDone payload ->
        Assert.That(payload.response_id, Is.EqualTo("resp_cp2"))
    | other -> Assert.Fail(sprintf "Unexpected: %A" other)

// ---------------------------------------------------------------------------
// P2.2 — negative-path tests: malformed input must surface as structured
// ServerEvent variants rather than throwing.
// ---------------------------------------------------------------------------

[<Test>]
let ``unknown type string becomes UnknownEvent`` () =
    let json = """{ "type": "something.new.the.library.doesnt.know" }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.UnknownEvent (eventType, _) ->
        Assert.That(eventType, Is.EqualTo("something.new.the.library.doesnt.know"))
    | other -> Assert.Fail(sprintf "Expected UnknownEvent, got %A" other)

[<Test>]
let ``missing type field becomes EventHandlingError with empty event type`` () =
    let json = """{ "event_id": "evt" }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.EventHandlingError (eventType, _, _) ->
        Assert.That(eventType, Is.EqualTo(""))
    | other -> Assert.Fail(sprintf "Expected EventHandlingError, got %A" other)

[<Test>]
let ``non-string type field becomes EventHandlingError`` () =
    let json = """{ "type": 42 }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.EventHandlingError (eventType, _, _) ->
        Assert.That(eventType, Is.EqualTo(""))
    | other -> Assert.Fail(sprintf "Expected EventHandlingError, got %A" other)

[<Test>]
let ``malformed payload for known type becomes EventHandlingError carrying eventType`` () =
    // "error" event expects an object-shaped error field; passing a scalar should
    // produce a deserialization failure.
    let json = """{ "type": "error", "error": 123 }"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.EventHandlingError (eventType, _, _) ->
        Assert.That(eventType, Is.EqualTo("error"))
    | other -> Assert.Fail(sprintf "Expected EventHandlingError, got %A" other)

[<Test>]
let ``non-object root becomes EventHandlingError`` () =
    let json = """[ { "type": "error" } ]"""
    match SerDe.toEvent (parse json) with
    | ServerEvent.EventHandlingError _ -> ()
    | other -> Assert.Fail(sprintf "Expected EventHandlingError, got %A" other)

// ---------------------------------------------------------------------------
// P2.3 — three-state optional fields: verify round-trip preserves
// Skip / Include None / Include (Some v) for Skippable<'t option> fields.
// Session.audio is a representative Skippable<Audio option> field.
// ---------------------------------------------------------------------------

[<Test>]
let ``Skip on Skippable<string option> field is omitted from the JSON`` () =
    let session = { Session.Default with id = Skip }
    let json = JsonSerializer.Serialize(session, SerDe.serOpts)
    Assert.That(json, Does.Not.Contain("\"id\""))

[<Test>]
let ``Include None on Skippable<string option> field serializes as null`` () =
    let session = { Session.Default with id = Include None }
    let json = JsonSerializer.Serialize(session, SerDe.serOpts)
    Assert.That(json, Does.Contain("\"id\": null"))

[<Test>]
let ``Include (Some v) on Skippable<string option> field serializes the value`` () =
    let session = { Session.Default with id = Include (Some "sess_123") }
    let json = JsonSerializer.Serialize(session, SerDe.serOpts)
    Assert.That(json, Does.Contain("\"id\": \"sess_123\""))

[<Test>]
let ``round-trip preserves Skip on Skippable<string option>`` () =
    let session = { Session.Default with id = Skip }
    let json = JsonSerializer.Serialize(session, SerDe.serOpts)
    let back = JsonSerializer.Deserialize<Session>(json, SerDe.serOpts)
    match back.id with
    | Skip -> ()
    | other -> Assert.Fail(sprintf "Expected Skip, got %A" other)

[<Test>]
let ``round-trip preserves Include None on Skippable<string option>`` () =
    let session = { Session.Default with id = Include None }
    let json = JsonSerializer.Serialize(session, SerDe.serOpts)
    let back = JsonSerializer.Deserialize<Session>(json, SerDe.serOpts)
    match back.id with
    | Include None -> ()
    | other -> Assert.Fail(sprintf "Expected Include None, got %A" other)

[<Test>]
let ``round-trip preserves Include (Some v) on Skippable<string option>`` () =
    let session = { Session.Default with id = Include (Some "sess_abc") }
    let json = JsonSerializer.Serialize(session, SerDe.serOpts)
    let back = JsonSerializer.Deserialize<Session>(json, SerDe.serOpts)
    match back.id with
    | Include (Some v) -> Assert.That(v, Is.EqualTo("sess_abc"))
    | other -> Assert.Fail(sprintf "Expected Include (Some sess_abc), got %A" other)
