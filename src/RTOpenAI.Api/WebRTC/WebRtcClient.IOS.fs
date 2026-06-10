namespace RTOpenAI.WebRTC.IOS
#if IOS || MACCATALYST
open System
open System.Threading
open System.Threading.Tasks
open RTOpenAI.Api
open RTOpenAI.WebRTC
open IOS.WebRTC
open Foundation
open AVFoundation
open ObjCRuntime
open System.Text.Json

module Connect =
    let private createIceServers (clientConfig: WebRtcClientConfig) =
        clientConfig.IceServerUrls
        |> List.map (fun url -> new RTCIceServer([| url |]))
        |> List.toArray

    let rtcConfiguration (clientConfig: WebRtcClientConfig) =
        let config = new RTCConfiguration()
        config.SdpSemantics <- RTCSdpSemantics.UnifiedPlan
        config.IceServers <- createIceServers clientConfig
        config

    let createPeerConnection (fac: RTCPeerConnectionFactory) config constraints dlg =
        try
            fac.PeerConnectionWithConfiguration(config, constraints, dlg)
        with ex ->
            Log.exn (ex, "unable to create peer connection")
            raise ex

    let createAudioTrack (fac: RTCPeerConnectionFactory) =
        let audioConstraints = new RTCMediaConstraints(null, null)
        let audioSource = fac.AudioSourceWithConstraints(audioConstraints)
        let audioTrack = fac.AudioTrackWithSource(audioSource, trackId = "audio0")
        audioSource, audioTrack

    let createDataChannel (pc: RTCPeerConnection) =
        let config = new RTCDataChannelConfiguration()

        try
            pc.DataChannelForLabel(Env.OPENAI_RT_DATA_CHANNEL.Value, config) |> Some
        with ex ->
            Log.exn (ex, "createDataChannel")
            None

    let createMediaSenders fac (pc: RTCPeerConnection) =
        let audioSource, audioTrack = createAudioTrack fac
        let dataChannel = createDataChannel pc
        audioSource, audioTrack, dataChannel

    let mediaConstraints () =
        let mandatory =
            [ "OfferToReceiveAudio" :> obj, "true" :> obj; "OfferToReceiveVideo", "false" ]
            |> dict

        let optional = dict [ "DtlsSrtpKeyAgreement" :> obj, "true" :> obj ]

        let nMandatory =
            NSDictionary<NSString, NSString>
                .FromObjectsAndKeys(Seq.toArray mandatory.Values, Seq.toArray mandatory.Keys)

        let nOptional =
            NSDictionary<NSString, NSString>.FromObjectsAndKeys(Seq.toArray optional.Values, Seq.toArray optional.Keys)

        new RTCMediaConstraints(null, nOptional)



module AudioUtils =
    let private routeExperimentName =
        "variant6-videochat-speaker-override-output-gain-065"

    let private audioSessionSettleDelayMs = 250

    let private speakerphoneIncomingAudioVolume = 0.65

    let private errorMessage (error: NSError) =
        if isNull (box error) then
            "unknown error"
        else
            error.LocalizedDescription

    let private describeRoute (route: AVAudioSessionRouteDescription) =
        if isNull (box route) then
            "route=unknown"
        else
            let inputs =
                route.Inputs
                |> Seq.map (fun port -> $"{port.PortType}:{port.PortName}")
                |> String.concat ", "

            let outputs =
                route.Outputs
                |> Seq.map (fun port -> $"{port.PortType}:{port.PortName}")
                |> String.concat ", "

            $"inputs=[{inputs}]; outputs=[{outputs}]"

    let private requireSessionCall src succeeded error =
        if not succeeded then
            let msg = $"error with recording session. Src {src}; Error {errorMessage error}"
            failwith msg

    let private warnSessionCall src succeeded error =
        if not succeeded then
            Log.warn $"error with recording session. Src {src}; Error {errorMessage error}"

    let private categoryOptionsForPolicy policy =
        match policy with
        | IosAudioRoutePolicy.Speakerphone ->
            AVAudioSessionCategoryOptions.AllowBluetooth
            ||| AVAudioSessionCategoryOptions.DefaultToSpeaker
        | IosAudioRoutePolicy.ReceiverOrHeadset -> AVAudioSessionCategoryOptions.AllowBluetooth

    let private policyName policy =
        match policy with
        | IosAudioRoutePolicy.Speakerphone -> "speakerphone"
        | IosAudioRoutePolicy.ReceiverOrHeadset -> "receiver-or-headset"

    let private incomingAudioVolume policy =
        match policy with
        | IosAudioRoutePolicy.Speakerphone -> speakerphoneIncomingAudioVolume
        | IosAudioRoutePolicy.ReceiverOrHeadset -> 1.0

    let private isPreferredHeadsetPort (port: AVAudioSessionPortDescription) =
        let portType = string port.PortType

        [ "Bluetooth"; "Headset"; "Headphones"; "HearingAid"; "USBAudio"; "CarAudio" ]
        |> List.exists (fun marker -> portType.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)

    let private routeHasPreferredHeadset (route: AVAudioSessionRouteDescription) =
        if isNull (box route) then
            false
        else
            (route.Inputs |> Seq.exists isPreferredHeadsetPort)
            || (route.Outputs |> Seq.exists isPreferredHeadsetPort)

    let private isBuiltInSpeakerPort (port: AVAudioSessionPortDescription) =
        let portType = string port.PortType
        let speakerPort = string AVAudioSession.PortBuiltInSpeaker

        portType.Equals(speakerPort, StringComparison.OrdinalIgnoreCase)
        || portType.IndexOf("Speaker", StringComparison.OrdinalIgnoreCase) >= 0

    let private routeOutputsBuiltInSpeaker (route: AVAudioSessionRouteDescription) =
        if isNull (box route) then
            false
        else
            route.Outputs |> Seq.exists isBuiltInSpeakerPort

    let private portOverrideForPolicy (audioSession: RTCAudioSession) policy =
        let route = audioSession.CurrentRoute
        let headsetRoutePreferred = routeHasPreferredHeadset route
        let outputAlreadySpeaker = routeOutputsBuiltInSpeaker route

        match policy with
        | IosAudioRoutePolicy.Speakerphone when headsetRoutePreferred ->
            AVAudioSessionPortOverride.None, headsetRoutePreferred, outputAlreadySpeaker, "headset route preferred"
        | IosAudioRoutePolicy.Speakerphone when outputAlreadySpeaker ->
            AVAudioSessionPortOverride.None, headsetRoutePreferred, outputAlreadySpeaker, "defaultToSpeaker route"
        | IosAudioRoutePolicy.Speakerphone ->
            AVAudioSessionPortOverride.Speaker,
            headsetRoutePreferred,
            outputAlreadySpeaker,
            "speaker setting fallback override"
        | IosAudioRoutePolicy.ReceiverOrHeadset ->
            AVAudioSessionPortOverride.None, headsetRoutePreferred, outputAlreadySpeaker, "receiver/headset policy"

    let private describeAudioSession (audioSession: RTCAudioSession) =
        $"active={audioSession.IsActive}; category={audioSession.Category}; mode={audioSession.Mode}; outputVolume={audioSession.OutputVolume}; {describeRoute audioSession.CurrentRoute}"

    let private logAudioSessionState phase routePolicy (audioSession: RTCAudioSession) =
        Log.info
            $"audio session {phase}: experiment={routeExperimentName}; routePolicy={policyName routePolicy}; {describeAudioSession audioSession}"

    let private clearOutputAudioPortOverride src (audioSession: RTCAudioSession) =
        use mutable error = null

        let overrideCleared =
            audioSession.OverrideOutputAudioPort(AVAudioSessionPortOverride.None, &error)

        warnSessionCall src overrideCleared error

    let release (clientConfig: WebRtcClientConfig) =
        let audioSession = RTCAudioSession.SharedInstance()
        let routePolicy = clientConfig.IosAudioRoutePolicy

        audioSession.LockForConfiguration()

        try
            logAudioSessionState "teardown begin" routePolicy audioSession
            clearOutputAudioPortOverride "clear output audio port override before release" audioSession

            use mutable error = null
            let audioSessionDeactivated = audioSession.SetActive(false, &error)
            warnSessionCall "deactivate WebRTC audio session" audioSessionDeactivated error

            logAudioSessionState "teardown deactivate requested" routePolicy audioSession
        finally
            audioSession.UnlockForConfiguration()

        Thread.Sleep audioSessionSettleDelayMs
        logAudioSessionState $"teardown settled ({audioSessionSettleDelayMs}ms)" routePolicy audioSession

    let configureAudioSession (clientConfig: WebRtcClientConfig) =
        let audioSession = RTCAudioSession.SharedInstance()
        let routePolicy = clientConfig.IosAudioRoutePolicy
        let opts = categoryOptionsForPolicy routePolicy

        audioSession.LockForConfiguration()

        try
            logAudioSessionState "configure begin" routePolicy audioSession
            audioSession.IgnoresPreferredAttributeConfigurationErrors <- true

            use mutable error = null

            let categoryConfigured =
                audioSession.SetCategory(
                    AVAudioSession.CategoryPlayAndRecord,
                    AVAudioSession.ModeVideoChat,
                    opts,
                    &error
                )

            requireSessionCall "set WebRTC audio session category/mode" categoryConfigured error

            clearOutputAudioPortOverride "clear stale output audio port override before activation" audioSession

            error <- null
            let sampleRateConfigured = audioSession.SetPreferredSampleRate(48000., &error)
            warnSessionCall "set preferred sample rate" sampleRateConfigured error

            error <- null

            let bufferDurationConfigured =
                audioSession.SetPreferredIOBufferDuration(0.01, &error)

            warnSessionCall "set preferred IO buffer duration" bufferDurationConfigured error

            error <- null
            let audioSessionActivated = audioSession.SetActive(true, &error)
            requireSessionCall "activate WebRTC audio session" audioSessionActivated error

            logAudioSessionState "configured" routePolicy audioSession
        finally
            audioSession.UnlockForConfiguration()

    let configureIncomingAudio (clientConfig: WebRtcClientConfig) (audioTrack: RTCAudioTrack) =
        let audioSession = RTCAudioSession.SharedInstance()
        let routePolicy = clientConfig.IosAudioRoutePolicy
        let audioVolume = incomingAudioVolume routePolicy

        audioTrack.Source.Volume <- audioVolume
        audioTrack.IsEnabled <- true

        audioSession.LockForConfiguration()

        try
            use mutable error = null

            let portOverride, headsetRoutePreferred, outputAlreadySpeaker, routeReason =
                portOverrideForPolicy audioSession routePolicy

            if not (audioSession.OverrideOutputAudioPort(portOverride, &error)) then
                let msg =
                    $"error with recording session. Src port override; Error {errorMessage error}"

                failwith msg

            Log.info
                $"audio session route applied: experiment={routeExperimentName}; routePolicy={policyName routePolicy}; routeReason={routeReason}; headsetRoutePreferred={headsetRoutePreferred}; outputAlreadySpeaker={outputAlreadySpeaker}; portOverride={portOverride}; incomingAudioVolume={audioVolume}; category={audioSession.Category}; mode={audioSession.Mode}; outputVolume={audioSession.OutputVolume}; {describeRoute audioSession.CurrentRoute}"
        finally
            audioSession.UnlockForConfiguration()

//encapsulates peer connection for IOS
type WebRtcClientIOS() =
    inherit NSObject()
    //weak var delegate: WebRTCClientDelegate?
    let instanceId = Guid.NewGuid().ToString("N").Substring(0, 8)
    let mutable peerConnection: RTCPeerConnection = null
    let mutable peerConnectionFactory: RTCPeerConnectionFactory = null
    let mutable audioQueue = MailboxProcessor.Start(ignore >> async.Return)
    let mutable mediaConstraints = Connect.mediaConstraints ()
    let mutable audioSource: RTCAudioSource = null
    let mutable audioTrack: RTCAudioTrack = null
    let mutable audioSender: RTCRtpSender = null
    let mutable dataChannel: RTCDataChannel = Unchecked.defaultof<_>
    let mutable outputChannel = Channels.Channel.CreateBounded<JsonDocument>(30)
    let mutable state = Disconnected
    let mutable disposeStarted = 0
    let mutable activeClientConfig = WebRtcClientConfig.Default
    let mutable microphoneEnabled = true
    let stateEvent = Event<State>()

    let mutable iceGatheringCompleted =
        TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let setState s =
        state <- s
        stateEvent.Trigger(s)

    let resetIceGathering () =
        iceGatheringCompleted <- TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let tryCompleteIceGathering () =
        iceGatheringCompleted.TrySetResult() |> ignore

    let applyMicrophoneEnabled enabled =
        microphoneEnabled <- enabled

        match audioTrack with
        | null ->
            Log.info
                $"pc[{instanceId}]: microphone enabled requested before local audio track exists: enabled={enabled}"
        | track ->
            track.IsEnabled <- enabled
            Log.info $"pc[{instanceId}]: microphone enabled={enabled}"

    let describeNative name (value: obj) =
        match value with
        | null -> $"{name}=null"
        | :? INativeObject as native -> $"{name}={value.GetType().Name}(handle={native.Handle})"
        | _ -> $"{name}={value.GetType().Name}"

    let logCleanupStart disposing pc dc sender track source factory constraints =
        Log.info (
            $"pc[{instanceId}]: dispose start disposing={disposing}; "
            + String.concat
                "; "
                [ describeNative "peerConnection" pc
                  describeNative "dataChannel" dc
                  describeNative "audioSender" sender
                  describeNative "audioTrack" track
                  describeNative "audioSource" source
                  describeNative "peerConnectionFactory" factory
                  describeNative "mediaConstraints" constraints ]
        )

    let safeCleanup step action =
        try
            action ()
        with ex ->
            Log.exn (ex, $"pc[{instanceId}]: cleanup step '{step}' failed")

    let tryGetLocalOfferSdp () =
        match peerConnection with
        | null -> None
        | pc ->
            match pc.LocalDescription with
            | null -> None
            | description when String.IsNullOrWhiteSpace description.Sdp -> None
            | description -> Some description.Sdp

    let hasLocalCandidates () =
        tryGetLocalOfferSdp ()
        |> Option.exists (fun sdp -> sdp.Contains("a=candidate:") || sdp.Contains("a=end-of-candidates"))

    let waitForLocalCandidatesAsync timeoutMs =
        task {
            if
                hasLocalCandidates ()
                || (peerConnection <> null
                    && peerConnection.IceGatheringState = RTCIceGatheringState.Complete)
            then
                ()
            else
                use timeout = new CancellationTokenSource(millisecondsDelay = timeoutMs)

                try
                    do! iceGatheringCompleted.Task.WaitAsync(timeout.Token)
                with :? OperationCanceledException ->
                    ()

            if hasLocalCandidates () then
                Log.info "pc: local ICE candidates gathered for offer"
            else
                Log.warn $"pc: continuing without local ICE candidates after waiting {timeoutMs}ms"
        }

    override this.Dispose(disposing: bool) =
        let alreadyDisposing = Interlocked.Exchange(&disposeStarted, 1) <> 0

        if alreadyDisposing then
            Log.warn $"pc[{instanceId}]: duplicate dispose ignored; disposing={disposing}"
        else
            let pc = peerConnection
            let dc = dataChannel
            let sender = audioSender
            let track = audioTrack
            let source = audioSource
            let factory = peerConnectionFactory
            let constraints = mediaConstraints
            let channel = outputChannel

            peerConnection <- null
            dataChannel <- Unchecked.defaultof<_>
            audioSender <- null
            audioTrack <- null
            audioSource <- null
            peerConnectionFactory <- null
            mediaConstraints <- null
            outputChannel <- Unchecked.defaultof<_>

            logCleanupStart disposing pc dc sender track source factory constraints
            tryCompleteIceGathering ()

            if channel <> Unchecked.defaultof<_> then
                channel.Writer.TryComplete() |> ignore

            if disposing then
                if dc <> Unchecked.defaultof<_> then
                    safeCleanup "detach data channel delegate" (fun () -> dc.Delegate <- null)
                    safeCleanup "close data channel" (fun () -> dc.Close())
                    safeCleanup "dispose data channel" (fun () -> dc.Dispose())

                if sender <> null then
                    safeCleanup "dispose audio sender" (fun () -> sender.Dispose())

                if track <> null then
                    safeCleanup "dispose audio track" (fun () -> track.Dispose())

                if source <> null then
                    safeCleanup "dispose audio source" (fun () -> source.Dispose())

                if pc <> null then
                    safeCleanup "detach peer connection delegate" (fun () -> pc.Delegate <- null)
                    safeCleanup "close peer connection" (fun () -> pc.Close())
                    safeCleanup "dispose peer connection" (fun () -> pc.Dispose())

                if constraints <> null then
                    safeCleanup "dispose media constraints" (fun () -> constraints.Dispose())

                if factory <> null then
                    safeCleanup "dispose peer connection factory" (fun () -> factory.Dispose())

                safeCleanup "release audio session" (fun () -> AudioUtils.release activeClientConfig)
                setState Disconnected
                Log.info $"pc[{instanceId}]: dispose completed"
            else
                state <- Disconnected

                Log.warn (
                    $"pc[{instanceId}]: finalizer-driven Dispose(false) reached; "
                    + "native WebRTC teardown was skipped to avoid Objective-C cleanup on the finalizer thread."
                )

        base.Dispose(disposing)

    interface INativeObject with
        member this.Handle = base.Handle

    interface IDisposable with
        member this.Dispose() = base.Dispose()

    //initialize WebRTCClient
    member this.Init(clientConfig: WebRtcClientConfig) =
        activeClientConfig <- clientConfig
        resetIceGathering ()
        AudioUtils.configureAudioSession clientConfig
        let config = Connect.rtcConfiguration clientConfig
        peerConnectionFactory <- new RTCPeerConnectionFactory(null, null)
        peerConnection <- Connect.createPeerConnection peerConnectionFactory config mediaConstraints this

        let createdAudioSource, createdAudioTrack, _dataChannel =
            Connect.createMediaSenders peerConnectionFactory peerConnection

        audioSource <- createdAudioSource
        audioTrack <- createdAudioTrack
        applyMicrophoneEnabled microphoneEnabled
        audioSender <- peerConnection.AddTrack(audioTrack, streamIds = [| "stream0" |])

        match _dataChannel with
        | Some d ->
            dataChannel <- d
            dataChannel.Delegate <- this
        | None -> failwith "unable to create data channel"

        match clientConfig.BindAddress with
        | Some address ->
            Log.warn $"pc: bind address {address} was requested but is not supported on iOS/MacCatalyst; ignoring"
        | None -> ()

        let initDetails =
            [ describeNative "peerConnection" peerConnection
              describeNative "dataChannel" dataChannel
              describeNative "audioSender" audioSender
              describeNative "audioTrack" audioTrack
              describeNative "audioSource" audioSource ]
            |> String.concat "; "

        Log.info $"pc[{instanceId}]: initialized {initDetails}"

    member this.SendOffer(ephemeralKey: string, url: string, clientConfig: WebRtcClientConfig) =
        task {
            use sem = new ManualResetEvent(false)

            let completionHandler =
                RTCCreateSessionDescriptionCompletionHandler(fun sdp err ->
                    if sdp <> null then
                        peerConnection.SetLocalDescription(
                            sdp,
                            RTCSetSessionDescriptionCompletionHandler(fun err ->
                                if err <> null then
                                    Log.error ($"pc: error set local description {err.Description}")
                                else
                                    task {
                                        try
                                            do! waitForLocalCandidatesAsync clientConfig.IceGatherTimeoutMs

                                            match tryGetLocalOfferSdp () with
                                            | Some offerSdp ->
                                                Log.info $"pc: local sdp {offerSdp}"
                                                let! answer = Utils.getAnswerSdp ephemeralKey url offerSdp
                                                Log.info $"pc: remote sdp {answer}"

                                                let answerSdp =
                                                    new RTCSessionDescription(
                                                        ``type`` = RTCSdpType.Answer,
                                                        sdp = answer
                                                    )

                                                peerConnection.SetRemoteDescription(
                                                    answerSdp,
                                                    RTCSetSessionDescriptionCompletionHandler(fun err ->
                                                        if err <> null then
                                                            Log.error
                                                                $"pc: error setting remote description {err.Description}"

                                                        sem.Set() |> ignore)
                                                )
                                            | None ->
                                                Log.error "pc: local sdp missing after ICE gathering"
                                                sem.Set() |> ignore
                                        with ex ->
                                            Log.exn (ex, "pc: failed while exchanging SDP")
                                            sem.Set() |> ignore
                                    }
                                    |> ignore)
                        )

                    elif err <> null then
                        Log.error $"pc: sdp offer error {err.Description}"
                    else
                        Log.error "pc: unknown issue when setting local error")

            peerConnection.OfferForConstraints(mediaConstraints, completionHandler)
            let! r = Async.AwaitWaitHandle(sem, 30000)

            if r then
                task { setState Connected } |> ignore
                return ()
            else
                task { setState Disconnected } |> ignore
                let msg = "SDP answer timeout"
                Log.error msg
                return failwith msg
        }

    //Main cross-platform API
    interface IWebRtcClient with
        member _.OutputChannel = outputChannel
        member _.State = state
        member _.StateChanged = stateEvent.Publish

        member this.Connect(key, url, config) =
            let config = WebRtcClientConfigHelpers.normalize config
            this.Init(config)
            setState Connecting
            this.SendOffer(key, url, config)

        member this.Send(data: string) =
            if dataChannel <> null then
                use buffer = new RTCDataBuffer(data, isBinary = false)
                dataChannel.SendData(buffer)
            else
                false

        member _.SetMicrophoneEnabled(enabled: bool) = applyMicrophoneEnabled enabled

    interface IRTCPeerConnectionDelegate with
        member this.DidAddReceiver
            (peerConnection: RTCPeerConnection, rtpReceiver: RTCRtpReceiver, mediaStreams: RTCMediaStream array)
            : unit =
            Log.info $"pc: added receiver: {rtpReceiver.Description}"

        member this.DidAddStream(peerConnection: RTCPeerConnection, stream: RTCMediaStream) : unit =
            Log.info $"pc: added remote stream: {stream.Description}"

            stream.AudioTracks
            |> Seq.tryHead
            |> Option.map (AudioUtils.configureIncomingAudio activeClientConfig)
            |> Option.defaultWith (fun () -> Log.warn "No audio track in remote stream")

        member this.DidChangeConnectionState
            (peerConnection: RTCPeerConnection, newState: RTCPeerConnectionState)
            : unit =
            Log.info $"pc: connection state changed to %A{newState}"

            match newState with
            | RTCPeerConnectionState.Disconnected
            | RTCPeerConnectionState.Failed
            | RTCPeerConnectionState.Closed -> setState State.Disconnected
            | _ -> ()

        member this.DidChangeIceConnectionState
            (peerConnection: RTCPeerConnection, newState: RTCIceConnectionState)
            : unit =
            Log.info $"pc: ice connection state changed to %A{newState}"

        member this.DidChangeIceGatheringState
            (peerConnection: RTCPeerConnection, newState: RTCIceGatheringState)
            : unit =
            Log.info $"pc: ice gathering state changed to %A{newState}"

            if newState = RTCIceGatheringState.Complete then
                tryCompleteIceGathering ()

        member this.DidChangeLocalCandidate
            (
                peerConnection: RTCPeerConnection,
                local: RTCIceCandidate,
                remote: RTCIceCandidate,
                lastDataReceivedMs: int,
                reason: string
            ) : unit =
            Log.info $"pc: ice ic local candidate changed, reason: %A{reason}"

        member this.DidChangeSignalingState(peerConnection: RTCPeerConnection, stateChanged: RTCSignalingState) : unit =
            Log.info $"pc: signaling state changed %A{stateChanged}"

        member this.DidChangeStandardizedIceConnectionState
            (peerConnection: RTCPeerConnection, newState: RTCIceConnectionState)
            : unit =
            Log.info $"pc: standardized ice connection state changed %A{newState}"

        member this.DidFailToGatherIceCandidate
            (peerConnection: RTCPeerConnection, event: RTCIceCandidateErrorEvent)
            : unit =
            Log.info $"pc: failed to gather ice candidate %A{event.ErrorText}"

        member this.DidGenerateIceCandidate(peerConnection: RTCPeerConnection, candidate: RTCIceCandidate) : unit =
            Log.info $"pc: generated ice candidate %A{candidate.Description}"

        member this.DidOpenDataChannel(peerConnection: RTCPeerConnection, dataChannel: RTCDataChannel) : unit =
            Log.info $"pc: opened data channel %A{dataChannel.Description}"

        member this.DidRemoveIceCandidates
            (peerConnection: RTCPeerConnection, candidates: RTCIceCandidate array)
            : unit =
            Log.info $"pc: removed ice candidates %A{candidates |> Seq.map _.Description |> Seq.toList}"

        member this.DidRemoveReceiver(peerConnection: RTCPeerConnection, rtpReceiver: RTCRtpReceiver) : unit =
            Log.info $"pc: removed receiver %A{rtpReceiver.Description}"

        member this.DidRemoveStream(peerConnection: RTCPeerConnection, stream: RTCMediaStream) : unit =
            Log.info $"pc: removed stream %A{stream.Description}"

        member this.DidStartReceivingOnTransceiver
            (peerConnection: RTCPeerConnection, transceiver: RTCRtpTransceiver)
            : unit =
            Log.info $"pc: started receiving on transceiver %A{transceiver.Description}"

        member this.ShouldNegotiate(peerConnection: RTCPeerConnection) : unit = Log.info $"pc: should negotiate called"

    interface IRTCDataChannelDelegate with
        member _.DataChannelDidChangeState(dataChannel: RTCDataChannel) =
            Log.info $"dc: channel changed {dataChannel.Description}"

        member _.DidChangeBufferedAmount(dataChannel: RTCDataChannel, amount: uint64) =
            Log.info $"dc: buffer amount changed {dataChannel.Description}"

        member this.DidReceiveMessageWithBuffer(dataChannel: RTCDataChannel, buffer: RTCDataBuffer) =
            let json =
                System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(buffer.Data.AsStream())

            if outputChannel <> Unchecked.defaultof<_> then
                if not (outputChannel.Writer.TryWrite json) then
                    Log.warn $"dropped message from server {json}"

#endif
