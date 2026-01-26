#load "packages.fsx"
open System
open System.IO
open FSharp.Data
open System.Text.Json

let (@@) (a:string) (b:string) = Path.Combine(a,b)

let homePath = lazy(
    match Environment.OSVersion.Platform with 
    | PlatformID.Unix 
    | PlatformID.MacOSX -> Environment.GetEnvironmentVariable("HOME") 
    | _                 -> Environment.GetEnvironmentVariable("USERPROFILE"))

let path = homePath.Value @@ "Library" @@ "dom.txt"

let txt = File.ReadAllText path
let html = HtmlDocument.Load(path)


let jsonFile = homePath.Value @@ "temp" @@ "clickables2.json"
let jtxt = File.ReadAllText jsonFile

type Clickable = {
    tag : string
    label : string
    x : float
    y : float
    width : float
    height : float
}
let j = JsonSerializer.Deserialize<Clickable list>(jtxt)
