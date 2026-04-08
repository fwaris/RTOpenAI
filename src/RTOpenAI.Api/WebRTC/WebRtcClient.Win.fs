namespace RTOpenAI.WebRTC.Win
#if WINDOWS
open RTOpenAI.WebRTC
open System
open System.Threading
open System.Threading.Tasks
open SIPSorcery.Net
open System.Text.Json
open RTOpenAI.Api
open SIPSorcery.Media
open SIPSorceryMedia.Windows
open SIPSorceryMedia.Abstractions
open Concentus
open Concentus.Enums

type WebRtcClientWin() = 
        
    let mutable peerConnection: RTCPeerConnection = null   
    let mutable dataChannel: RTCDataChannel = Unchecked.defaultof<_>
    let mutable outputChannel = Channels.Channel.CreateBounded<JsonDocument>(30)
    let mutable disposables : IDisposable list  = []
    let mutable state = Disconnected
    let stateEvent = Event<State>()
    let mutable iceGatheringCompleted = TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let addDisposables xs = disposables <- disposables @ xs
    
    let setState s = state <- s; stateEvent.Trigger(s)
    let resetIceGathering() =
        iceGatheringCompleted <- TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let tryGetLocalOfferSdp() =
        match peerConnection with
        | null -> None
        | pc when isNull pc.localDescription -> None
        | pc ->
            let sdp = pc.localDescription.sdp.ToString()

            if String.IsNullOrWhiteSpace sdp then
                None
            else
                Some sdp

    let hasLocalCandidates() =
        tryGetLocalOfferSdp()
        |> Option.exists (fun sdp ->
            sdp.Contains("a=candidate:")
            || sdp.Contains("a=end-of-candidates"))

    let waitForLocalCandidatesAsync timeoutMs =
        task {
            if hasLocalCandidates() then
                ()
            else
                use timeout = new CancellationTokenSource(millisecondsDelay = timeoutMs)

                try
                    do! iceGatheringCompleted.Task.WaitAsync(timeout.Token)
                with
                | :? OperationCanceledException -> ()

            if hasLocalCandidates() then
                Log.info "pc: local ICE candidates gathered for offer"
            else
                Log.warn $"pc: continuing without local ICE candidates after waiting {timeoutMs}ms"
        }

    let onMessage dataChannel payload (bytes:byte[]) = 
        let msg = JsonSerializer.Deserialize(bytes)
        outputChannel.Writer.TryWrite(msg) |> ignore

    let createIceServers (clientConfig: WebRtcClientConfig) =
        clientConfig.IceServerUrls
        |> List.map (fun url ->
            let iceServer = RTCIceServer()
            iceServer.urls <- url
            iceServer)

    let audioDelegate (pc:RTCPeerConnection) = EncodedSampleDelegate(fun duration bytes -> pc.SendAudio(duration,bytes))

    let addAudio(pc:RTCPeerConnection) =
        let audioEncoder = new AudioEncoder(AudioCommonlyUsedFormats.OpusWebRTC);
        let winAudioEP = WindowsAudioEndPoint(audioEncoder)
        winAudioEP.add_OnAudioSinkError(SourceErrorDelegate(fun err -> Log.info $"Audio sink error {err}"))
        let audioDlg = audioDelegate pc
        winAudioEP.add_OnAudioSourceEncodedSample(audioDlg)
        let audioTrack = new MediaStreamTrack(AudioCommonlyUsedFormats.OpusWebRTC, MediaStreamStatusEnum.SendRecv)
        pc.addTrack(audioTrack)
        pc.add_OnAudioFormatsNegotiated(fun afs -> 
            let af = Seq.head afs
            Log.info $"Audio format negotiated {af.FormatName} channels:{af.ChannelCount} rate:{af.ClockRate} rtpRate:{af.RtpClockRate} {af.FormatID}."
            winAudioEP.SetAudioSinkFormat(af) 
            winAudioEP.SetAudioSourceFormat(af)
        )
        winAudioEP

    let hookConnectionHandling (winAudioEP:WindowsAudioEndPoint) (pc:RTCPeerConnection) = 
        pc.add_oniceconnectionstatechange(fun state -> Log.info $"ICE connection state changed to {state}")
        pc.add_onconnectionstatechange(fun state ->             
            task {
                Log.info $"Peer connection state changed to {state}"
                if state = RTCPeerConnectionState.connected then 
                    do! winAudioEP.StartAudio()
                    do! winAudioEP.StartAudioSink()
                    do! winAudioEP.Start()
                elif state = RTCPeerConnectionState.closed || state = RTCPeerConnectionState.disconnected then
                    do! winAudioEP.Close()
                }
                |> ignore)

    let hookAudioStream (winAudioEP:WindowsAudioEndPoint) (pc:RTCPeerConnection) =
        //pc.add_OnAudioFrameReceived(fun audioFrame -> winAudioEP.GotEncodedMediaFrame(audioFrame))
        pc.add_OnAudioFrameReceived(Action<EncodedAudioFrame>(winAudioEP.GotEncodedMediaFrame))

    let createPcConnection (clientConfig: WebRtcClientConfig) = 
        task {
            let pcConfig =
                RTCConfiguration(
                    X_UseRtpFeedbackProfile = true,
                    X_ICEIncludeAllInterfaceAddresses = clientConfig.BindAddress.IsNone
                )

            pcConfig.iceServers <- System.Collections.Generic.List<RTCIceServer>(createIceServers clientConfig)

            match clientConfig.BindAddress with
            | Some address ->
                pcConfig.X_BindAddress <- address
                Log.info $"pc: using bind address {address}"
            | None -> ()

            let pc = new RTCPeerConnection(pcConfig)
            let! dataChannel = pc.createDataChannel(Env.OPENAI_RT_DATA_CHANNEL.Value)
            let winAudioEP = addAudio pc
            pc.add_OnTimeout(fun mediaType -> Log.info $"Timeout on media {mediaType}")
            pc.add_onicegatheringstatechange(fun state ->
                Log.info $"ICE gathering state changed to {state}"

                if state = RTCIceGatheringState.complete then
                    iceGatheringCompleted.TrySetResult() |> ignore)
            hookConnectionHandling winAudioEP pc
            hookAudioStream winAudioEP pc
            return pc,dataChannel,winAudioEP
        }


    member this.Init (clientConfig: WebRtcClientConfig) =               
        task {
            resetIceGathering()
            let! pc,dc,winAudioEP = createPcConnection clientConfig
            peerConnection <- pc
            dataChannel <- dc
            dataChannel.add_onmessage(OnDataChannelMessageDelegate(onMessage))
        }

    member this.SendOffer(ephemeralKey:string,url:string,clientConfig: WebRtcClientConfig) =
        task {
            let offer = peerConnection.createOffer()
            do! peerConnection.setLocalDescription(offer)
            do! waitForLocalCandidatesAsync clientConfig.IceGatherTimeoutMs

            match tryGetLocalOfferSdp() with
            | Some offerSdp ->
                let! answer = Utils.getAnswerSdp ephemeralKey url offerSdp
                let r =
                    peerConnection.setRemoteDescription(
                        RTCSessionDescriptionInit(sdp = answer, ``type`` = RTCSdpType.answer)
                    )

                if r = SetDescriptionResultEnum.OK then 
                    task {setState Connected} |> ignore
                else
                    task {setState Disconnected} |> ignore
                    Utils.logAndFail "timeout on setting remote sdp as answer"
            | None ->
                task {setState Disconnected} |> ignore
                Utils.logAndFail "local offer SDP was empty after ICE gathering"
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
                    
        member this.Connect (key,url,config) = 
            task {
                try
                    let config = WebRtcClientConfigHelpers.normalize config
                    do! this.Init(config)
                    setState Connecting
                    do!this.SendOffer(key,url,config)
                with ex ->
                    Log.exn (ex,"IWebRtcClient.Connect")
            }

        member this.Send (data:string) =
            if dataChannel <> null then 
                let dataBytes : byte [] = Text.UTF8Encoding.Default.GetBytes data
                dataChannel.send(dataBytes)
                true
            else
                false
#endif
