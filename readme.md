# RTOpenAI
A library to build mobile applications in F# / MAUI, which utilizes the
OpenAI realtime API via the WebRTC protocol.

### Overview
The [OpenAI realtime API](https://openai.com/index/introducing-the-realtime-api/) enables voice chats against the 'realtime' version of GPT.
It's ideal for building voice-enabled chat assistants, mainly due to the following features:

- Natural assistant voice:
  - With intonation, pauses and even an occasional stutter!
  - Interruptible - carry natural, human-like conversations. VAD (voice activity detection) means that the user can start speaking at any time to provide input. 
  - Multiple voices available 
- Speech tone understanding - realtime model receives the speaker's voice embeddings as input and so can detect tone (other features of the voice) and incorporate these in its responses.
- Fast responses - realtime version of GPT is tuned for faster responses than the reqular version

This repo is organized as follows:
- RTOpenAI.Api
  - The core library for connecting to the realtime API via the WebRTC protocol.
- WebRTCme.Bindings.Maui.{Android | iOS}
  - WebRTC bindings for Maui apps referenced by RTOpenAI.Api
  - Support multiple, bidirectional, streaming channels: 1) for audio input/output (mic/speaker) and 2) for data
  - Libs connect audio hardware directly to realtime API, via WebRTC
  - *WebRTC vs. Web Sockets*: Both are supported by OpenAI for realtime. 
WebRTC is resilient to minor network issues at the protocol level whereas Web Sockets requires application level
handling for the same. WebRTC sends audio as compressed binary; Web Sockets sends it as raw, binary-encoded strings.
All things considered, WebRTC is the more suited protocol for voice chat applications running on mobile devices.
- Samples
  - RTOpenAI.Sample
    - A minimal sample for voice chatting via the realtime API (no function calling) 
  - RT.Assistant 
    - A voice assistant for question-answering 
    - Showcases function calling
    - More details [here](src/Samples/PlanSelection/PlanAssistant/readme.md)
  - Note: The sample UIs are designed to enhance the visibility and transparency of the underlying activity and data. 
While these are not fully polished applications, the underlying Fabulous/MAUI technology 
is capable of creating sophisticated mobile applications.   

### Challenges 
Building against the realtime API is very different from building traditional chat applications.
There is a steady flow of events from the server. Not all events need to be explicitly handled, but a different, 
'stateful', architecture/approach is needed here.

The RTOpenAI.Api library surfaces the server events to the consumer application via a dotnet 'channel'. 
It also servers as a conduit for sending client events to the server.
The server and client events are available as strongly-typed F# discriminated unions structures to ease event handling.

Note that audio from the server is automatically routed to the default output audio device (speaker/headset). 
Similarly, the recorded audio is sent directly to the server. All this is done by the underlying WebRTC libs.
The client application is notified of these actions in detail via server messages.

### Architecture / Approach
The stateful nature of the realtime interaction is handled by an **asynchronous state machine** pattern, which is quite easy to implement in the F# language.

The general flow is as follows:
- Attach to the event channel where server events are being thrown, by castings it to an AsyncSeq (from IAsyncEnumerable)  
- Use AsyncSeq.scanAsync with a start state and the state update function
- The update function generally uses F# pattern matching over server events to take action (or to ignore events)
- The update function can send clients events, UI events and/or update the application state as required.
- Iter over all events until the channel disconnects.

The following F# snippet depicts this flow:
```F#
module Machine =
 
    // accepts old state and next event - return new state; optionally dispatch UI messages or client events to server
    let update dispatch conn (st:State) ev =
        async {
            match ev with
            | SessionCreated s when not st.initialized -> sendUpdateSession conn s.session; return {st with initialized = true} 
            | SessionUpdated s -> return {st with currentSession = s.session }
            | ResponseOutputItemDone ev when isFunctionCall ev  -> callFunction ev; return st            
            | ResponseOutputItemDone ev when isFunctionResult ev  -> dispatch (extractUIInfo ev); return st
            | ...            
            | other -> (* Log.info $"unhandled event: {other}"; *) return st //log other events
        }
        
    //continuously process server events
    let run (conn:RTOpenAI.Api.Connection) dispatch =
        let comp = 
            conn.WebRtcClient.OutputChannel.Reader.ReadAllAsync()
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.map Api.Exts.toEvent //convert raw json to typed server event
            |> AsyncSeq.scanAsync (update dispatch conn) ssInit   //handle actual event (above), ssInit = initial state
            |> AsyncSeq.iter (fun s -> ())
        async {
            match! Async.Catch comp with
            | Choice1Of2 _ -> Log.info "server events completed"
            | Choice2Of2 exn -> Log.exn(exn,"Error: Machine.run")
        }
        |> Async.Start
```

Many variations of this approach are possible. For instance, the server event channel can be 
combined with a local application event channel. This allows the update function to handle 
both types of events, enabling long-running processes or workflows, replete with timeout and exception
handling. By centralizing workflow event tracking within the update function, race conditions 
are eliminated, and overall complexity is reduced.

### Build Notes
Get set up for [Maui development](https://learn.microsoft.com/en-us/dotnet/maui/?view=net-maui-9.0).
VS Code does not yet work for F#/Maui so need either Visual Studio or JetBrains Rider.
Most of the development was done on MacOS with JetBrains Rider (EAP version).

The solution works for Android, iOS and MacOS (maccatalyst). Windows requires a WebRTC implementation. 
WebRTC for Windows is available from [SipSorcery](https://www.nuget.org/packages/SIPSorcery) 
but it has not been integrated yet.

### Acknowledgements

- Tim Lariviere for [Fabulous Maui](https://github.com/fabulous-dev/Fabulous.MauiControls)  
- [WebRTCme](https://github.com/melihercan/WebRTCme) - provided the base bindings for Maui WebRTC. These where modified (significantly for IOS) to make them work for RTOpenAI.
- [Tau Prolog](http://tau-prolog.org/) - a javascript Prolog interpreter used in the function calling sample RT.Assistant. 
