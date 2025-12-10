#load "packages.fsx"
#load "../../../../RTOpenAI.Api/Constants.fs"
#load "../../../../RTOpenAI.Api/Api/Events.fs"


open System.Text.Json
open System.Text.Json.Serialization
open RTOpenAI.Api.Events

let serOptionsFSharp = 
    let o = JsonSerializerOptions(JsonSerializerDefaults.General)
    o.Converters.Add(OutputTokensTypeConverter())
    // o.Converters.Add(ContentTypeConverter())
    // o.Converters.Add(ConversationItemTypeConverter())
    o.WriteIndented <- true
    //o.ReadCommentHandling <- JsonCommentHandling.Skip        
    let opts = JsonFSharpOptions.Default()
    opts
    //     //.WithSkippableOptionFields(true)
    //     //.WithAllowNullFields()
         .WithUnionUnwrapRecordCases()
         .WithAllowOverride()
    //     // .WithUnionUnwrapFieldlessTags()
    //     //.WithUnwrapOption()
    //     //.WithUnionInternalTag()
    //     //.WithUnionTagName("type")        
         .AddToJsonSerializerOptions(o)        
    o

let sess_orig = """
{
  "value": "ek_69372e1657ec819195466cb4d74db6e2",
  "expires_at": 1765224558,
  "session": {
    "type": "realtime",
    "object": "realtime.session",
    "id": "sess_Ckbg6ZXqxqfvhD3OV2Ykb",
    "model": "gpt-realtime",
    "output_modalities": [
      "audio"
    ],
    "instructions": "You are a helpful AI assistant",
    "tools": [],
    "tool_choice": "auto",
    "max_output_tokens": "inf",
    "tracing": null,
    "truncation": "auto",
    "prompt": null,
    "expires_at": 0,
    "audio": {
      "input": {
        "format": {
          "type": "audio/pcm",
          "rate": 24000
        },
        "transcription": null,
        "noise_reduction": null,
        "turn_detection": {
          "type": "server_vad",
          "threshold": 0.5,
          "prefix_padding_ms": 300,
          "silence_duration_ms": 200,
          "idle_timeout_ms": null,
          "create_response": true,
          "interrupt_response": true
        }
      },
      "output": {
        "format": {
          "type": "audio/pcm",
          "rate": 24000
        },
        "voice": "alloy",
        "speed": 1.0
      }
    },
    "include": []
  }
}
"""

let sess = """
{
  "value": "ek_69372e1657ec819195466cb4d74db6e2",
  "expires_at": 1765224558,
  "session": {
    "type": "realtime",
    "object": "realtime.session",
    "id": "sess_Ckbg6ZXqxqfvhD3OV2Ykb",
    "model": "gpt-realtime",
    "output_modalities": [
      "audio"
    ],
    "max_output_tokens": "inf",
    "instructions": "You are a helpful AI assistant",
    "tools": [],
    "tool_choice": "auto",
    "tracing": null,
    "truncation": "auto",
    "prompt": null,
    "expires_at": 0,
    "audio": {
      "input": {
        "format": {
          "type": "audio/pcm",
          "rate": 24000
        },
        "transcription": null,
        "noise_reduction": null,
        "turn_detection": {
          "type": "server_vad",
          "threshold": 0.5,
          "prefix_padding_ms": 300,
          "silence_duration_ms": 200,
          "idle_timeout_ms": null,
          "create_response": true,
          "interrupt_response": true
        }
      },
      "output": {
        "format": {
          "type": "audio/pcm",
          "rate": 24000
        },
        "voice": "alloy",
        "speed": 1.0
      }
    },
    "include": []
  }
}
"""


(*
let js = JsonSerializer.Deserialize<KeyResp>(sess)


let serOpts =
    let opts =
        JsonFSharpOptions.Default()
            // .WithUnionInternalTag()
            // .WithUnionTagName("type")
            .WithUnionUnwrapRecordCases()
            // .WithUnionTagCaseInsensitive()
            // .WithAllowNullFields()
            .WithAllowOverride()
            // .WithUnionUnwrapFieldlessTags()
            .ToJsonSerializerOptions()
    opts.WriteIndented <- true
    opts
    
[<JsonFSharpConverter(
    BaseUnionEncoding = JsonUnionEncoding.InternalTag,
    UnionTagName = "type",
    UnionUnwrapRecordCases = true
)>]
[<RequireQualifiedAccess>]
type AudioFormat =
    | [<JsonName("audio/pcm")>] PCM of {|rate:int|} //24000
    | [<JsonName("audio/pcmu")>] PCMU
    | [<JsonName("audio/pcma")>] PCMA 
   
let af = AudioFormat.PCM {|rate=100|}
let af2 = AudioFormat.PCMU
let afstr = JsonSerializer.Serialize(af)
*)

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

let supdev = 
         {SessionUpdateEvent.Default with
                        event_id = "xy"
                        session =
                            {Session.Default with 
                                audio = Include Audio.Default
                            }
                    }
let supdevstr = JsonSerializer.Serialize(Audio.Default,serOpts)
supdevstr |> printfn "%s"

