namespace RTOpenAI
open System
open System.Net.WebSockets

module Machine =

    let defaultMachine log dispatch (session: Session, event: Events.ServerEvent) : Session =
        log event
        match event with
        | Events.ServerEvent.Error e ->
            // Handle error event (you might want to log the error or update the session state)
            session

        | Events.ServerEvent.SessionCreated e -> 
            dispatch SessionCreated
            {session with RTSession=e.session}

        | Events.ServerEvent.SessionUpdated e ->            
            // Update session with updated session data
            let rtSession = 
                {session.RTSession with
                    model = e.session.model |> Option.orElse session.RTSession.model
                    modalities = e.session.modalities
                    instructions = e.session.instructions |> Option.orElse session.RTSession.instructions
                    voice = e.session.voice |> Option.orElse session.RTSession.voice
                    input_audio_format = e.session.input_audio_format |> Option.orElse session.RTSession.input_audio_format
                    output_audio_format = e.session.output_audio_format |> Option.orElse session.RTSession.output_audio_format
                    input_audio_transcription = e.session.input_audio_transcription |> Option.orElse session.RTSession.input_audio_transcription
                    turn_detection = e.session.turn_detection |> Option.orElse session.RTSession.turn_detection
                }
            {session with RTSession=rtSession}

        | Events.ServerEvent.ConversationCreated e ->
            session

        | Events.ServerEvent.ConversationItemCreated e ->
            {session with Conversation = Conversation.insertItem e session.Conversation}

        | Events.ServerEvent.ConversationItemInputAudioTranscriptionCompleted e ->
            {session with Conversation=Conversation.updateAudioTranscription e session.Conversation}

        | Events.ServerEvent.ConversationItemInputAudioTranscriptionFailed e ->
            {session with Conversation=[]}

        | Events.ServerEvent.ConversationItemTruncated e ->
            session

        | Events.ServerEvent.ConversationItemDeleted e ->
            {session with Conversation = Conversation.deleteItem e session.Conversation}

        | Events.ServerEvent.InputAudioBufferCommitted e ->
            session

        | Events.ServerEvent.InputAudioBufferCleared e ->
            session

        | Events.ServerEvent.InputAudioBufferSpeechStarted e ->
            session

        | Events.ServerEvent.InputAudioBufferSpeechStopped e ->
            session

        | Events.ServerEvent.ResponseCreated e ->
            session

        | Events.ServerEvent.ResponseDone e ->
            session

        | Events.ServerEvent.ResponseOutputItemAdded e ->
            session

        | Events.ServerEvent.ResponseOutputItemDone e ->
            session

        | Events.ServerEvent.ResponseContentPartAdded e ->
            session

        | Events.ServerEvent.ResponseContentPartDone e ->
            session

        | Events.ServerEvent.ResponseTextDelta e ->
            session

        | Events.ServerEvent.ResponseTextDone e ->
            session

        | Events.ServerEvent.ResponseAudioDelta e ->
            session

        | Events.ServerEvent.ResponseAudioDone e ->
            session

        | Events.ServerEvent.ResponseAudioTranscriptDelta e ->
            session

        | Events.ServerEvent.ResponseAudioTranscriptDone e ->
            session

        | Events.ServerEvent.ResponseFunctionCallArgumentsDelta e ->
            session

        | Events.ServerEvent.ResponseFunctionCallArgumentsDone e ->
            session

        | Events.ServerEvent.RateLimitsUpdated e ->
            session

    let run (state:Ref<Session>) machine dispatch = 
        let comp = 
            async {
                while state.Value.Ws.State = WebSocketState.Open do
                    let! s' = Session.nextEvent state.Value.Ws state.Value machine
                    state.Value <- s'
            }
        async {
            match! Async.Catch(comp) with
            | Choice1Of2 _ -> dispatch SessionEnded
            | Choice2Of2 ex -> dispatch (EventError ex)
        }
        |> Async.Start
        