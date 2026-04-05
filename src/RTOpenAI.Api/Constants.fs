namespace RTOpenAI.Api

open System

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
