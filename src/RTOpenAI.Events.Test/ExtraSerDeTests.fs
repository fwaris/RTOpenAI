module RTOpenAI.Api.Test.ExtraSerDeTests

open NUnit.Framework
open System.Text.Json
open System.Text.Json.Serialization
open RTOpenAI.Events

[<SetUp>]
let Setup () =
    ()
    
[<Test>]
let ``response done sample`` () =
    let json = """
{
    "type": "response.done",
    "event_id": "event_ClOgW5qjRjLa9JkHxSjr6",
    "response": {
        "object": "realtime.response",
        "id": "resp_ClOgTNDMpHrqh2RbbpbQK",
        "status": "completed",
        "status_details": null,
        "output": [
            {
                "id": "item_ClOgTFiKRyh3Z5ISxFanO",
                "type": "message",
                "status": "completed",
                "role": "assistant",
                "content": [
                    {
                        "type": "output_audio",
                        "transcript": "Of course, I\u0027d be happy to help you. Let me find phone plans specifically for military veterans that offer two lines. I\u0027ll take a quick look at the available options."
                    }
                ]
            },
            {
                "id": "item_ClOgVOClfgKQyB8hJ0h0f",
                "type": "function_call",
                "status": "completed",
                "name": "planQuery",
                "call_id": "call_SoWh233BKLuwepXl",
                "arguments": "{  \n  \u0022query\u0022: \u0022find all plans in the military_veteran category that support 2 lines.\u0022 \n} \n"
            }
        ],
        "conversation_id": "conv_ClOgJ4h2J3HOAUctk0hfh",
        "output_modalities": [
            "audio"
        ],
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
            "total_tokens": 1610,
            "input_tokens": 1339,
            "output_tokens": 271,
            "input_token_details": {
                "text_tokens": 1278,
                "audio_tokens": 61,
                "image_tokens": 0,
                "cached_tokens": 0,
                "cached_tokens_details": {
                    "text_tokens": 0,
                    "audio_tokens": 0,
                    "image_tokens": 0
                }
            },
            "output_token_details": {
                "text_tokens": 86,
                "audio_tokens": 185
            }
        },
        "metadata": null
    }
}"""
    let d = SerDe.toEvent (JsonDocument.Parse json)
    match d with
    | ServerEvent.EventHandlingError(t,msg,_) -> Assert.Fail(msg)
    | _ -> ()
    Assert.That(d.IsResponseDone)
    
[<Test>]
let ``conversation item done sample``() =
    let json = """
{
    "type": "conversation.item.done",
    "event_id": "event_ClOuC5LFUtyLmuwPrkhr3",
    "previous_item_id": null,
    "item": {
        "id": "item_ClOu8aZhwLPxVgMTM6Pzo",
        "type": "message",
        "status": "completed",
        "role": "user",
        "content": [
            {
                "type": "input_audio",
                "transcript": null
            }
        ]
    }
}
"""
    let d = SerDe.toEvent(JsonDocument.Parse json)
    match d with
    | ServerEvent.EventHandlingError(t,msg,_) -> Assert.Fail(msg)
    | _ -> ()
    Assert.That(d.IsConversationItemDone)
    