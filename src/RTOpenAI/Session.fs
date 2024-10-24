namespace RTOpenAI
open System
open System.Buffers
open System.Threading
open System.Net.WebSockets

type Session = {
        Ws : ClientWebSocket
        RTSession: Events.Session
        Conversation: Events.ConversationItem list
        InputAudioBuffer: int16[] list
        Response: Events.ServerEvent list
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

    let create key =
        async {
            let uri = new Uri(url)        
            let ws = new ClientWebSocket()
            ws.Options.SetRequestHeader("Authorization", $"Bearer {key}")
            ws.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1")
            do! ws.ConnectAsync(uri, CancellationToken.None) |> Async.AwaitTask
            return 
                {
                    Ws=ws 
                    RTSession=Events.Session.Default
                    Conversation=[]
                    Response=[]
                    InputAudioBuffer=[]
                }
        }

    let nextEvent (ws:ClientWebSocket) (state:'t) (stateMachine:('t * Events.ServerEvent) -> 't) =
      async {
            let! message = Pipe.receiveEvent ws
            return stateMachine (state, message)
        }
