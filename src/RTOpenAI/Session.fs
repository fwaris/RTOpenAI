namespace RTOpenAI
open System
open System.Threading
open System.Threading.Channels
open System.Net.WebSockets
open FSharp.Control.Websockets
open FSharp.Control
open RTOpenAI.Events

type Session = {
        WsWrapper : ThreadSafeWebSocket.ThreadSafeWebSocket
        Ws : ClientWebSocket
        RTSession: Events.Session
        Conversation: Events.ConversationItem list
        InputAudioBuffer: int16[] list
        Response: Events.ServerEvent list
        AudioInput : Channel<byte[]>
        AudioOutput : Channel<byte[]>
    }

module ContentItem = 

    let update 
        (at:int) 
        (func:Events.ConversationItemContent -> Events.ConversationItemContent) 
        (contents:Events.ConversationItemContent list) 
        =
        let content = contents |> List.tryItem at |> Option.defaultValue (failwith $"content index {at} not found")
        contents |> List.updateAt at (func content)

    let updateTranscription transcript (ci:Events.ConversationItemContent) = {ci with transcript = Some transcript}

    let truncate (contents:Events.ConversationItemContent list) (new_length:int) = contents |> List.truncate new_length

module Conversation = 

    let update  
        (at:string) 
        (func:Events.ConversationItem->Events.ConversationItem)
        (conversation:Events.ConversationItem list) = 
        conversation 
        |> List.tryFindIndex (fun x -> x.id = at)
        |> Option.map (fun i -> conversation |> List.updateAt i (func conversation.[i]))
        |> Option.defaultWith (fun _ -> failwith $"conversation item with {at} not found")

    let updateAudioTranscription 
            (item:Events.ConversationItemInputAudioTranscriptionCompletedEvent) 
            (conversation:Events.ConversationItem list)
            = 
            conversation 
            |> update item.item_id (fun co -> 
                {co with
                    content = 
                        co.content 
                        |> ContentItem.update 
                            item.content_index 
                            (ContentItem.updateTranscription item.transcript)
                })

    let insertItem 
        (item:Events.ConversationItemCreatedEvent) 
        (conversation:Events.ConversationItem list) 
        = 
        conversation |> List.tryFindIndexBack (fun x -> (Some x.id) = item.previous_item_id)
        |> Option.map (fun i -> conversation |> List.insertAt (i+1) item.item)
        |> Option.defaultValue (conversation |> List.append [item.item])

    let deleteItem 
        (item:Events.ConversationItemDeletedEvent) 
        (conversation:Events.ConversationItem list) 
        = 
        conversation |> List.filter(fun y -> y.id <> item.item_id)

module Session =
    let url = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";

    let rec submitLoop<'t> (caller:string) c t : Async<'t> =
        async {
            try
                return! t
            with ex ->
                printfn $"{ex.Message}"
                if c < 5 then
                    do! Async.Sleep 1000
                    return! submitLoop caller (c+1) t
                else
                    return raise ex
        }

    let create key audioInput audioOutput =
        let uri = new Uri(url)        
        let ws = new ClientWebSocket()
        ws.Options.SetRequestHeader("Authorization", $"Bearer {key}")
        ws.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1")  
        {
            WsWrapper=ThreadSafeWebSocket.createFromWebSocket(ws)
            Ws = ws
            RTSession=Events.Session.Default
            Conversation=[]
            Response=[]
            InputAudioBuffer=[]
            AudioInput = audioInput
            AudioOutput  = audioOutput
        }
                
    let close (sess:Session) =
        sess.WsWrapper.websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None)
        |> Async.AwaitTask

    let nextEvent (ws:ThreadSafeWebSocket.ThreadSafeWebSocket) (state:'t) (stateMachine:('t * Events.ServerEvent) -> 't) =
      async {
            let! result = Pipe.receiveEvent ws
            return
                match result with
                | Ok (Some message) -> stateMachine (state, message)
                | Ok None -> failwith "websocket closed"
                | Result.Error x -> raise x.SourceException
        }

    type SessionEvent = SE_Server of ServerEvent | SE_InputAudio of byte[] | SE_Tick of DateTime
    let serverEvents (ws:ThreadSafeWebSocket.ThreadSafeWebSocket) =
        asyncSeq {
            let mutable go = true
            while go do 
                match! Pipe.receiveEvent ws with
                | Result.Ok (Some message) -> yield (SE_Server message)
                | Result.Ok None -> go <- false
                | Result.Error x -> raise x.SourceException
        }
        
    let micEvents (audio:Channel<byte[]>) = audio.Reader.ReadAllAsync() |> AsyncSeq.ofAsyncEnum |> AsyncSeq.map SE_InputAudio
    
    let ticks() = AsyncSeq.intervalMs 1000 |> AsyncSeq.map SE_Tick
    
