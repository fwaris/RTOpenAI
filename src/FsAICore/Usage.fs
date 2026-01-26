namespace FsAICore

type Usage = {input_tokens:int; output_tokens:int; total_tokens:int}
    with
    static member Default = {input_tokens = 0; output_tokens=0; total_tokens = 0}
    
type UsageMap = Map<string,Usage>

module Usage =
    let combineUsage (a:UsageMap) (b:UsageMap) =
        (Map.toList a) @ (Map.toList b)
        |> List.groupBy fst
        |> List.map(fun (k,xs) ->
            k,
            (Usage.Default,xs) ||> List.fold (fun acc (_,c) ->
                {
                    total_tokens = acc.total_tokens + c.total_tokens
                    input_tokens = acc.input_tokens + c.input_tokens
                    output_tokens = acc.output_tokens + c.output_tokens
                }))
        |> Map.ofList    

