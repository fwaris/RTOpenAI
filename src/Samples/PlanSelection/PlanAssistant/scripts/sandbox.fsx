#load "packages.fsx"
#load "../../../../RTOpenAI.Api/Api/Events.fs"

open System.Text.Json
open System.Text.Json.Serialization

let data = """{"type":"session.created","event_id":"event_CkBNY5q68b1BqrxVOorzK","session":{"object":"realtime.session","id":"sess_CkBNY4Df98S3UR04YBREU","model":"gpt-realtime","modalities":["audio","text"],"instructions":"You are a friendly assistant","voice":"alloy","output_audio_format":"pcm16","tools":[],"tool_choice":"auto","temperature":0.8,"max_response_output_tokens":"inf","turn_detection":{"type":"server_vad","threshold":0.5,"prefix_padding_ms":300,"silence_duration_ms":200,"idle_timeout_ms":null,"create_response":true,"interrupt_response":true},"speed":1.0,"tracing":null,"truncation":"auto","prompt":null,"expires_at":1765126464,"input_audio_noise_reduction":null,"input_audio_format":"pcm16","input_audio_transcription":null,"client_secret":null,"include":null}}"""
let dsess = """{"object":"realtime.session","id":"sess_CkBNY4Df98S3UR04YBREU","model":"gpt-realtime","modalities":["audio","text"],"instructions":"You are a friendly assistant","voice":"alloy","output_audio_format":"pcm16","tools":[],"tool_choice":"auto","temperature":0.8,"max_response_output_tokens":"inf","turn_detection":{"type":"server_vad","threshold":0.5,"prefix_padding_ms":300,"silence_duration_ms":200,"idle_timeout_ms":null,"create_response":true,"interrupt_response":true},"speed":1.0,"tracing":null,"truncation":"auto","prompt":null,"expires_at":1765126464,"input_audio_noise_reduction":null,"input_audio_format":"pcm16","input_audio_transcription":null,"client_secret":null,"include":null}"""


let serOptionsFSharp = 
    let o = JsonSerializerOptions(JsonSerializerDefaults.General)
    // o.Converters.Add(ContentTypeConverter())
    // o.Converters.Add(ConversationItemTypeConverter())
    //o.WriteIndented <- true
    //o.ReadCommentHandling <- JsonCommentHandling.Skip        
    let opts = JsonFSharpOptions.Default()
    opts
        //.WithSkippableOptionFields(true)
        //.WithAllowNullFields()
        //.WithUnionUnwrapRecordCases()
        // .WithAllowOverride()
        // .WithUnionUnwrapFieldlessTags()
        //.WithUnwrapOption()
        .AddToJsonSerializerOptions(o)        
    o

    
let j2 = JsonSerializer.Deserialize<RTOpenAI.Api.Events.SessionCreatedEvent>(data,serOptionsFSharp)
let j3 = JsonSerializer.Deserialize<RTOpenAI.Api.Events.Session>(dsess,serOptionsFSharp)


    