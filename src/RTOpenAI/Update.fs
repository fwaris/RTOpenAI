namespace RTOpenAI
open System
open System.IO
open System.Threading
open FSharp.Control.Websockets
open Fabulous
open type Fabulous.Maui.View
open Microsoft.Maui.Controls
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel
open FSharp.Control
open RTOpenAI.Audio
open System.Threading.Tasks

module Audio = 
    let tempFile() = Path.Combine(FileSystem.CacheDirectory, Path.GetRandomFileName() + ".pcm")
    let getRecordPermission() = Permissions.RequestAsync<Permissions.Microphone>()        
    let createRecorder (model:Model) =
        task {
            let! permission = MainThread.InvokeOnMainThreadAsync<PermissionStatus>(fun () -> getRecordPermission())
            debug $"Permission: {permission}"
            if permission = PermissionStatus.Granted then
                let rcdr = Utils.audioManager.CreateRecorder(model.audioFormat)
                return Some rcdr
            else
                return None            
        }
        |> Async.AwaitTask
    let createPlayer (model:Model) = Utils.audioManager.CreatePlayer(model.audioFormat)
        
module Connect =
    let stateIcon model =
        if model.session = Unchecked.defaultof<_> then Icons.link_off
        else Icons.link
    let connecting model =
        match model.session with
        | Some sess -> sess.WsWrapper.websocket.State = Net.WebSockets.WebSocketState.Connecting
        | None -> false
        
    let promptKey() =
        task {
            let app = Application.Current
            let wins = app.Windows
            let page = wins.[0].Page
            let! result = page.DisplayPromptAsync("Key","Input OpenAI Real-time API key")
            match result with
            | null -> ()
            | s -> do! SecureStorage.Default.SetAsync(C.API_KEY,s)
        }
        
    let getKey() =
        task {
            let! key = SecureStorage.Default.GetAsync(C.API_KEY)
            match key with
            | null ->
                do! promptKey()
                let! key = SecureStorage.Default.GetAsync(C.API_KEY)
                return match key with null -> None | s -> Some s
            | s -> return Some s
        }
        |> Async.AwaitTask
        
    let startStopSession (model:Model) =
        async {
            match model.session with
            | None -> 
                match! getKey() with
                | None -> return failwith "No api key" 
                | Some key ->
                    match! Audio.createRecorder model with
                    | None -> return failwith "unable to create audio recorder"
                    | Some rcdr -> 
                        let player = Audio.createPlayer model 
                        let sess = Session.create key player.Channel rcdr.Channel
                        return (Some (sess,rcdr,player))
            | Some session ->
                do! Session.close session
                model.player |> Option.iter _.Stop()
                model.recorder |> Option.iter _.Stop()
                return None
        }
        
    let connect (sess:Session) =
        task{
            do! sess.Ws.ConnectAsync(Uri(Session.url), CancellationToken.None)
        }
  
module Update =
    let initModel () = 
        {
            audioFormat = AudioFormat.Default
            recorder = None
            session = None
            player = None
            mailbox = System.Threading.Channels.Channel.CreateBounded<Msg>(30)
            log = []
        },[]

    let update msg model =
        match msg with
        | Export -> model, Cmd.none
        | EventError exn -> debug exn.Message; {model with log=exn.Message::model.log}, Cmd.none
        | Log_Append s -> { model with log = s::model.log |> List.truncate C.MAX_LOG }, Cmd.none
        | Log_Clear -> { model with log = [] }, Cmd.none
        | Session_StartStop -> model, Cmd.OfAsync.either Connect.startStopSession model Session_Set EventError 
        | Session_Set (Some(sess,rcdr,plyr)) -> {model with session = Some sess; recorder = Some rcdr; player = Some plyr }, Cmd.ofMsg (Session_Connect sess)
        | Session_Set None -> {model with session = None; recorder = None; player = None }, Cmd.none
        | Session_Connect sess -> model, Cmd.OfTask.attempt Connect.connect sess EventError
        | Key_Set -> model, Cmd.OfTask.attempt Connect.promptKey () EventError
        | Key_Get -> model, Cmd.OfAsync.either Connect.getKey () Key_Value EventError
        | Key_Value str -> model, Cmd.none
        
