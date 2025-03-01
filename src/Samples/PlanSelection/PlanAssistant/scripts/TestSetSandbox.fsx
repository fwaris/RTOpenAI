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

let folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/eval/sandbox"
Packages.ensureDir folder

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
    code
    
let evalCode (code:CodeGenResp) =
    let scriptFile = folder + $"/eval.pl"
    let outfile = $"outfile.txt"
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


let code = genCode parmsO3Mini "List the plans which offer the most highspeed hotspot data" 
let rslt = evalCode code
printfn $"{code.Predicates}"
printfn $"{code.Query}"
match rslt with EvalOutput c -> printfn $"{c}" | CodeError s -> printfn $"{s}"
