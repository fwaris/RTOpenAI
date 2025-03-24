namespace RTOpenAI.WebRTC.Windows
#if WINDOWS
open RTOpenAI.WebRTC
open System
open System.Threading
open SIPSorcery.Net
open System.Text.Json
open RTOpenAI.Api
open SIPSorceryMedia.Abstractions

module Connect =
    let createPeerConnection (config:RTCConfiguration) =
        try
            new RTCPeerConnection(config)
        with ex ->
            Log.exn(ex,"unable to create peer connection")
            raise ex
    
    let createAudioTrack(pc:RTCPeerConnection) = 
        let codec = new Opus.Maui.Graph()        
        let opus = ResizeArray [new AudioFormat(111, "OPUS", Opus.Maui.Graph.SampleRate, Opus.Maui.Graph.Channels, "useinbandfec=1")]
        let audioTrack = new MediaStreamTrack(opus, MediaStreamStatusEnum.SendRecv)
        pc.addTrack(audioTrack)
        codec.InitializeAsync().Wait()
        //Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(codec.MicGraph.InitializeAsync).Wait()
        pc.add_OnAudioFormatsNegotiated(fun afs -> Utils.debug $"audio format is {Seq.head afs}")
        pc.add_onconnectionstatechange(fun state -> 
            task {
                Utils.debug $"Peer connection state changed to {state}"
                Log.info $"Peer connection state changed to {state}"
                if not( state = RTCPeerConnectionState.connected || state = RTCPeerConnectionState.connecting) then 
                   codec.Dispose()
                elif state = RTCPeerConnectionState.connected then 
                    codec.Start(fun (duration,bytes) -> pc.SendAudio(duration,bytes))                    
            }
            |> ignore            
        )
        pc.add_OnRtpPacketReceived(fun ep media packet -> 
            if media = SDPMediaTypesEnum.audio then 
                let ph = packet.Header
                codec.DecodeAndPlay(packet.Payload)                
            )
        audioTrack,codec

    let createDataChannel (pc:RTCPeerConnection) =        
        try
            pc.createDataChannel(C.OPENAI_RT_DATA_CHANNEL).Result |> Some
        with ex -> 
            Log.exn (ex,"createDataChannel")
            None

    let createMediaSenders (pc:RTCPeerConnection)   = 
        let audioTrack = createAudioTrack pc
        let dataChannel = createDataChannel pc
        audioTrack,dataChannel


type WebRtcClientWin() = 
        
    let mutable peerConnection: RTCPeerConnection = null   
    let mutable dataChannel: RTCDataChannel = Unchecked.defaultof<_>
    let mutable outputChannel = Channels.Channel.CreateBounded<JsonDocument>(30)
    let mutable disposables : IDisposable list  = []
    let mutable state = Disconnected
    let stateEvent = Event<State>()

    let addDisposables xs = disposables <- disposables @ xs
    
    let setState s = state <- s; stateEvent.Trigger(s)

    let onMessage dataChannel payload (bytes:byte[]) = 
        let msg = JsonSerializer.Deserialize(bytes)
        outputChannel.Writer.TryWrite(msg) |> ignore

    member this.Init() =
        let pcConfig = RTCConfiguration(X_UseRtpFeedbackProfile = true)
        peerConnection <- Connect.createPeerConnection(pcConfig)
        let audioTrack,dataChannelOpt = Connect.createMediaSenders peerConnection
        peerConnection.add_OnTimeout(fun mediaType -> Log.info $"Timeout on media {mediaType}")
        match dataChannelOpt with 
        | Some dc ->
            dataChannel <- dc
            dc.add_onmessage(OnDataChannelMessageDelegate(onMessage))
        | None -> ()

    member this.SendOffer(ephemeralKey:string,url:string) =
        task {
            let offer = peerConnection.createOffer()
            do! peerConnection.setLocalDescription(offer)
            let! answer = Utils.getAnswerSdp ephemeralKey url offer.sdp
            let r = peerConnection.setRemoteDescription(RTCSessionDescriptionInit(
                                                        sdp=answer, ``type`` = RTCSdpType.answer))
            if r = SetDescriptionResultEnum.OK then 
                task {setState Connected} |> ignore
            else
                task {setState Disconnected} |> ignore
                Utils.logAndFail "timeout on setting remote sdp as answer"
        }   

    interface System.IDisposable with 
        member this.Dispose (): unit = 
            match peerConnection with 
            | null -> () 
            | x -> 
                x.Close("done")
                x.Dispose()
                peerConnection <- null

            if dataChannel <> Unchecked.defaultof<_> then 
                dataChannel.close()                
                dataChannel <- Unchecked.defaultof<_>
            if outputChannel <>  Unchecked.defaultof<_> then
                outputChannel.Writer.Complete()
                outputChannel <- Unchecked.defaultof<_>
            setState Disconnected            

    //Main cross-platform API
    interface IWebRtcClient with     
        member _.OutputChannel with get () = outputChannel
        member _.State with get() = state
        member _.StateChanged = stateEvent.Publish
                    
        member this.Connect (key,url) = 
            //let c = MainThread.InvokeOnMainThreadAsync<WebRTCWrapper.Class>(fun () ->WebRTCWrapper.Class()).Result
            //let p = c.MyProperty
            this.Init()
            setState Connecting
            this.SendOffer(key,url)

        member this.Send (data:string) =
            if dataChannel <> null then 
                let dataBytes : byte [] = Text.UTF8Encoding.Default.GetBytes data
                dataChannel.send(dataBytes)
                true
            else
                false
#endif
