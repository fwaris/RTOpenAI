namespace Anthropic

module Client = 
    open Anthropic.SDK        // Anthropic Claude SDK
    open Microsoft.Extensions.AI   // For IChatClient, ChatOptions, etc.
    open System
    open System.Collections.Generic
    open Anthropic.SDK.Messaging

    let mutable ApiKeyProvider = lazy(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"))
    
    let toList (xs: AIContent list) : IList<AIContent> = List(xs)

    let createAnthropicClient(key:string) =
        let httpClient = new System.Net.Http.HttpClient()
        httpClient.DefaultRequestHeaders.Add("anthropic-beta","computer-use-2025-01-24")
        httpClient.DefaultRequestHeaders.Add("anthropic-beta","structured-outputs-2025-11-13")
        new AnthropicClient(key,httpClient)
        
    let createClientWithKey(key) : IChatClient =  
        (createAnthropicClient(key))
            .Messages
            .AsBuilder()
            //.UseFunctionInvocation()
            .Build()     
    let createClient() : IChatClient =createClientWithKey(ApiKeyProvider.Value)
        
    let thinking = lazy(
        let tp = ThinkingParameters()
        tp.BudgetTokens <- 2048
        tp
    )

    let toSchema(t:Type) = AIJsonUtilities.CreateJsonSchema(t)
