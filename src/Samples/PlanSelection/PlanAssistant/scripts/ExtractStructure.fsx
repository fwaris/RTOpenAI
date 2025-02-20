#load "AICore.fsx"
(*
This script loads the json document that contains the available plans
and converts them to the plan(Title,category(Category),lines(Lines),features(Features)) structures.
This is a multistep processes that involves several LLM invocations to accomplish the task.
*)
open System
open System.IO
open FSharp.Data

let ignoreCase = StringComparison.CurrentCultureIgnoreCase

[<Literal>]
let PlanJson = __SOURCE_DIRECTORY__ + "/PLANS.json"
type T_Plan = JsonProvider< PlanJson, SampleIsList=true>
let tPlan = T_Plan.GetSamples() |> Seq.toList

let byPlan = tPlan |> List.groupBy(fun x->x.Plan)

let addProp tag f (x:T_Plan.Root) acc =
    let v : int = f x
    let ls = 
      acc
      |> Map.tryFind tag |> Option.map (fun ps ->
        let ls = ps |> Map.tryFind tag |> Option.map (fun ss -> Set.add v ss) |> Option.defaultValue (set [v])
        Map.add tag ls ps)
      |> Option.defaultWith(fun _ -> Map.ofSeq [tag,set [v]])
    Map.add  tag ls acc
      
let compressPlan acc (x:T_Plan.Root)=
  let line = x.NumberOfLines
  let salePrice = x.PricePerMonth
  let regPrice = x.OriginalPricePerMonth
  let acc =
    acc
    |> addProp "lines" _.NumberOfLines x
    |> addProp "salePrice" _.PricePerMonth x
    |> addProp "regPrice" _.OriginalPricePerMonth x
  let kvs = x.JsonValue.Properties() |> Seq.filter(fun (k,v) -> v.Properties().Length = 0)
  (acc,kvs)
  ||> Seq.fold (fun acc (k,v) ->
    let sval = v.AsString()
    let pm = 
      acc
      |> Map.tryFind k
      |> Option.map (fun pm ->
          let ss = pm |> Map.tryFind sval |> Option.map (fun ss -> ss |> Set.add line) |> Option.defaultValue (set [line])
          Map.add sval ss pm)
      |> Option.defaultValue ([sval, set [line]] |> Map.ofSeq)
    Map.add k pm acc)
  
let compressesPlans = byPlan |> List.map (fun (n,xs) -> n, (Map.empty,xs) ||> List.fold compressPlan)

let printCompressedPlans planNameOpt =
  compressesPlans
  |> Seq.filter(fun (n,_) -> match planNameOpt with Some n' -> n' = n | _ -> true)
  |> Seq.iter (fun (k,ms) ->
    printfn $"* {k}"
    ms
    |> Map.toSeq
    |> Seq.map (fun (p,ls) -> ls |> Map.toSeq |> Seq.map (fun (v,ms) -> $"""  {p}: [{ms |> Seq.map string |> String.concat ","}]={v}""") |> String.concat "; ")
    |> Seq.iter (printfn "%s"))

(*
printCompressedPlans None
printCompressedPlans (Some "Connect")
*)

let propsByPlan =
  byPlan
  |> List.map(fun (k,ps) ->
    k,
    (Map.empty,ps) ||> List.fold (fun acc k ->
      let kvs = k.JsonValue.Properties() |> Seq.filter(fun (k,v) -> v.Properties().Length = 0)
      (acc,kvs)    
      ||> Seq.fold (fun acc (k,j) ->
        let v = j.AsString()
        let vs = acc |> Map.tryFind k |> Option.map (fun vs -> Set.add v vs) |> Option.defaultValue (set [v])
        acc |> Map.add k vs)))  

let showPlansWithLineDependentProps() =
  propsByPlan
  |> List.iter(fun  (p,ms) ->
    printfn $"* {p}"
    let complexProps = ["_id";"Number_of_lines"; "Original_Price_Per_Month"; "Price_Per_Month"] |> set
    let ms = ms |> Map.filter (fun k v -> complexProps.Contains k |> not)  
    ms |> Map.toSeq |> Seq.filter (fun (k,vs) -> vs.Count > 1) |> Seq.iter (fun (k,vs) -> printfn $"  {k}, {Set.count vs}"))

(*
showPlansWithLineDependentProps()
*)

let allProps() = 
  (Map.empty,propsByPlan)
  ||> List.fold(fun acc (plan,props) -> 
    (acc,props) 
    ||> Map.fold(fun acc k v ->
      let v' = 
        acc
        |> Map.tryFind k 
        |> Option.map(fun s -> Set.union s v)
        |> Option.defaultValue v
      Map.add k v' acc))

let printProp k vs =    
  printfn $"* {k}"
  vs |> Seq.iter (fun v -> printfn $"  {v}")

let printPropsAndValueSets() =
  allProps()
  |> Map.iter printProp
let printFtrPropsAndValues() =
  let excludeList = set ["_id"; "Price_Per_Month"; "Original_Price_Per_Month"; "Number_of_lines"; ]
  allProps()
  |> Map.filter(fun k v -> excludeList.Contains k |> not)
  |> Map.iter printProp

let printPropsContaining(text:string) = 
  allProps() 
  |> Map.filter(fun k v -> k.Contains(text, ignoreCase))
  |> Map.iter printProp

(*
printPropsAndValueSets()
printFtrPropsAndValues()
printPropsContaining "tax"
*)

(* ----------------------------------------------------------------------- *)

let propertiesList = """
taxes_and_fees_included
autopay_monthly_discount
no_annual_service_contract
phone_upgrades
wi_fi_calling
voicemail_to_text
scam_shield_premium
high_speed_data
access_5g_at_no_extra_cost
canada_and_mexico_included
unlimited_talk_and_text
unlimited_international_texting_from_home
data_and_texting_while_abroad
low_flat_rate_calling_while_abroad
mobile_hotspot
video_streaming_quality
in_flight_connection
apple_tv_plus
hulu
netflix
one_year_aaa_membership_on_us
connect_telco_travel
"""

let propertiesListTest = """
apple_tv_plus
hulu
netflix
"""

let featureNames = lazy(AICore.lines false propertiesList)

let templatePlanStructure = lazy(File.ReadAllText(__SOURCE_DIRECTORY__ + "../../Resources/Raw/plan_schema.pl"))
templatePlanStructure.Value
let basePlanStructure = lazy(File.ReadAllText(__SOURCE_DIRECTORY__ + "/BasePlanStructure.pl"))

let serializePlanGroup (plans:T_Plan.Root list) = $"""[
  {plans |> List.map (fun x -> x.JsonValue.ToString()) |> String.concat ",\n"}
]
"""

/// Trim properties from the json object
/// Ensure that the property whose name is closest to the given 'name', is kept.
/// Also keep some special properties ["Number_of_lines"]
/// All else are removed 
let filterToFeature (name:string) (j:JsonValue) =
  let d = F23.StringSimilarity.Damerau()
  let props = match j with JsonValue.Record props -> props | _ -> failwith "json object expected"
  let keyProp = 
    props
    |> Array.map(fun kv -> d.Distance(name,fst kv), kv)
    |> Array.sortBy fst
    |> Array.map snd
    |> Array.head
    |> fst
  let propSet = keyProp::["Number_of_lines"] |> set //; "Original_Price_Per_Month"; "Price_Per_Month"] |> set
  props |> Array.filter (fun (k,_) -> propSet.Contains k) |> JsonValue.Record
  
let serPlanGrpForFeature featureName (plans:T_Plan.Root list) =
  let jsons = plans |> List.map (fun x->x.JsonValue) |> List.map (filterToFeature featureName) |> List.map _.ToString()
  printfn "%A" jsons
  $"""[
  {jsons |> String.concat ",\n"}
]
"""

let mapToStructurePrompt (plans:T_Plan.Root list) = $""" 
PLAN_STRUCTURE below is template Prolog structure of a phone plan.
Given a group of related plans as a list of JSON objects in PLAN_GROUP, your job
is to create the plan structure from the combined
information in the PLAN_GROUP

PLAN_TEMPLATE```
{templatePlanStructure.Value}
```

PLAN_GROUP```
{serializePlanGroup plans}
```

Keep the 'features' property as an empty list.
Ensure the arity plan/4.
PLAN FACTS STRUCTURE:
```plan(Title,category(Category),lines(line(N,monthly_price(P), original_price(O)), ...]),features([feature(F1,applies_to_lines(all),feature(F2,...),...]))```
N = Number of lines
P = Current monthly price for the N number of lines (i.e. the sale price)
O = Original price (i.e. not the current sale price).

ONLY GENERATE PURE PROLOG CODE.
ANY COMMENTS SHOULD BE PROPER PROLOG COMMENTS, USING %%
```prolog
"""

(*
let testPlanstr = AICore.runO1 "You are an expert Prolog programmer" (mapToStructurePrompt (snd byPlan.Head))
printfn "%s" testPlanstr
*)

let mapToFeatureStructurePrompt feature (plans:T_Plan.Root list) = $"""
PLAN_STRUCTURE below is template Prolog structure of a phone plan.
Given a group of related plan features as a list of JSON objects in PLAN_GROUP, your job
is to create a properly populated feature structure for only the named feature given if FEATURE_NAME.

PLAN_STRUCTURE```
{templatePlanStructure.Value}
```

PLAN_GROUP```
{serPlanGrpForFeature feature plans}
```

FEATURE_NAME```
{feature}
```

Note that the information to populate the feature structure may be spread across
multiple plans in the group. Be sure to read and understand associated JSON values of the associated feature, to determine how to populate the values of feature.
If something is 'ON US' it means the feature is included.

Every feature has a 'desc' (description) field.  Populate its value from the texts of the associated JSON property values.
If all texts have the same value, then just use the actual text otherwise summarize the texts into a single text value.

Sometimes the fact values in the PLAN_STRUCTURE may list alternatives separated by '|'.
Ensure that generated Prolog contains only one of the alternatives and does not contain '|'.


ONLY generate the Prolog for the feature.

ONLY GENERATE PURE PROLOG CODE.
ANY COMMENTS SHOULD BE PROPER PROLOG COMMENTS, USING %%

```prolog
"""

(*
let feature = featureNames.Value |> List.skip 3 |> List.head  
let testFeatureStr = AICore.run4o "You are an expert Prolog programmer" (mapToFeatureStructurePrompt feature (snd byPlan.Head))
printfn "%s" testFeatureStr
*)

let folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/temp"

open System.Diagnostics

//run swi-prolog process in batch mode with the given script
let runSwipl args =
  let pi = ProcessStartInfo()
  pi.FileName <- "swipl"
  pi.WorkingDirectory <- folder
  pi.Arguments <- args
  let pf = Process.Start(pi)
  pf.WaitForExit() 

//generate prolog for each of the features of the plan group
let planFeatures planGroup =    
    featureNames.Value
    |> List.map(fun feature ->
      printfn $"feature {feature}"
      let rslt = AICore.run03Mini "You are an expert Prolog programmer" (mapToFeatureStructurePrompt feature planGroup)
      printfn $"{rslt}"
      rslt)

//construct Prolog script to add the feature collection to the base plan structure
let planScript (title:string) baseStructure newFeatures = $"""
:- dynamic(plan/4).

export_clauses(Filename, Predicate) :-
    tell(Filename),  %% Open the file for writing
    listing(Predicate),  %% Write all clauses of the given predicate
    told.  %% Close the file

add_plan_features(Title,NewFeatures) :-
  plan(Title,B,D,features(CurrentFeatures)),
  retract(plan(Title,B,D,features(CurrentFeatures))),
  assertz(plan(Title,B,D,features(NewFeatures))).

{baseStructure}

{newFeatures}

addFeatures :- newFeatures(Fs),add_plan_features("{title}",Fs).

savePlan :- export_clauses('export_{title}.pl',plan/4).

"""
  
let generatePlan (planName:string,planGroup) =
  let planFileName = planName.Replace(" ","_")
  printfn $"running {planName}"
  let ftrs = planFeatures planGroup
  let baseStructure = AICore.run03Mini "You are an expert Prolog programmer" (mapToStructurePrompt planGroup) |> AICore.extractCode
  let newFeatures = $"""newFeatures([
    {ftrs |> List.map (AICore.extractCode>>AICore.removeDot) |> String.concat ",\n"}  
    ])."""
  let planScript = planScript planName baseStructure newFeatures
  let scriptFile = folder + $"/input_{planFileName}.pl"
  File.WriteAllText(scriptFile, planScript)
  let args = $"-q -f {scriptFile} -g addFeatures -g savePlan -t halt"
  runSwipl args
  
let missedPlans folder =
    let expected = byPlan |> List.map fst |> List.map(fun t-> $"export_{t}.pl".ToLower(),t)
    let processed =
          Directory.GetFiles(folder,"export*.pl")
          |> Seq.map Path.GetFileName
          |> Seq.map _.ToLower()
          |> set
    let rem = expected |> List.filter(fun (fn,t) -> processed.Contains fn |> not)
    let planNames = rem |> List.map snd |> set
    byPlan |> List.filter (fun (t,_) -> planNames.Contains t)


//tries to generate Prolog structures for each plan group (related plans)
//can be re-run to complete missed plans 
let generateAllPlans() =
  async {
    let missed = missedPlans folder
    printfn $"processing {missed.Length} plans"
    missed |> List.iter generatePlan
  }
  |> Async.Start

//needed for tau prolog to function correctly with generated plans
let preamble = $"""
:- set_prolog_flag(double_quotes, atom).
:- use_module(library(lists)). 
"""

//put all plan clauses into a single file and put that under the 'raw/wwwroot' folder
let consolidatePlans() = 
  let relPath = @"src/Samples/PlanSelection/PlanAssistant/Resources/Raw/wwwroot"
  let rootPath = __SOURCE_DIRECTORY__ + "../../../../../../"
  let outputFile = Path.GetFullPath(Path.Combine(rootPath,relPath,"plan_clauses.pl"))
  printfn "%s" outputFile
  Directory.GetFiles(folder,"export_*.pl")
  |> Seq.map File.ReadAllText
  |> Seq.map _.Replace(":- dynamic plan/4.","")
  |> String.concat "\n"
  |> fun txt -> File.WriteAllText(outputFile, $"{preamble}\n{txt}")
  

(*

generateAllPlans() //run this repeatedly till no more plans left to be processed
                   //(in a single run some plans may be skipped due to errors)

consolidatePlans()
*)

//need to validate that all plans have the same basic structure
let structureComparison = """
% abstract_term(+Term, -Abstract)
% Leaves variables unchanged, replaces atomic subterms with fresh variables,
% and recurses into compound terms.
abstract_term(Term, Abstract) :-
    var(Term), !,
    Abstract = Term.
abstract_term(Term, Abstract) :-
    atomic(Term), !,
    Abstract = _.
abstract_term(Term, Abstract) :-
    compound(Term),
    Term =.. [F|Args],
    maplist(abstract_term, Args, AbsArgs),
    Abstract =.. [F|AbsArgs].

% variant(+Term1, +Term2)
% Succeeds if Term1 and Term2 are variants of each other (ignoring atomic differences).
variant(Term1, Term2) :-
    abstract_term(Term1, Abs1),
    abstract_term(Term2, Abs2),
    % The following is one common way to check for variants:
    copy_term(Abs1, Copy1),
    copy_term(Abs2, Copy2),
    numbervars(Copy1, 0, _),
    numbervars(Copy2, 0, _),
    Copy1 == Copy2.

comparePlans(V) :- 
	findall(plan(A,B,C,D),plan(A,B,C,D),Fs),
     Fs=[plan(_,_,lines([L1|_]),_),plan(_,_,lines([L2|_]),_)|_],
    variant(L1,L2).
"""

