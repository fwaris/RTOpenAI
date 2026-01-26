#load "packages.fsx"
open RT.Assistant

let fl = FlowDefs.vzFlow.Path |> List.choose (function Clicks xs -> Some xs.[0] | _-> None)

let fpath (m:string) (e:ElemRef) = e.path |> Option.map (fun x -> x.Contains(m)) |> Option.defaultValue false

let ec = 
    fl 
    |> List.find  (fpath "Cancel")

let js = FlowsJs.findElementBoxJs ec |> snd |> printfn "%A"


let tokensinmth = 1500. * 60. * 60. * 24. * 30.
let cost_mth = 10000.0 / 12.0

let costPerMTokens = tokensinmth / 1000000.0 / cost_mth

let mtks = tokensinmth / 1000000.0 * 12.0

let costPerM = 10000.0 / mtks


