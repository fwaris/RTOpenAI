namespace FsAICore.Functions

open System
open FsAICore
open Microsoft.SemanticKernel
open System.ComponentModel
open System.Text.Json

(*
Some generally applicable functions 
*)

module internal U =
    let homePath = lazy(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))

///semantic kernel 'plugin' class that implements memory functions
type FsOpMemory() =
    let mutable bag = Map.empty
    static member statefile = lazy(U.homePath.Value @@ "memory.json")

    static member serOpts = lazy(
        let opts = JsonSerializerOptions()
        opts.TypeInfoResolver <- System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        opts.WriteIndented <- true
        opts)

    member this.SetMemory(m) = bag <- m

    static member LoadState() =
        try
            if System.IO.File.Exists(FsOpMemory.statefile.Value) then
                use str = System.IO.File.OpenRead(FsOpMemory.statefile.Value)
                let map = System.Text.Json.JsonSerializer.Deserialize<Map<string,string list>>(str)
                let mem = new FsOpMemory()
                mem.SetMemory(map)
                mem
            else
                new FsOpMemory()
        with ex ->
            Log.exn(ex, nameof FsOpMemory.LoadState)
            new FsOpMemory()

    static member Serialize<'t>(o:'t) = JsonSerializer.Serialize(o,options=FsOpMemory.serOpts.Value)

    static member private _SaveState(map:Map<string,string list>) =
        try
            use str = System.IO.File.Create FsOpMemory.statefile.Value
            JsonSerializer.Serialize(str,map, options=FsOpMemory.serOpts.Value)
        with ex ->
            Log.exn(ex,nameof FsOpMemory._SaveState)

    [<KernelFunction("memory_save")>]
    [<Description("Save a key-value pair for later retrieval")>]
    member this.memory_save(key:string, value:string) =
        Log.info $"{nameof this.memory_save}:{key} = {value}"
        lock bag (fun _ -> 
            bag <-
                bag 
                |> Map.tryFind key 
                |> Option.map (fun vs -> bag |> Map.add key (List.distinct (value::vs)))
                |> Option.defaultWith (fun _ -> bag |> Map.add key [value])
            FsOpMemory._SaveState(bag)
        )
        "saved"

    [<KernelFunction("memory_get_all")>]
    [<Description("Retrieve all key value pairs saved in memory")>]
    member this.memory_get_all() =
        Log.info (nameof this.memory_get_all)
        FsOpMemory.Serialize(bag)

    [<KernelFunction("memory_get_all_keys")>]
    [<Description("retrieve all keys in the memory store ")>]
    member this.memory_get_all_keys() =
        let ks = Map.keys bag |> Seq.toList
        Log.info $"{nameof this.memory_get_all_keys}: {ks}"
        FsOpMemory.Serialize(ks)

    [<KernelFunction("memory_get_value")>]
    [<Description("retrieve a value for the given key")>]
    member this.memory_get_value(key:string) =
        let v = bag |> Map.tryFind key
        Log.info $"{nameof this.memory_get_value} {key} = {v}"
        FsOpMemory.Serialize(v)

    member this.getMemory() = bag

