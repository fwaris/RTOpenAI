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
    
    
[<Test>]
let ``response done sample with status`` () =
    let json = """
{
    "type": "response.done",
    "event_id": "event_Clb8cmQAEyzxia5U94QvZ",
    "response": {
        "object": "realtime.response",
        "id": "resp_Clb8c8yY8MPDK1OyZvYTs",
        "status": "cancelled",
        "status_details": {
            "type": "cancelled",
            "reason": "turn_detected"
        },
        "output": [],
        "conversation_id": "conv_Clb7UdIzg2XwJQ15Xiffe",
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
            "total_tokens": 0,
            "input_tokens": 0,
            "output_tokens": 0,
            "input_token_details": {
                "text_tokens": 0,
                "audio_tokens": 0,
                "image_tokens": 0,
                "cached_tokens": 0,
                "cached_tokens_details": {
                    "text_tokens": 0,
                    "audio_tokens": 0,
                    "image_tokens": 0
                }
            },
            "output_token_details": {
                "text_tokens": 0,
                "audio_tokens": 0
            }
        },
        "metadata": null
    }
}
"""
    let d = SerDe.toEvent (JsonDocument.Parse json)
    match d with
    | ServerEvent.EventHandlingError(t,msg,_) -> Assert.Fail(msg)
    | _ -> ()
    Assert.That(d.IsResponseDone)


[<Test>]
let ``response done sample with status details`` () =
    let json = """
{
    "type": "response.done",
    "event_id": "event_Cp0zTLKyWns5p8Er3DwCH",
    "response": {
        "object": "realtime.response",
        "id": "resp_Cp0zPfggltLKoly0LJBEi",
        "status": "completed",
        "status_details": null,
        "output": [
            {
                "id": "item_Cp0zPhs7xvoKPhK6t4YWs",
                "type": "message",
                "status": "completed",
                "role": "assistant",
                "content": [
                    {
                        "type": "output_audio",
                        "transcript": "Here are the plans that support four lines for 200 dollars or less and also include Netflix: \n\n1. Connect Plus \u2013 Price: 185 dollars for four lines\n2. Connect Next First Responder \u2013 Price: 200 dollars for four lines\n\nBoth of these plans meet your criteria."
                    }
                ]
            }
        ],
        "conversation_id": "conv_Cp0yNKRUDKGziRbZRKJd5",
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
            "total_tokens": 1099,
            "input_tokens": 684,
            "output_tokens": 415,
            "input_token_details": {
                "text_tokens": 455,
                "audio_tokens": 229,
                "image_tokens": 0,
                "cached_tokens": 0,
                "cached_tokens_details": {
                    "text_tokens": 0,
                    "audio_tokens": 0,
                    "image_tokens": 0
                }
            },
            "output_token_details": {
                "text_tokens": 83,
                "audio_tokens": 332
            }
        },
        "metadata": null
    }
}
"""
    let d = SerDe.toEvent (JsonDocument.Parse json)
    match d with
    | ServerEvent.EventHandlingError(t,msg,_) -> Assert.Fail(msg)
    | _ -> ()
    Assert.That(d.IsResponseDone)
