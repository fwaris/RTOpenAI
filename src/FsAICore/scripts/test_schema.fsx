#r "nuget: Microsoft.Extensions.AI.Abstractions, 10.5.0"
#load "../JsonUtils.fs"
type D = {t:string; i:int}
FsAICore.JsonUtils.toSchema typeof<D> 
