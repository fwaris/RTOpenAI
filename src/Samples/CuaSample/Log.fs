namespace FsPlaySamples.Cua

open FSharp.DI

type LogCategory() = class end 
module Log =     
        let private log = DI.loggerLazy<LogCategory>()
        let info (msg:string) = log.Value.info msg
        let warn (msg:string) = log.Value.warn msg
        let error (msg:string) = log.Value.error msg
        let exn (exn:exn,msg) = log.Value.exn (exn,msg)
