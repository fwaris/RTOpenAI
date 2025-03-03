#r "nuget: FSharp.Data.Mutator, 0.1.1-beta" //local package built from source with latest FSharp.Data
#load "AICore.fsx"

(* for quick testing of code snippets. Loads packages and code files typically required for 'assistant' related tasks *)
open System

open FSharp.Data
open FSharp.Data.Mutator

[<Literal>]
let PlanJson = __SOURCE_DIRECTORY__ + "/PLANS.json"

type T_Plan = JsonProvider< PlanJson, SampleIsList=true>
let tPlan = T_Plan.GetSamples() 
let ts = tPlan |> Seq.toList |> List.indexed |> List.map(fun (i,x) -> x |> Change <@ fun x -> x.Id.JsonValue = JsonValue.Number i @>)
let str = ts |> List.map (fun x->x.JsonValue.ToString()) |> String.concat ",\n"
let jstr = $"""[{str }]"""
System.IO.File.WriteAllText(__SOURCE_DIRECTORY__ + "/PLANS2.json",jstr)



