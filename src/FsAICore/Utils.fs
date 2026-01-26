namespace FsAICore
open System
open System.IO
open System.Text.Json
open System.Runtime.InteropServices
open System.Text.Encodings.Web
open System.Text.Json.Serialization

[<AutoOpen>]
module Utility =
    
    let homePath = lazy(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
    
    let debug (msg:string) =
        System.Diagnostics.Debug.WriteLine(msg)

    let shorten n (s:string) =
        if s.Length < n then
            s
        else
            let left = s.Substring(0,n/2)
            let right = s.Substring(s.Length - n/2)
            left + " [\u2026] " + right

    let isEmpty (s:string) =
        String.IsNullOrWhiteSpace s

    let checkEmpty s = if isEmpty s then None else Some s

    let fixEmpty s = if isEmpty s then "" else s

    let (@@) (a:string) (b:string) = Path.Combine(a,b)

    /// String comparison that ignores case
    let (=*=) (a:string) (b:string) = a.Equals(b, StringComparison.OrdinalIgnoreCase)
    let (====) (a:string option) (b:string option) =
        match a,b with
        | Some a, Some b -> a =*= b
        | None,None      -> true
        | _              -> false
    
    let isWindows() = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    let isMac() = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
    let isLinux() = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)

    let ddict xs = System.Collections.Generic.Dictionary(dict xs)

    ///<summary>
    ///Json serialization options suitable for deserializing OpenAI 'structured output'.<br />
    ///Note: can use simple enums, in such types but not F# DUs
    ///</summary>
    let openAIResponseSerOpts =
        let o = JsonSerializerOptions(JsonSerializerDefaults.General)
        o.Converters.Add(JsonStringEnumConverter())
        o.WriteIndented <- true
        o.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        o.ReadCommentHandling <- JsonCommentHandling.Skip
        let opts = JsonFSharpOptions.Default()
        opts
            .WithSkippableOptionFields(true)
            .AddToJsonSerializerOptions(o)
        o           

    ///Serialize object to json with minimal escaping
    let formatJson<'t>(j:'t) =
        JsonSerializer.Serialize(j,openAIResponseSerOpts)

    let prependToFile (t:string) (f:string) =
        let pText = if File.Exists f then File.ReadAllText f else ""
        File.WriteAllText(f,t + pText)
