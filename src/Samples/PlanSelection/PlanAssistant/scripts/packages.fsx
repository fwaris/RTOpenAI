(*
Load requires packages code files for running 'assistant' related F# scripts
*)

#r "nuget: Microsoft.DeepDev.TokenizerLib"
#r "nuget: Azure.Search.Documents"
#r "nuget: Microsoft.SemanticKernel"
#r "nuget: Microsoft.SemanticKernel.Connectors.Google, 1.38.0-alpha"
#r "nuget: FSharp.SystemTextJson"
#r "nuget: FSharp.Data"
#r "nuget: FSharp.Collections.ParallelSeq"
#r "nuget: FSharp.Control.AsyncSeq"
#r "nuget: FsPickler.Json"
#r "nuget: F23.StringSimilarity"
#r "nuget: FsExcel"
#r "nuget: ExcelProvider"

//transient packages. update to remove vulnerability warnings
#r "nuget: Newtonsoft.Json"
#r "nuget: System.Text.RegularExpressions"
#r "nuget: System.Private.Uri"

#load "../Utils.fs"
#load "../Plan/AsyncExt.fs" 
#load "../Plan/AICore.fs"
#load "../Plan/PlanPrompts.fs"

open System
open System.Diagnostics
open System.IO
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.ChatCompletion
open Microsoft.SemanticKernel.Connectors.OpenAI

let runA = Async.RunSynchronously

let ensureDir (path:string) = if Directory.Exists path |> not then Directory.CreateDirectory path |> ignore

let runO1Mini (sysMsg:string) (prompt:string) =
    let msg = $"{sysMsg}\n\n{prompt}"
    let svc = OpenAIChatCompletionService("o1-mini",Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    //let opts = OpenAIPromptExecutionSettings()
    //opts.Temperature <- 0.2
    let rslt = svc.GetChatMessageContentAsync(msg)
    rslt.Result.Content
        
let key = lazy(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
let parms model = {RT.Assistant.Plan.Model=model; RT.Assistant.Plan.Key=key.Value}
    
let runO1 = RT.Assistant.Plan.AICore.runModel (parms "o1")
//let runO1Mini = runModel "o1-mini"
let run4oa s p = RT.Assistant.Plan.AICore.runModel (parms "gpt-4o") s p |> runA
let run4oMini s p = RT.Assistant.Plan.AICore.runModel (parms "gpt-4o-mini") s p |> runA
let run03Mini  s p = RT.Assistant.Plan.AICore.runModel (parms "o3-mini") s p |> runA
let run03MiniHi s p = RT.Assistant.Plan.AICore.runModel (parms "o3-mini-high") s p |> runA

let runGemini (sysMsg:string) (prompt:string) =
    let ch = ChatHistory(sysMsg)
    ch.AddUserMessage(prompt)
    let key = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
    let k = Kernel.CreateBuilder().AddGoogleAIGeminiChatCompletion("gemini-2.0-flash-thinking-exp",key).Build()
    let svc = k.GetRequiredService<IChatCompletionService>()
    let rslt = svc.GetChatMessageContentAsync(ch)
    rslt.Result
    
let runR1 (sysMsg:string) (prompt:string) =
    let msg = $"{sysMsg}\n\n{prompt}"
    let svc = OpenAIChatCompletionService("deepseek-reasoner", Uri "https://api.deepseek.com",Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY"))
    //let opts = OpenAIPromptExecutionSettings()
    //opts.Temperature <- 0.2
    let rslt = svc.GetChatMessageContentAsync(msg)
    rslt.Result.Content

//run swi-prolog process in batch mode with the given script
let runSwipl (workikngDir:string) (args:string) =
  let pi = ProcessStartInfo()
  pi.FileName <- "swipl"
  pi.WorkingDirectory <- workikngDir
  pi.Arguments <- args
  use pf = Process.Start(pi)
  pf.WaitForExit()
  pf.StandardOutput.ReadToEnd()
  
