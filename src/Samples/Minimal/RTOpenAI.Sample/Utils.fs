namespace RTOpenAI.Sample
open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Security.Cryptography

type LogLevel = Verbose | Terse
                     
[<AutoOpen>]
module Utils =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)

    let notEmpty (s:string) = String.IsNullOrWhiteSpace s |> not
    let isEmpty (s:string) = String.IsNullOrWhiteSpace s 
    let contains (s:string) (ptrn:string) = s.Contains(ptrn,StringComparison.CurrentCultureIgnoreCase)
    let checkEmpty (s:string) = if isEmpty s then None else Some s

    let shorten len (s:string) = if s.Length > len then s.Substring(0,len) + "..." else s

    let (@@) a b = System.IO.Path.Combine(a,b)
                    
    let serOptionsFSharp = 
        let o = JsonSerializerOptions(JsonSerializerDefaults.General)        
        o.WriteIndented <- true
        o.ReadCommentHandling <- JsonCommentHandling.Skip        
        let opts = JsonFSharpOptions.Default()
        opts
            .WithSkippableOptionFields(true)            
            .AddToJsonSerializerOptions(o)        
        o
