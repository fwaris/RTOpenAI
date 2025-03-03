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

let clausesFile_ = __SOURCE_DIRECTORY__ + "/../Resources/Raw/wwwroot/plan_clauses.pl"
let clausesFile = Path.GetFullPath(clausesFile_).Replace("\\","/") //forward slashes needed
printfn "%s" clausesFile
let clauses = lazy(File.ReadAllText clausesFile)

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

open SbsSW.SwiPlCs

PlEngine.Initialize([|"-q"|])

let evalProlog (file:string) (query:string) = 
    try 
        let r = PlQuery.PlCall($"consult('{file}').")
        if not r then failwith $"unable to compile prediciate"
        use q = new PlQuery(query)        
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
    finally                
        ()

let addPredicates (predicates:string) = 
    clauses.Value + "\n\n" + predicates

    
let evalCode (code:CodeGenResp) =
    try 
        let id = "sb"
        let scriptFile = Path.GetFullPath(folder + $"/eval_{id}.pl").Replace("\\","/")
        let outfile = $"outfile_{id}.txt"
        let text = addPredicates code.Predicates
        File.WriteAllText(scriptFile,text)
        let rslt = evalProlog scriptFile code.Query
        EvalOutput rslt
    with ex -> 
        CodeError ex.Message

let testRun() =
    let code = genCode parmsO3Mini "I am over 55 and am looking for the cheapest plan where netflix is included. Give the price and the number of lines"
    let rslt = evalCode code
    printfn $"{code.Predicates}"
    printfn $"{code.Query}"
    match rslt with EvalOutput c -> printfn $"{c}" | CodeError s -> printfn $"Error: {s}"

(*
let testCode = 
    {
      Predicates = ""
      Query = "setof(Category, Title^Lines^Features^(plan(Title, category(Category), Lines, Features)), Categories)."
    }

let r = evalCode testCode

let fn = (Path.GetFullPath(folder @@ "eval_2.pl")).Replace("\\","/")
File.WriteAllText(fn,addPredicates testCode.Predicates)
PlQuery.PlCall($"consult('{fn}').")
let q = PlQuery(testCode.Query)
let sol = q.ToList()
sol.Count
let sol0 = sol.[0]
let vs = q.VariableNames |> Seq.map(fun x -> sol0.[x]) |> Seq.toList
vs |> List.map _.PlType
let vsVals = vs |> List.map _.ToString()
t.ToString()
let vTerms = q.VariableNames |> Seq.toList |> List.map(fun n -> q.Variables.[n])
let vTs = vTerms |> List.map(fun x->x.ToStringCanonical())
let vars = [for i in 0 .. q.Args.Size-1 -> if q.Args[i].IsVar then Some i else None] |> List.choose id
let args = [for i in 0 .. q.Args.Size-1 -> q.Args.[i]]
let vars = args |> List.map _.IsVar
let comp = args |> List.map _.IsCompound
let vals = args |> List.map _.ToString()
let grnd = args |> List.map _.IsGround
let x1 = args |> List.map _.IsInitialized
let x2 = args |> List.map _.PlType
let arity = args |> List.map (fun x -> if x.IsCompound then x.Arity else 0)


let vn = q.VariableNames.[2]
q.NextSolution()
let n0 = q.VariableNames.[vars[0]]
let a0 = q.Args.[vars[0]]
a0.ToString()
$"{n0}={a0}"
let mutable c = 0
let ms = 
    seq {
        while q.NextSolution() do
            let vars =
                seq {
                    for i in 0 .. vars.Length - 1 do                                                       
                        let n = q.VariableNames.[i]
                        let arg = q.Args.[i]
                        yield $"{n}={arg.ToString()}"
                }
                |> Seq.toList
                |> String.concat "; "
            c <- c + 1
            $"{c}: {vars}"
    }
    |> Seq.toList
    |> String.concat "\n"        

q.NextSolution()
let args = [for i in 0 .. q.Args.Size-1 -> q.Args.[i]]
let vars = args |> List.map _.IsVar
let comp = args |> List.map _.IsCompound
let vals = args |> List.map _.ToString()
let arity = args |> List.map (fun x -> if x.IsCompound then x.Arity else 0)
let a00 = args.[2].[1].ToString()
let 
q.Args
let ss = q.SolutionVariables
ss |> Seq.length
q.Args.Size
[for i in 0 .. q.Args.Size-1 -> q.Args.[i].PlType]
q.Args.[0].IsCompound
let rtest = evalCode testCode
match rtest with EvalOutput x -> printfn "%s" x
let sol = q.Solutions
let x0 = Seq.head sol
x0.[0].ToString()
x0.[1].ToString()
x0.[2].ToString()
x0.[0]
let testCode0 = { 
    Predicates = """
veteran_plan(Title, TaxesIncluded) :-
    plan(Title, category(military_veteran), _Lines, features(Features)),
    member(feature(taxes_and_fees(_Desc, included_in_monthly_price(TaxesIncluded)), _Applies), Features).
% The query retrieves all veteran plans with the taxes and fees inclusion information
"""

    Query = "findall((Title,TaxesIncluded), veteran_plan(Title,TaxesIncluded), Plans)."
}

*)
