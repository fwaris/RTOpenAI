(*
Load requires packages code files for running 'assistant' related F# scripts
*)



#r "nuget: Microsoft.DeepDev.TokenizerLib"
#r "nuget: Azure.Search.Documents"
#r "nuget: Microsoft.SemanticKernel"
#r "nuget: Microsoft.SemanticKernel.Connectors.Google, *-*"
#r "nuget: Lost.SemanticKernel.Connectors.Anthropic, *-*"
#r "nuget: FSharp.SystemTextJson"
#r "nuget: FSharp.Data"
#r "nuget: FSharp.Collections.ParallelSeq"
#r "nuget: FSharp.Control.AsyncSeq"
#r "nuget: FsPickler.Json"
#r "nuget: F23.StringSimilarity"
#r "nuget: FsExcel"
#r "nuget: ExcelProvider"
#r "nuget: Plotly.NET"

#r @"..\..\SwiPlcsCore\bin\Debug\net9.0\SwiPlcsCore.dll"


//transient packages. update to remove vulnerability warnings
#r "nuget: Newtonsoft.Json"
#r "nuget: System.Text.RegularExpressions"
#r "nuget: System.Private.Uri"

#load "../Utils.fs"
#load "../Plan/AsyncExt.fs" 
#load "../Plan/AICore.fs"
#load "../Plan/PlanPrompts.fs"

open System
open System.Threading
open System.Diagnostics
open System.IO
open System.Text.Json
open Microsoft.DeepDev
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.ChatCompletion
open Microsoft.SemanticKernel.Connectors.OpenAI
open RT.Assistant
open SbsSW.SwiPlCs

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
  pi.UseShellExecute <- false
  pi.RedirectStandardError <- true
  pi.RedirectStandardOutput <- true
  use pf = Process.Start(pi)
  pf.WaitForExit()
  pf.StandardOutput.ReadToEnd() + "\n" + pf.StandardError.ReadToEnd()
  
let tokenSize (s:string) =
    let tokenizer = TokenizerBuilder.CreateByModelNameAsync("gpt-4").GetAwaiter().GetResult();
    let tokens = tokenizer.Encode(s, new System.Collections.Generic.HashSet<string>());
    float tokens.Count

type TestCase = {
    Index : int
    Query : string
    Answer : string
    AnswerPredicates: string
    AnswerQuery : string    
}

type EvalCodeResp = NotEvaluated | CodeError of string | EvalOutput of string  //result of code evaluation
type EvaluatedCode = {Code:RT.Assistant.Plan.CodeGenResp; Resp:EvalCodeResp}      //pair generated code with its eval result

type TestAttempt = Direct of string | CodeGen of EvaluatedCode     //test attempt answer from LLM 'direct' or via coden gen

type TokenUsage =
    {
        InputTokens : float
        OutputTokens : float
        TotalTokens : float
    }

//answer attempt with stats 
type TestAttemptWithStats  = {
    Attempt : TestAttempt
    Usage : TokenUsage
    Time : TimeSpan
}

type Judgement = {
    Pass : bool       
    Comments : string 
}

//test case paired with attempted answer and its evaluation
type TestResult =
    {
        TestCase : TestCase
        Run : int
        Model : string
        Attempt : TestAttemptWithStats
        Judgement : Judgement option
    }

let saveTestResults (path:string) (testResults:TestResult list) =
    use str = File.Create path
    let opts = Utils.serOptionsFSharp
    JsonSerializer.Serialize(str,testResults,options=opts)
    
let loadTestResults (path:string) : TestResult list =
    let opts = Utils.serOptionsFSharp
    use str = File.OpenRead path
    JsonSerializer.Deserialize(str,options=opts)

   
let evalPrologLocal (file:string) (query:string) = 
    if not (PlEngine.IsInitialized) then  PlEngine.Initialize([|"-q";"--no-signals";"--traditional"|])    
    printfn $"running: {query}"    
    //let mple1 = new PlMtEngine()
    //mple1.PlSetEngine()
    let mutable q = Unchecked.defaultof<_>
    try 
        try 
            let r = PlQuery.PlCall($"consult('{file}').")
            if not r then failwith $"unable to compile prediciate"
            q  <- new PlQuery(query)        
            if q.VariableNames.Count = 0 then failwith "no variables in query"        
            let rslt = 
                q.ToList()
                |> Seq.indexed
                |> Seq.map(fun (i,vars) ->
                    let varVals =
                        q.VariableNames 
                        |> Seq.map(fun v ->v, vars.[v].ToString())
                        |> Seq.filter(fun (v,vs) -> vs <> "_")
                        |> Seq.map (fun (v,vs) -> $"{v}={vs}") 
                        |> String.concat "; "
                    $"{i}: {varVals}"
                )
                |> String.concat "\n"        
            printfn "%s" rslt
            rslt
        with ex -> 
            printfn "%s" ex.Message
            raise ex
    finally                
        if q <> Unchecked.defaultof<_> then q.Dispose()        
        //mple1.PlDetachEngine()
        //mple1.Dispose()
        PlEngine.PlCleanup()
        ()

type Input = {
    File : string
    Query : string
}

let MAX_CODE_EVAL_TIME_MS = 5000

let evalPrologExternal (file:string) (query:string) = 
    async {
        try
            printfn  $"running: {query}"
            let workingDir = Path.GetDirectoryName(file)
            let jsonFile = file + ".json"
            let input = {File = file; Query=query}
            let jsonStr = JsonSerializer.Serialize(input)
            File.WriteAllText(jsonFile,jsonStr)
            let pi = ProcessStartInfo()
            pi.FileName <- "E:/s/rtapi/evalP/EvalProlog.exe"
            pi.WorkingDirectory <- @"E:/s/rtapi/evalP/"
            pi.Arguments <- $""" "{jsonFile}" """
            pi.UseShellExecute <- false
            pi.RedirectStandardError <- true
            pi.RedirectStandardOutput <- true
            let cts = new CancellationTokenSource()
            cts.Token.ThrowIfCancellationRequested()
            let p = new Process(StartInfo = pi)
            let runProcess() = 
                async {
                    try
                        cts.CancelAfter(MAX_CODE_EVAL_TIME_MS)       //start timer                            
                        do! p.WaitForExitAsync(cts.Token) |> Async.AwaitTask
                        try p.Kill(true) with _ -> ()
                        return 0
                    with
                    | ex ->
                        
                        try p.Kill(true) with _ -> ()
                        if cts.IsCancellationRequested then
                            return 1 //timeout
                        else 
                            return 2 //other error 
                }
            let getOutput() = 
                async {
                    let! str = p.StandardOutput.ReadToEndAsync() |> Async.AwaitTask
                    let! str2 = p.StandardError.ReadToEndAsync() |> Async.AwaitTask
                    return str + "\n" + str2
                }
            p.Start() |> ignore
            let! job1 = Async.StartChild (runProcess())
            let! job2 = Async.StartChild (getOutput())
            let! r = job1
            let! out = job2
            let out = if Utils.isEmpty out then "Unknown error" else out
            printfn "%s" out
            match r with 
            | 0 -> return out
            | 1 -> return failwith "prolog eval timed out"
            | 2 -> return failwith out
            | _ -> return failwith "fsiEvalCode: return case not handled"
        with ex -> 
            printfn "%s" ex.Message
            return raise ex
    }

let testEval() = 
    let file = @"C:\Users\Faisa\eval\eval_1_0.pl"
    let query = "plan(T,_,_,_)."
    let ans = evalPrologExternal file query |> runA
    ans 


let private evalAgent = MailboxProcessor.Start (fun inbox -> 
    async {
        while true do          
            let! (file,query,rc:AsyncReplyChannel<string>) = inbox.Receive()
            try
                let! rslt = evalPrologExternal file query
                rc.Reply(rslt)
            with ex ->
                rc.Reply(ex.Message)

    })

let evalPrologAsync file query = async {
    try 
        let! rs = evalAgent.PostAndAsyncReply((fun rc -> file,query,rc), timeout=5000)
        return rs
    with ex ->
       return raise ex
}
