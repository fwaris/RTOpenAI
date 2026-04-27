namespace RTOpenAI.Api

open System
open FSharp.DI

type RTOpenAILog() = class end

module Log =
    let mutable debug_logging = false
    let private log = DI.loggerLazy<RTOpenAILog>()
    let info (msg:string) = log.Value.info msg
    let warn (msg:string) = log.Value.warn msg
    let error (msg:string) = log.Value.error msg
    let exn (exn:exn,msg) = log.Value.exn (exn,msg)

    let init (sp:IServiceProvider) =
        DI.init sp
        info "Initialized"

module rec Env =
    let private fromEnvironmentOrDefault name fallback =
        lazy (
            match Environment.GetEnvironmentVariable name with
            | null -> fallback
            | value when String.IsNullOrWhiteSpace value -> fallback
            | value -> value
        )

    let OPENAI_RT_API_CALLS =
        fromEnvironmentOrDefault (nameof OPENAI_RT_API_CALLS) "https://api.openai.com/v1/realtime/calls"

    let OPENAI_RT_API =
        fromEnvironmentOrDefault (nameof OPENAI_RT_API) "https://api.openai.com/v1/realtime"

    let OPENAI_SESSION_API =
        fromEnvironmentOrDefault (nameof OPENAI_SESSION_API) "https://api.openai.com/v1/realtime/client_secrets"

    let OPENAI_RT_MODEL_GPT_REALTIME =
        fromEnvironmentOrDefault (nameof OPENAI_RT_MODEL_GPT_REALTIME) "gpt-realtime"

    let OPENAI_RT_MODEL_GPT_REALTIME_MINI =
        fromEnvironmentOrDefault (nameof OPENAI_RT_MODEL_GPT_REALTIME_MINI) "gpt-realtime-mini"

    let OPENAI_RT_DATA_CHANNEL =
        fromEnvironmentOrDefault (nameof OPENAI_RT_DATA_CHANNEL) "oai-events"
