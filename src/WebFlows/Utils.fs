namespace WebFlows
open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Text.Json.Serialization.Metadata
open System.Security.Cryptography

type LogLevel = Verbose | Terse

///treats the value "null" as equal to null. 
type NullConverter< 'T >() = 
    inherit JsonConverter< Option<'T> >()
    
    let dser(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
            let str = reader.GetString()
            let v = JsonSerializer.Deserialize<'T>(str)
            Some v

    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
        if reader.TokenType = JsonTokenType.String then                
            let json = reader.GetString()
            if json = "null" then
                None            
            else
                dser(&reader, typeToConvert, options)
        elif reader.TokenType = JsonTokenType.Null then
            None
        else
            dser(&reader, typeToConvert, options)

    override _.Write(writer: Utf8JsonWriter, value: 'T option, options: JsonSerializerOptions) =
        match value with
        | None -> writer.WriteNullValue()
        | Some t -> 
            let json = JsonSerializer.Serialize(t, options)
            writer.WriteStringValue(json)

[<AutoOpen>]
module Utils =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)
    
    let (===) (a:string) (b:string) = a.Equals(b,StringComparison.CurrentCultureIgnoreCase)
    
    let (====) (a:string option) (b:string option) =
        match a,b with
        | Some a, Some b -> a === b
        | None,None      -> true
        | _              -> false
    
    let newId() = 
        Guid.NewGuid().ToByteArray() 
        |> Convert.ToBase64String 
        |> Seq.takeWhile (fun c -> c <> '=') 
        |> Seq.map (function '/' -> 'a' | c -> c)
        |> Seq.toArray 
        |> String

    let notEmpty (s:string) = String.IsNullOrWhiteSpace s |> not
    let isEmpty (s:string) = String.IsNullOrWhiteSpace s 
    let contains (s:string) (ptrn:string) = s.Contains(ptrn,StringComparison.CurrentCultureIgnoreCase)
    let checkEmpty (s:string) = if isEmpty s then None else Some s

    let shorten len (s:string) = if s.Length > len then s.Substring(0,len) + "..." else s

    let (@@) a b = System.IO.Path.Combine(a,b)
        
    let genKey () =
        let key = Aes.Create()
        key.GenerateKey()
        key.GenerateIV()
        key.Key,key.IV

    let encrFile (key,iv) (path:string)(outpath) = 
        use enc = Aes.Create()
        enc.Mode <- CipherMode.CBC
        enc.Key <- key
        enc.IV <- iv
        use inStream = new FileStream(path, FileMode.Open)
        use outStream = new FileStream(outpath, FileMode.Create)
        use encStream = new CryptoStream(outStream, enc.CreateEncryptor(), CryptoStreamMode.Write)  
        inStream.CopyTo(encStream)

    let decrFile (key,iv) (path:string) (outpath:string) = 
        use enc = Aes.Create()
        enc.Mode <- CipherMode.CBC
        enc.Key <- key
        enc.IV <- iv
        use inStream = new FileStream(path, FileMode.Open)
        use decrStream = new CryptoStream(inStream, enc.CreateDecryptor(), CryptoStreamMode.Read)  
        use outStream = new FileStream(outpath, FileMode.Create)
        decrStream.CopyTo(outStream)

    let toInt16Buffer (base64:string) = 
        let bytes = Convert.FromBase64String(base64) //pcm audio format little endian regardless of platform
        let buff:int16[] = Array.zeroCreate (bytes.Length/2)
        for i in 0..buff.Length-1 do
            let short = BitConverter.ToInt16(bytes,i*2)
            buff.[i] <- short
            
    let serOptionsFSharp = lazy(
        let o = JsonSerializerOptions(JsonSerializerDefaults.General)
        o.TypeInfoResolver <- new DefaultJsonTypeInfoResolver()
        o.WriteIndented <- true
        o.ReadCommentHandling <- JsonCommentHandling.Skip        
        let opts = JsonFSharpOptions.Default()
        opts
            .WithSkippableOptionFields(true)
            .WithAllowNullFields(true)
            .AddToJsonSerializerOptions(o)        
        o)
