#r @"..\..\SwiPlcsCore\bin\Debug\net9.0\SwiPlcsCore.dll"
open System
open System.IO
open SbsSW.SwiPlCs


if not PlEngine.IsInitialized then  PlEngine.Initialize([|"-q"|])

let clausesFile_ = __SOURCE_DIRECTORY__ + "/../Resources/Raw/wwwroot/plan_clauses.pl"
let clausesFile = Path.GetFullPath(clausesFile_).Replace("\\","/") //forward slashes needed
printfn "%s" clausesFile

let test1() = 
    use q =  new PlQuery("member(A,[a,b,c]).")  
    while q.NextSolution() do
        printfn $"{q.VariableNames.[0]}, {q.Args.[0].ToString()}"

let test2() = 
    let r = PlQuery.PlCall($"consult('{clausesFile}').")
    printfn $"consult: {r}"
    use q = new PlQuery("plan(T,_,_,_).")
    while (q.NextSolution()) do
        printfn $"{q.VariableNames.[0]}, {q.Args.[0].ToString()}"


let testConsult = """
valid_plan_for_military_veteran(Title, Lines, Features) :-
    plan(Title, category(military_veteran), lines(Lines), features(Features)),
    member(line(4, monthly_price(MonthPrice), _), Lines),
    % Assuming 'around $100' means a maximum of $100 total monthly price
    MonthPrice =< 120
"""
let testQuery = "valid_plan_for_military_veteran(Title,Lines, Features)."

let test3() = 
    //use frame = new PlFrame() //new terms are valid only for the lifetime of the frame    
    let r = PlQuery.PlCall($"consult('{clausesFile}').")
    let mutable av = new PlTermV(1)
    let pred = new PlTerm(testConsult)
    av.[0] <- pred
    let a = PlQuery.PlCall($"assert(({testConsult})).")
    use q = new PlQuery(testQuery)
    q.VariableNames |> Seq.toList |> printfn "%A"    
    let mutable c = 1
    while q.NextSolution() do
        for i in 0 .. q.VariableNames.Count - 1 do
            printf $"{c}; {q.VariableNames.[i]}={q.Args.[i].ToString()}, "
        printfn ""
        c <- c + 1

    //a.Dispose()
    q.Dispose();   // IMPORTANT ! never forget to free the query before the PlFrame is closed
    //frame.Dispose()

test3()

$"assert(({testConsult}))." |> printfn "%s"


