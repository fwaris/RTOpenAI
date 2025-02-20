namespace RTOpenAI.Sample
open System
open System.IO
open type Fabulous.Maui.View
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel
open FSharp.Control

[<RequireQualifiedAccess>]
module Audio = 

    let getRecordPermission() = Permissions.RequestAsync<Permissions.Microphone>()        
    let haveRecordPermission (model:Model) =
        task {
            let! permission = MainThread.InvokeOnMainThreadAsync<PermissionStatus>(fun () -> getRecordPermission())
            RTOpenAI.Api.Utils.debug $"Permission: {permission}"
            if permission = PermissionStatus.Granted then
                return Some permission
            else
                return None            
        }
        |> Async.AwaitTask
  
      