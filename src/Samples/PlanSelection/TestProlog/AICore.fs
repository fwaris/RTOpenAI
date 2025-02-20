module AICore
open System
open System.IO
open Microsoft.SemanticKernel.ChatCompletion
open Microsoft.SemanticKernel.Connectors.OpenAI

let extractTripleQuoted (inp:string) =
    let lines =
        seq {
            use sr = new System.IO.StringReader(inp)
            let mutable line = sr.ReadLine()
            while line <> null do
                yield line
                line <- sr.ReadLine()
        }        
        |> Seq.toList
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

let extractCode inp =
    extractTripleQuoted inp
    |> Seq.collect id
    |> fun xs -> String.Join("\n",xs)

let runModel (model:string) (sysMsg:string) (prompt:string) =
    let ch = ChatHistory(sysMsg)
    ch.AddUserMessage(prompt)
    let svc = OpenAIChatCompletionService(model,Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    let rslt = svc.GetChatMessageContentsAsync(ch).Result
    rslt.[0].Content        

let run = runModel "o1"
let runFast = runModel "gpt-4o"
let runFastest = runModel "gpt-4o-mini"

let removeComment (s:string) = let i = s.IndexOf('%') in if i >=0 then s.Substring(0,i) else s

let removeMultiLineComment (s:string) =
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

let isClauseEnd (s:string) = s.Trim().EndsWith(").")

let rec splitClauses accClauses accC xs =
  match xs with
  | [] -> (accC |> List.rev |> String.concat "\n"):: accClauses |> List.rev
  | x::ys when isClauseEnd x -> splitClauses ((List.rev (x::accC) |> String.concat "\n")::accClauses) [] ys
  | x::ys -> splitClauses accClauses (x::accC) ys
  
let lines (s:string) =
  seq{
    use s = new StringReader(s)
    let mutable line = s.ReadLine()
    while line <> null do
      yield line
      line <- s.ReadLine()
  }
  
let stripComments (s:string) =
    s |> removeMultiLineComment |> lines |> Seq.map removeComment |> String.concat "\n"
