open SbsSW.SwiPlCs
open System.IO
open System.Text.Json

type Input = {
    File : string
    Query : string
}

let evalProlog (jsonFile:string) = 
    if not (PlEngine.IsInitialized) then  PlEngine.Initialize([|"-q";"--no-signals"|])    
    //let mple1 = new PlMtEngine()
    //mple1.PlSetEngine()
    let mutable q = Unchecked.defaultof<_>
    try 
        try 
            let str = File.ReadAllText(jsonFile)
            let json = JsonSerializer.Deserialize<Input>(str)
            let inputFile = json.File.Replace("\\","/")
            let r = PlQuery.PlCall($"consult('{inputFile}').")
            if not r then failwith $"unable to compile prediciate"
            q  <- new PlQuery(json.Query)        
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
            System.Console.WriteLine($"{rslt}")
        with ex -> 
            System.Console.Error.WriteLine($"Error {ex.Message}")            
    finally                
        if q <> Unchecked.defaultof<_> then q.Dispose()        
        //mple1.PlDetachEngine()
        //mple1.Dispose()
        PlEngine.PlHalt()
        ()

[<EntryPoint>]
let main args = 
    evalProlog args.[0]
    0
