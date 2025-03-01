#load "packages.fsx"
open Packages
open System.Diagnostics
open System.Text.Json
open RT.Assistant
open RT.Assistant.Plan
open System
open System.IO
open FSharp.Control
open FSharp.Interop.Excel
open AsyncExts

let parmsGpt40 = Packages.parms AICore.models.gpt_4o
let parmsO3Mini = Packages.parms AICore.models.o3_mini
let parmsGemini = {Packages.parms AICore.models.gemini_think with Key = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")}
let parmsClaude = {Packages.parms AICore.models.sonnet_37 with Key = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")}
 
(*
conduct accuracy experiments
*)

let runA = Packages.runA

let folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/eval"
Packages.ensureDir folder

let [<Literal>] TestSetFile = __SOURCE_DIRECTORY__ + "/TestSet.xlsx"
type T_TestSet = ExcelFile<TestSetFile>
let TestOutput = __SOURCE_DIRECTORY__ + "/TestOutput.xlsx"
let clauses = lazy(File.ReadAllText(__SOURCE_DIRECTORY__ + "/../Resources/Raw/wwwroot/plan_clauses.pl"))
let plans_json = lazy(File.ReadAllText(__SOURCE_DIRECTORY__ + "/PLANS.json"))

let metaQuery (outfile:string) = $"""
write_query_output(Query) :-
    open('{outfile}', append, Stream),
    current_output(Old),
    set_output(Stream),
    %% For each solution, decompose the term and write just the arguments
    forall(
        (call(Query), Query =.. [_|Args]),
        writeln(Args)
    ),
    set_output(Old),
    close(Stream).

delete_if_exists(File) :-
    catch(delete_file(File),
          error(existence_error(file, File), _),
          writeln('File does not exist or cannot be deleted.')).
"""

let outTyp= typeof<CodeGenResp>

let genCode parms prompt =
    let w = Stopwatch()
    w.Start()
    let resp = AICore.getOutputWithStats parms PlanPrompts.sysMsg.Value prompt outTyp |> runA
    w.Stop()
    let cntnt = resp.Content
    printfn "%s" cntnt
    let json = AICore.extractCode cntnt
    let code = JsonSerializer.Deserialize<CodeGenResp>(json)
    let usage =
        match resp.Metadata.TryGetValue("Usage") with
        | true,v ->
            let u = v :?> OpenAI.Chat.ChatTokenUsage
            {
                InputTokens = u.InputTokenCount
                OutputTokens = u.OutputTokenCount
                TotalTokens = u.TotalTokenCount
            }
        | _ ->
            let tInp = Packages.tokenSize (PlanPrompts.sysMsg.Value + prompt)
            let tOut = Packages.tokenSize cntnt
            {
                InputTokens =  tInp
                OutputTokens = tOut
                TotalTokens = (tInp + tOut)
            }
    let time = w.Elapsed
    let attempt = CodeGen {Code=code; Resp=NotEvaluated}
    {
        Attempt = attempt
        Usage = usage
        Time = time
    }
    
let evalCode (id:string) (code:CodeGenResp) =
    let scriptFile = folder + $"/eval_{id}.pl"
    let outfile = $"outfile_{id}.txt"
    let goal = $"""
goal :-
    delete_if_exists('{outfile}'),
    write_query_output(({AICore.removeDot code.Query})).
"""
    let cls =
        clauses.Value + "\n"
        + metaQuery(outfile) + "\n"
        + code.Predicates + "\n\n"
        + goal
    File.WriteAllText(scriptFile,cls)
    let args = $""" -f {scriptFile} -g goal -t halt"""
    let rslt = Packages.runSwipl folder args
    if rslt.Contains("Error:") then
        CodeError rslt
    else
        let file = Path.Combine(folder,outfile)
        if File.Exists(file) then
            EvalOutput (File.ReadAllText(file))
        else
            CodeError "no output produced"
    
let runTestCodeGen parms (run,id:string,test:TestCase) =
    async {
        let stats = genCode parms test.Query
        match stats.Attempt with
        | CodeGen c ->
            let resp = evalCode id c.Code
            let attempt = {stats with Attempt = CodeGen {c with Resp=resp}}
            return 
                {
                    TestCase = test
                    Attempt = attempt
                    Model = parms.Model
                    Run = run
                    Judgement = None
                }
        | _ -> return failwith "not expected" 
    }   

let runTestSetCodeGen tpm parms tests run  =
    tests
    |> Seq.map (fun t -> run,$"{run}_{t.Index}",t)     
    |> AsyncSeq.ofSeq
    |> AsyncSeq.map (fun (a,b,c) -> int64 (tokenSize (Plan.PlanPrompts.sysMsg.Value + c.Query)) ,(a,b,c))
    |> AsyncSeq.mapAsyncParallelTokenLimit tpm (runTestCodeGen parms)
    |> AsyncSeq.toBlockingSeq
    |> Seq.toList    
        
let directQueryPrompt = lazy $"""
Your goal is to help a customer find a suitable phone plan given a customer's needs.
The information about the available plans exists as set of JSON records given under PLANS

PLANS```
{plans_json.Value}
```
Respond to the customer query from the information under PLANS. 
"""
    
let runTestDirect parms (run,id:string,test:TestCase) =
    async {
        let sw = Stopwatch()
        sw.Start()
        let! resp = AICore.runModel parms directQueryPrompt.Value test.Query
        sw.Stop()
        
        let usage =
            match resp.Metadata.TryGetValue("Usage") with
            | true,v ->
                let u = v :?> OpenAI.Chat.ChatTokenUsage
                {
                    InputTokens = u.InputTokenCount
                    OutputTokens = u.OutputTokenCount
                    TotalTokens = u.TotalTokenCount
                }
            | _ ->
                let tInp = Packages.tokenSize (directQueryPrompt.Value + test.Query)
                let tOut = Packages.tokenSize resp.Content
                {
                    InputTokens =  tInp
                    OutputTokens = tOut
                    TotalTokens = (tInp + tOut)
                }
          
        let resp = Direct resp.Content
        
        let attempt =
            {
                Attempt = resp
                Usage = usage
                Time = sw.Elapsed
            }
            
        return 
            {
                TestCase = test
                Attempt = attempt
                Model = parms.Model
                Run = run
                Judgement = None
            }
    }

let runTestSetDirect tpm parms tests run  =
    tests
    |> Seq.map (fun t -> run,$"{run}_{t.Index}",t)     
    |> AsyncSeq.ofSeq
    |> AsyncSeq.map (fun (a,b,c) -> int64 (tokenSize (directQueryPrompt.Value + c.Query)) ,(a,b,c))
    |> AsyncSeq.mapAsyncParallelTokenLimit tpm (runTestDirect parms)
    |> AsyncSeq.toBlockingSeq
    |> Seq.toList
    
let testSet =
    T_TestSet().Data
    |> Seq.indexed
    |> Seq.map(fun (i,r) ->
            {
                Index = i
                Query = r.Question
                Answer = r.Answer
                AnswerPredicates = string r.Predicates
                AnswerQuery = r.Query
            }
        )
    |> Seq.toList
 

let outfolder = folder @@ "tests"
ensureDir outfolder



(*
let testEvalsGpt4 = [1 .. 2] |> List.map (runTestSetCodeGen 500000 parmsGpt40 testSet) |> List.collect id
testEvalsGpt4 |> saveTestResults (outfolder @@ "gpt4o_code.json")
let testEvalsGpt4D = [1 .. 2] |> List.map (runTestSetDirect 500000 parmsGpt40 testSet) |> List.collect id
testEvalsGpt4D |> saveTestResults (outfolder @@ "gpt4o_direct.json")

let testEvalsO3mini = [1 .. 2] |> List.map (runTestSetCodeGen 500000 parmsO3Mini testSet) |> List.collect id
testEvalsO3mini |> saveTestResults (outfolder @@ "o3mini_code.json")
let testEvalsO3miniD = [1 .. 2] |> List.map (runTestSetDirect 500000 parmsO3Mini testSet) |> List.collect id
testEvalsO3miniD |> saveTestResults (outfolder @@ "o3mini_direct.json")


let testEvalsGemini = [1 .. 2] |> List.map (runTestSetCodeGen 500000 parmsGemini testSet) |> List.collect id
testEvalsGemini |>  saveTestResults (outfolder @@ "gemini_flash_code.json")
let testEvalsGeminiD = [1 .. 2] |> List.map (runTestSetDirect 100000 parmsGemini testSet) |> List.collect id
testEvalsGeminiD |> saveTestResults (outfolder @@ "gemini_flash_direct.json")


let testEvalsSonnet37D = [1 .. 2] |> List.map (runTestSetDirect 2000 parmsClaude testSet) |> List.collect id
testEvalsSonnet37D |> saveTestResults (outfolder @@ "sonnet_37_direct.json")
let testEvalsSonnet37 = [1 .. 1] |> List.map (runTestSetCodeGen 2000 parmsClaude testSet) |> List.collect id
testEvalsSonnet37 |> saveTestResults (outfolder @@ "sonnet_37_code.json")
*)
