(*
Load requires packages code files for running 'assistant' related F# scripts
*)

#r "nuget: Microsoft.DeepDev.TokenizerLib"
#r "nuget: Azure.Search.Documents"
#r "nuget: Microsoft.SemanticKernel"
#r "nuget: Microsoft.SemanticKernel.Connectors.Google, 1.34.0-alpha"
#r "nuget: FSharp.SystemTextJson"
#r "nuget: FSharp.Data"
#r "nuget: FSharp.Collections.ParallelSeq"
#r "nuget: FSharp.Control.AsyncSeq"
#r "nuget: FsPickler.Json"
#r "nuget: F23.StringSimilarity"

//transient packages. update to remove vulnerability warnings
#r "nuget: Newtonsoft.Json"
#r "nuget: System.Text.RegularExpressions"
#r "nuget: System.Private.Uri"

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.ChatCompletion
open Microsoft.SemanticKernel.Connectors.OpenAI

let serOptionsFSharp = 
    let o = JsonSerializerOptions(JsonSerializerDefaults.General)        
    o.WriteIndented <- true
    o.ReadCommentHandling <- JsonCommentHandling.Skip        
    let opts = JsonFSharpOptions.Default()
    opts
        .WithSkippableOptionFields(true)            
        .AddToJsonSerializerOptions(o)        
    o

let lines includeEmptyLines (s:string) =
    seq{
        use sr = new StringReader(s)
        let mutable line = sr.ReadLine()
        while line <> null do
            yield line
            line <- sr.ReadLine()
    }
    |> Seq.filter (fun s -> if includeEmptyLines then true else s |> String.IsNullOrWhiteSpace |> not)
    |> Seq.toList
    
let extractTripleQuoted (inp:string) =
    let lines = lines false inp
    let addSnip acc accSnip =
        match accSnip with
        |[] -> acc
        | _ -> (List.rev accSnip)::acc
    let isQuote (s:string) = s.StartsWith("```")
    let rec start acc (xs:string list) =
        match xs with
        | []                   -> List.rev acc
        | x::xs when isQuote x -> accQuoted acc [] xs
        | x::xs                -> start acc xs
    and accQuoted acc accSnip xs =
        match xs with
        | []                   -> List.rev (addSnip acc accSnip)
        | x::xs when isQuote x -> start (addSnip acc accSnip) xs
        | x::xs                -> accQuoted acc (x::accSnip) xs
    start [] lines

let ensureEndDot (s:string) = let s = s.Trim() in if s.EndsWith('.') then s else s + "."

let extractCode (inp:string) =
    let rawCode =
        if inp.Contains("```prolog") then 
            extractTripleQuoted inp
            |> Seq.collect id
            |> fun xs -> String.Join("\n",xs)
        else
            inp
    rawCode.Replace("?-","") //cprolog does not support ?- op
    |> ensureEndDot

let removeDot (inp:string) = let s = inp.Trim() in if s.EndsWith('.') then s.TrimEnd('.') else s
                       
let runModel (model:string) (sysMsg:string) (prompt:string) =
    let ch = ChatHistory(sysMsg)
    ch.AddUserMessage(prompt)
    let svc = OpenAIChatCompletionService(model,Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    //let opts = OpenAIPromptExecutionSettings()
    //opts.Temperature <- 0.2
    let rslt = svc.GetChatMessageContentsAsync(ch).Result
    rslt.[0].Content
    
let runO1Mini (sysMsg:string) (prompt:string) =
    let msg = $"{sysMsg}\n\n{prompt}"
    let svc = OpenAIChatCompletionService("o1-mini",Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    //let opts = OpenAIPromptExecutionSettings()
    //opts.Temperature <- 0.2
    let rslt = svc.GetChatMessageContentAsync(msg)
    rslt.Result.Content
    
let runR1 (sysMsg:string) (prompt:string) =
    let msg = $"{sysMsg}\n\n{prompt}"
    let svc = OpenAIChatCompletionService("deepseek-reasoner", Uri "https://api.deepseek.com",Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY"))
    //let opts = OpenAIPromptExecutionSettings()
    //opts.Temperature <- 0.2
    let rslt = svc.GetChatMessageContentAsync(msg)
    rslt.Result.Content
    
let runO1 = runModel "o1"
//let runO1Mini = runModel "o1-mini"
let run4o = runModel "gpt-4o"
let run4oMini = runModel "gpt-4o-mini"
let run03Mini = runModel "o3-mini"
let run03MiniHi = runModel "o3-mini-high"

let runGemini (sysMsg:string) (prompt:string) =
    let ch = ChatHistory(sysMsg)
    ch.AddUserMessage(prompt)
    let key = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
    let k = Kernel.CreateBuilder().AddGoogleAIGeminiChatCompletion("gemini-2.0-flash-thinking-exp",key).Build()
    let svc = k.GetRequiredService<IChatCompletionService>()
    let rslt = svc.GetChatMessageContentAsync(ch)
    rslt.Result
    // rslt.Result.ModelId
    // rslt.Result.Content
    
(*
let rslt = runGemini "you are an ai asst." "what is the meaning of life"
*)
    
let skipLineComment (s:string) = let i = s.IndexOf('%') in if i >=0 then s.Substring(0,i) else s

let stripComments (s:string) =
    let rec loop (acc:char list) inComment xs =
        match xs with
        | [] -> acc |> List.rev |> Seq.toArray |> String
        | ('/','*')::ys when not inComment -> loop acc true ys
        | ('*','/')::_::ys when inComment -> loop acc false ys
        | [('*','/');(_,b)] when inComment -> loop (b::acc) false []
        | _::ys when inComment -> loop acc inComment ys
        | [(a,b)] when not inComment -> loop (b::a::acc) inComment []
        | (c,_)::ys -> loop (c::acc) inComment ys
    let xs = s |> Seq.pairwise |> Seq.toList
    loop [] false xs

