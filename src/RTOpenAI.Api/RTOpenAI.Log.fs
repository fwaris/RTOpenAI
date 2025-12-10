namespace RTOpenAI.Api

open Microsoft.Extensions.Logging
open Microsoft.Maui

type RTOpenAILog() = class end 
module Log =
        let mutable debug_logging = false
        let getLogger() = IPlatformApplication.Current.Services.GetService(typeof<ILogger<RTOpenAILog>>)  :?> ILogger<RTOpenAILog>      
        let private _log= lazy(getLogger())
        let info  (msg:string) = if _log.Value <> Unchecked.defaultof<_> then _log.Value.LogInformation(msg)
        let warn (msg:string) = if _log.Value <> Unchecked.defaultof<_> then _log.Value.LogWarning(msg)
        let error (msg:string) = if _log.Value <> Unchecked.defaultof<_> then _log.Value.LogError(msg)
        let exn (exn:exn,msg) = if _log.Value <> Unchecked.defaultof<_> then _log.Value.LogError(exn,msg)
        
        
        

