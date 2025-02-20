namespace RT.Assistant.Plan
open System
open System.IO
open Microsoft.SemanticKernel.ChatCompletion
open Microsoft.SemanticKernel.Connectors.OpenAI
open RT.Assistant

type QueryResult =
    {
        Index: int
        Plan : Map<string,string>
    }
   
type Answer = {
    Results : QueryResult list
    ErroredOut : bool
    Message : string option
}

module AICore =
    
    module models =
        let o1 = "o1"    
        let gpt_4o = "gpt-4o"
        let gpt_4o_mini = "gpt-4o-mini"
        let o3_mini = "o3-mini"
        let isReasoning model = model = o1 || model = o3_mini
    
    let lines skipEmptyLines (s:string) =
        seq{
            use sr = new StringReader(s)
            let mutable line = sr.ReadLine()
            while line <> null do
                yield line
                line <- sr.ReadLine()
        }
        |> Seq.filter (fun s -> if skipEmptyLines then true else s |> String.IsNullOrWhiteSpace |> not)
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
        let rawCode =
            if inp.Contains("```prolog") then 
                extractTripleQuoted inp
                |> Seq.collect id
                |> fun xs -> String.Join("\n",xs)
            else
                inp
        rawCode.Replace("?-","") //cprolog does not support ?- op
        |> ensureEndDot

    let removeDot (inp:string) = let s = inp.Trim() in if s.EndsWith('.') then s.TrimEnd('.') else s
    
    let runModelWithOptions (model:string) (sysMsg:string) (prompt:string) opts =
        async {
            let key = RT.Assistant.Settings.Environment.apiKey()
            if Utils.isEmpty key then failwith "openai key not found"
            let ch = ChatHistory(sysMsg)
            ch.AddUserMessage(prompt)
            let svc = OpenAIChatCompletionService(model,key)
            let! rslt = svc.GetChatMessageContentsAsync(ch,opts) |> Async.AwaitTask
            return rslt.[0].Content            
        }    
                      
    let runModel (model:string) (sysMsg:string) (prompt:string) =
        let opts = OpenAIPromptExecutionSettings()
        if not (models.isReasoning model) then 
            opts.Temperature <- 0.1
        runModelWithOptions model sysMsg prompt opts                                   
        
    //run with structured output
    let getOutput (model:string) (sysMsg:string) (prompt:string) (outputFormat:Type) =
        let opts = OpenAIPromptExecutionSettings()
        opts.ResponseFormat <- outputFormat
        if not (models.isReasoning model) then 
            opts.Temperature <- 0.1
        runModelWithOptions model sysMsg prompt opts        
        
    let runO1Mini (sysMsg:string) (prompt:string) =
        let msg = $"{sysMsg}\n\n{prompt}" //o1-mini does not seem to like receiving a sys. msg.
        let svc = OpenAIChatCompletionService("o1-mini",Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
        let rslt = svc.GetChatMessageContentAsync(msg)
        rslt.Result.Content

    let runO1 = runModel models.o1    
    let run4o = runModel models.gpt_4o
    let run4oMini = runModel models.gpt_4o_mini
    let run03Mini = runModel models.o3_mini

    let skipLineComment (s:string) = let i = s.IndexOf('%') in if i >=0 then s.Substring(0,i) else s

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
        
