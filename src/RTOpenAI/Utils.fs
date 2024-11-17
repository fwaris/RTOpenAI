namespace RTOpenAI
open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Security.Cryptography

[<AutoOpen>]
module Utils =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)

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


    let serOptions = 
        let o = JsonSerializerOptions(JsonSerializerDefaults.General)
        o.WriteIndented <- true
        o.ReadCommentHandling <- JsonCommentHandling.Skip
        JsonFSharpOptions.Default()
            //.WithUnionEncoding(JsonUnionEncoding.NewtonsoftLike) //consider for future as provides better roundtrip support
            .AddToJsonSerializerOptions(o)        
        o

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
