namespace RTOpenAI.WebRTC.Windows
#if WINDOWS
open RTOpenAI.WebRTC
open System
open System.Net
open System.Threading
open SIPSorcery.Media
open SIPSorcery.Net
open SIPSorceryMedia.Windows
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

        // Sink (speaker) only audio end point.
        let windowsAudioEP = new WindowsAudioEndPoint(new AudioEncoder(includeOpus = true), -1, -1, false, false);
        windowsAudioEP.RestrictFormats(fun x -> x.FormatName = "OPUS");
        let hErr = SourceErrorDelegate(fun err -> Utils.debug $"audio error :{err}"; Log.warn $"****Audio sink error. {err}.")
        windowsAudioEP.add_OnAudioSinkError(hErr)
        let sendAudio duration sample = Utils.debug $"dur:{duration}"; pc.SendAudio(duration,sample)        
        windowsAudioEP.add_OnAudioSourceEncodedSample(EncodedSampleDelegate(sendAudio))
        let audioTrack = new MediaStreamTrack(windowsAudioEP.GetAudioSourceFormats(), MediaStreamStatusEnum.SendRecv)
        pc.addTrack(audioTrack)
        pc.add_OnAudioFormatsNegotiated(fun audiFormats -> 
            let topFormat = Seq.head audiFormats
            Log.info $"Audio format negotiated {topFormat.FormatName}"
            windowsAudioEP.SetAudioSinkFormat(topFormat)
            windowsAudioEP.SetAudioSourceFormat(topFormat)
        )
        pc.add_onconnectionstatechange(fun state -> 
            task {
                Utils.debug $"Peer connection state changed to {state}"
                Log.info $"Peer connection state changed to {state}"
                if state = RTCPeerConnectionState.connected then 
                    do! windowsAudioEP.StartAudio()
                    do! windowsAudioEP.StartAudioSink()
                else
                    do! windowsAudioEP.CloseAudio();
            }
            |> ignore            
        )
        pc.add_OnRtpPacketReceived(fun ep media packet -> 
            if media = SDPMediaTypesEnum.audio then 
                let ph = packet.Header
                //Utils.debug $"{ph.SequenceNumber}; {ph.PayloadType}"
                windowsAudioEP.GotAudioRtp(
                    ep,
                    ph.SyncSource,
                    uint32 ph.SequenceNumber,
                    ph.Timestamp,
                    ph.PayloadType,
                    ph.MarkerBit = 1,
                    packet.Payload)
            )
        audioTrack

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