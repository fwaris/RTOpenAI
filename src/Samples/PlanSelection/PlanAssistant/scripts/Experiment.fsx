#load "packages.fsx"
open System.Diagnostics
open System.Text.Json
open System.Threading
open RT.Assistant.Plan
open System
open System.IO
open FSharp.Control
open FSharp.Interop.Excel
open AsyncExts

let parmsGpt40 = Packages.parms AICore.models.gpt_4o
let parmsO3Mini = Packages.parms AICore.models.o3_mini
let parmsGemini = {Packages.parms AICore.models.gemini_think with Key = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")}

type TestCase = {
    Index : int
    Query : string
    Answer : string
    AnswerPredicates: string
    AnswerQuery : string    
}

type EvalCodeResp = NotEvaluated | CodeError of string | EvalOutput of string  //result of code evaluation
type EvaluatedCode = {Code:CodeGenResp; Resp:EvalCodeResp}      //pair generated code with its eval result

type TestAttempt = Direct of string | CodeGen of EvaluatedCode     //test attempt answer from LLM 'direct' or via coden gen 

//answer attempt with stats 
type TestAttemptWithStats  = {
    Attempt : TestAttempt
    Usage : OpenAI.Chat.ChatTokenUsage option
    Time : TimeSpan
}

//test case paired with attempted answer and its evaluation
type TesResult =
    {
        TestCase : TestCase
        Run : int
        Model : string
        Attempt : TestAttemptWithStats
        Pass : bool                     //attempt was success, if true 
    }

(*
conduct accuracy experiments
*)

let runA = Packages.runA

let folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/eval"
Packages.ensureDir folder

let [<Literal>] TestSet = __SOURCE_DIRECTORY__ + "/TestSet.xlsx"
type T_TestSet = ExcelFile<TestSet>
let TestOutput = __SOURCE_DIRECTORY__ + "/TestOutput.xlsx"
let clauses = lazy(File.ReadAllText(__SOURCE_DIRECTORY__ + "/../Resources/Raw/wwwroot/plan_clauses.pl"))

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
        | true,v -> v :?> OpenAI.Chat.ChatTokenUsage |> Some
        | _ -> None
    let time = w.Elapsed
    let attempt = CodeGen {Code=code; Resp=NotEvaluated}
    {
        Attempt = attempt
        Usage = usage
        Time = time
    }

//run swi-prolog process in batch mode with the given script
let runSwipl2 (workikngDir:string) (args:string) =
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
    let rslt = runSwipl2 folder args
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
                    Pass = false
                }
        | _ -> return failwith "not expected" 
    }
    
let runTestSetCodeGen parms tests run  =
    tests
    |> Seq.map (fun t -> run,$"{run}_{t.Index}",t)     
    |> AsyncSeq.ofSeq
    |> AsyncSeq.mapAsyncParallelThrottled 2 (runTestCodeGen parms)
    |> AsyncSeq.toBlockingSeq
    |> Seq.toList
    
let testSet =
    T_TestSet().Data
    |> Seq.indexed
    |> Seq.map(fun (i,r) ->
            {
                Index = i
                Query = r.Query
                Answer = r.Answer
                AnswerPredicates = string r.Predicates
                AnswerQuery = r.Query
            }
        )
    |> Seq.toList
    
(*
let testEvalsGpt4 = [1 .. 2] |> List.map (runTestSetCodeGen parmsGpt40 testSet)
let testEvalsGemini = [1 .. 2] |> List.map (runTestSetCodeGen parmsGemini testSet)
*)


