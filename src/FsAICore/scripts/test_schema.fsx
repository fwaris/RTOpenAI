#r "nuget: FSharp.Data.JsonSchema"
#load "../JsonUtils.fs"
type D = {t:string; i:int}
FsAICore.JsonUtils.toSchema typeof<D> 