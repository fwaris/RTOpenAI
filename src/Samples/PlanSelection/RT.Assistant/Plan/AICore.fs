namespace RT.Assistant.Plan
open System
open System.IO

type ApiParms = {Model:string; Key:string}

module AICore =    
    module models =
        let gpt_4o = "gpt-4o"
        let gpt_4o_mini = "gpt-4o-mini"
        let sonnet_37 = "claude-3-7-sonnet-20250219"
        let o3_mini = "o3-mini"
        let gemini_think = "gemini-2.0-flash-thinking-exp"
        let gemini_flash = "gemini-2.0-flash"
        let isReasoning model = model = gemini_think || model = o3_mini || model = sonnet_37

    let skipLineComment (s:string) = let i = s.IndexOf('%') in if i >= 0 then s.Substring(0,i) else s

    let stripComments (s:string) =
        let rec loop (acc:char list) inComment xs =
            match xs with
            | [] -> acc |> List.rev |> Seq.toArray |> String
            | ('/','*')::ys when not inComment -> loop acc true ys
            | ('*','/')::_::ys when inComment -> loop acc false ys
            | [('*','/');(_,b)] when inComment -> loop (b::acc) false []
            | _::ys when inComment -> loop acc inComment ys
            | [(a,b)] when not inComment -> loop (b::a::acc) inComment []
            | (c,_)::ys -> loop (c::acc) inComment ys
        let xs = s |> Seq.pairwise |> Seq.toList
        loop [] false xs
    
    let lines keepEmptyLines (s:string) =
        seq{
            use sr = new StringReader(s)
            let mutable line = sr.ReadLine()
            while line <> null do
                yield line
                line <- sr.ReadLine()
        }
        |> Seq.filter (fun s -> if keepEmptyLines then true else s |> String.IsNullOrWhiteSpace |> not)
        |> Seq.toList
        
    let extractTripleQuoted (inp:string) =
        let lines = lines false inp
        let addSnip acc accSnip =
            match accSnip with
            |[] -> acc
            | _ -> (List.rev accSnip)::acc
        let isQuote (s:string) = s.StartsWith("```")
        let rec start acc (xs:string list) =
            match xs with
            | []                   -> List.rev acc
            | x::xs when isQuote x -> accQuoted acc [] xs
            | x::xs                -> start acc xs
        and accQuoted acc accSnip xs =
            match xs with
            | []                   -> List.rev (addSnip acc accSnip)
            | x::xs when isQuote x -> start (addSnip acc accSnip) xs
            | x::xs                -> accQuoted acc (x::accSnip) xs
        start [] lines

    let ensureEndDot (s:string) = let s = s.Trim() in if s.EndsWith('.') then s else s + "."

    let extractCode (inp:string) =
        if inp.IndexOf("```", min 10 inp.Length) >= 0 then 
            let code = 
                extractTripleQuoted inp
                |> Seq.collect id
                |> fun xs -> String.Join("\n",xs)
            if inp.Contains("```prolog") then
                code.Replace("?-","") //cprolog does not support ?- op
                |> ensureEndDot
            else
                code
        else
            inp
                
    let removeDot (inp:string) = let s = inp.Trim() in if s.EndsWith('.') then s.TrimEnd('.') else s
  
    let endpoint model =
        if model = models.gpt_4o then None
        elif model = models.o3_mini then None
        elif model = models.gemini_think || model = models.gemini_flash then Some("google")
        elif model = models.sonnet_37 then Some("anthropic")
        else None            
        
