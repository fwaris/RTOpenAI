namespace RT.Assistant
open Microsoft.Extensions.AI   // For IChatClient, ChatOptions, etc.
open System
open System.Collections.Generic
#nowarn "57"


module AnthropicClient = 
    open Anthropic.SDK
    open Anthropic.SDK.Messaging
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
            
    let createClient() : IChatClient =createClientWithKey(Settings.Values.anthropicKey())
        
    let thinking = lazy(
        let tp = ThinkingParameters()
        tp.BudgetTokens <- 2048
        tp
    )
    
    let toSchema(t:Type) = AIJsonUtilities.CreateJsonSchema(t)

module OpenAIClient =
    let toList (xs: AIContent list) : IList<AIContent> = List(xs)
                
    let createClientWithKey(key:string) : IChatClient =
        let oaiClient = OpenAI.OpenAIClient(key)
        let respClient = oaiClient.GetResponsesClient("gpt-5.1-codex")
        respClient.AsIChatClient() 
 
    let createClient() = createClientWithKey(Settings.Values.openaiKey())

