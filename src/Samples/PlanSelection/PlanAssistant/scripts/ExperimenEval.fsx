#load "packages.fsx"
open Packages
open System.Text.Json
open RT.Assistant.Plan
open System
open System.IO
open FSharp.Control
open AsyncExts
open Plotly.NET

(*
evaluate accuracy experiment results
*)

let parmsO3Mini = Packages.parms AICore.models.o3_mini

let runA = Packages.runA

let folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/eval/tests"

let testFiles = Directory.GetFiles(folder,"*.json")

let evalPrompt question groundTruth generatedAnswer = $"""
Compare the GROUND_TRUTH answer to the GENERATED_ANSWER for the user QUERY.

QUERY```
{query}
```

GROUND_TRUTH```
{groundTruth}
```

GENERATED_ANSWER```
{generatedAnswer}
```

Determine whether the GENERATED_ANSWER is the same s the GROUND_TRUTH.

Answer with Pass = true / false as per the schema given.

Note the GENERATED_ANSWER can vary in format and content but it should
contain the essential data facts given in the GROUND_TRUTH.
Primarily, for most queries, the 'plan' name or title is key.
And as such the plan names in GROUND_TRUTH are expected to be present
in the GENERATED_ANSWER.
"""

let judgeTest (test:TestResult) =
    async {
        let generatedAnswer = 
            match test.Attempt.Attempt with
            | Direct ans -> Some ans
            | CodeGen c -> match c.Resp with EvalOutput ans -> Some ans | _ -> None
        match generatedAnswer with
        | Some ans -> 
            let prompt = evalPrompt test.TestCase.Query test.TestCase.Answer ans
            let! resp = AICore.getOutput parmsO3Mini "You are a judge for comparing two answers" prompt typeof<Judgement>
            let jdmnt = JsonSerializer.Deserialize<Judgement>(resp.Content)
            printfn $"model: {test.Model}; query: {test.TestCase.Query}; pass:{jdmnt.Pass}; comments: {jdmnt.Comments}"
            return { test with Judgement = Some jdmnt}
        | None -> return test
    }
        
let judgeTestSet tests =
    tests
    |> AsyncSeq.ofSeq
    |> AsyncSeq.mapAsyncParallelThrottled 2 judgeTest
    |> AsyncSeq.toBlockingSeq
    |> Seq.toList
    
let test1() =
    let fn = "/Users/faisalwaris/eval/tests/gpt4o_code.json" //testFiles.[0]
    let ts1 = fn |> Packages.loadTestResults
    let resp = ts1.Head |> judgeTest |> runA
    printfn "%A" (resp.Judgement)


let evalResults fn = 
     printfn "%s" fn
     let tests = Packages.loadTestResults fn
     let judged = judgeTestSet tests
     Packages.saveTestResults fn judged
     judged


let evalTestResults() = testFiles |> Array.map evalResults
    
let loadResults() =
    testFiles
    |> Array.map (fun fn -> Packages.loadTestResults fn)
    |> Seq.collect id
    |> Seq.toList

let testType = function CodeGen _ -> "Code" | _ -> "Direct"
let passVal = function true -> 1.0 | _ -> 0.0

let plotCorrect() =
    loadResults()
    |> List.groupBy (fun t -> t.Model,testType t.Attempt.Attempt)
    |> List.map(fun (k,xs) -> $"{fst k} {snd k}", xs |> List.map (fun x -> passVal x.Judgement.Value.Pass) |> List.average)
    |> Chart.Bar
    |> Chart.show
   
let plotCorrectVsTime() = 
    loadResults()
    |> List.groupBy (fun t -> t.Model,testType t.Attempt.Attempt)
    |> List.map(fun (k,xs) -> 
        let text = $"{fst k} {snd k}"
        let corr = xs |> List.map (fun x -> passVal x.Judgement.Value.Pass) |> List.average
        let time = xs |> List.map _.Attempt.Time.TotalSeconds |> List.average
        Chart.Scatter([corr],[time],mode=StyleParam.Mode.Markers_Text,Text=text,TextPosition=StyleParam.TextPosition.TopRight,ShowLegend=false)        
        |> Chart.withTraceInfo text ) 
    |> Chart.combine
    |> Chart.withTitle "Avg. Correctness vs. Avg. Time (seconds) by Model and Mode"
    |> Chart.withXAxisStyle "Avg. Correctness"
    |> Chart.withYAxisStyle "Avg. Response Time (seconds)"
    |> Chart.withXAxisStyle(MinMax=(0.,1.))    
    |> Chart.withMarkerStyle(Size=15)
    |> Chart.withSize(1000,500)
    |> Chart.show


let palette = 
    [ Color.fromString "#1f77b4"
      Color.fromString "#ff7f0e"
      Color.fromString "#2ca02c"
      Color.fromString "#d62728"
      Color.fromString "#9467bd"
      Color.fromString "#8c564b"
      Color.fromString "#e377c2"
      Color.fromString "#7f7f7f"
      Color.fromString "#bcbd22"
      Color.fromString "#17becf" 
    ]
    

let plotTime() = 
    loadResults()
    |> List.groupBy (fun t -> t.Model,testType t.Attempt.Attempt)
    |> List.map(fun (k,xs) -> $"{fst k} {snd k}", xs |> List.map (fun x -> x.Attempt.Time.TotalSeconds) |> List.average)
    |> Chart.Bar
    |> Chart.show
    
let plotTokens() = 
    loadResults()
    |> List.groupBy (fun t -> t.Model,testType t.Attempt.Attempt)
    |> List.map(fun (k,xs) -> $"{fst k} {snd k}", xs |> List.map (fun x -> x.Attempt.Usage.TotalTokens) |> List.average)
    |> Chart.Bar
    |> Chart.show


(*
evalTestResults()
testFiles
let file = testFiles.[0]
let rt = evalResults file

plotCorrectVsTime()
plotCorrectVsTimeScatter()

plotTime()
plotTokens()
plotCorrect()
*)
