namespace RT.Assistant

open Fabulous.Maui.Transform
open FSharp.DI

type RTOpenAILog() = class end 
module Log =     
        let private log = DI.loggerLazy<RTOpenAILog>()
        let info (msg:string) = log.Value.info msg
        let warn (msg:string) = log.Value.warn msg
        let error (msg:string) = log.Value.error msg
        let exn (exn:exn,msg) = log.Value.exn (exn,msg)
