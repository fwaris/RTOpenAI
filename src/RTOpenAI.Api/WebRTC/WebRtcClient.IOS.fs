namespace RTOpenAI.WebRTC.IOS
#if IOS || MACCATALYST
open System
open System.Threading
open System.Threading.Tasks
open RTOpenAI.Api
open RTOpenAI.WebRTC
open FsWebRTC.Bindings
open Foundation
open AVFoundation
open ObjCRuntime
open System.Text.Json

module Connect =
    let private createIceServers (clientConfig: WebRtcClientConfig) =
        clientConfig.IceServerUrls
        |> List.map (fun url -> new LKRTCIceServer([| url |]))
        |> List.toArray

    let rtcConfiguration (clientConfig: WebRtcClientConfig) =
        let config = new LKRTCConfiguration()
        config.SdpSemantics <- LKRTCSdpSemantics.UnifiedPlan
        config.IceServers <- createIceServers clientConfig
        config

    let createPeerConnection (fac: LKRTCPeerConnectionFactory) config constraints dlg =
        try
            fac.PeerConnectionWithConfiguration(config, constraints, dlg)
        with ex ->
            Log.exn (ex, "unable to create peer connection")
            raise ex

    let createAudioTrack (fac: LKRTCPeerConnectionFactory) =
        let audioConstraints = new LKRTCMediaConstraints(null, null)
        let audioSource = fac.AudioSourceWithConstraints(audioConstraints)
        let audioTrack = fac.AudioTrackWithSource(audioSource, trackId = "audio0")
        audioSource, audioTrack

    let createDataChannel (pc: LKRTCPeerConnection) =
        let config = new LKRTCDataChannelConfiguration()

        try
            pc.DataChannelForLabel(Env.OPENAI_RT_DATA_CHANNEL.Value, config) |> Some
        with ex ->
            Log.exn (ex, "createDataChannel")
            None

    let createMediaSenders fac (pc: LKRTCPeerConnection) =
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

        new LKRTCMediaConstraints(null, nOptional)



module AudioUtils =
    let private routeExperimentName =
        "variant7-livekit-webrtc-config-manual-audio"

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

    let private portOverrideForPolicy (audioSession: LKRTCAudioSession) policy =
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

    let private describeAudioSession (audioSession: LKRTCAudioSession) =
        $"active={audioSession.IsActive}; category={audioSession.Category}; mode={audioSession.Mode}; outputVolume={audioSession.OutputVolume}; {describeRoute audioSession.CurrentRoute}"

    let private logAudioSessionState phase routePolicy (audioSession: LKRTCAudioSession) =
        Log.info
            $"audio session {phase}: experiment={routeExperimentName}; routePolicy={policyName routePolicy}; {describeAudioSession audioSession}"

    let private clearOutputAudioPortOverride src (audioSession: LKRTCAudioSession) =
        use mutable error = null

        let overrideCleared =
            audioSession.OverrideOutputAudioPort(AVAudioSessionPortOverride.None, &error)

        warnSessionCall src overrideCleared error

    let private webRtcAudioConfiguration opts =
        let config = LKRTCAudioSessionConfiguration.WebRTCConfiguration()
        config.Category <- AVAudioSession.CategoryPlayAndRecord
        config.Mode <- AVAudioSession.ModeVideoChat
        config.CategoryOptions <- opts
        config.SampleRate <- 48000.
        config.IoBufferDuration <- 0.01
        config.InputNumberOfChannels <- 1n
        config.OutputNumberOfChannels <- 1n
        LKRTCAudioSessionConfiguration.SetWebRTCConfiguration(config)
        config

    let private enableManualAudio (audioSession: LKRTCAudioSession) =
        audioSession.UseManualAudio <- true
        audioSession.IsAudioEnabled <- true

    let private applyWebRtcAudioConfiguration src routePolicy (audioSession: LKRTCAudioSession) =
        let opts = categoryOptionsForPolicy routePolicy
        let config = webRtcAudioConfiguration opts

        use mutable error = null
        let configured = audioSession.SetConfiguration(config, true, &error)
        warnSessionCall src configured error

        configured

    let release (clientConfig: WebRtcClientConfig) =
        let audioSession = LKRTCAudioSession.SharedInstance()
        let routePolicy = clientConfig.IosAudioRoutePolicy

        audioSession.LockForConfiguration()

        try
            logAudioSessionState "teardown begin" routePolicy audioSession
            audioSession.IsAudioEnabled <- false
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
        let audioSession = LKRTCAudioSession.SharedInstance()
        let routePolicy = clientConfig.IosAudioRoutePolicy

        audioSession.LockForConfiguration()

        try
            logAudioSessionState "configure begin" routePolicy audioSession
            audioSession.IgnoresPreferredAttributeConfigurationErrors <- true
            enableManualAudio audioSession
            applyWebRtcAudioConfiguration "set LiveKit WebRTC audio configuration" routePolicy audioSession
            |> ignore

            clearOutputAudioPortOverride "clear stale output audio port override before activation" audioSession

            logAudioSessionState "configured" routePolicy audioSession
        finally
            audioSession.UnlockForConfiguration()

    let configureIncomingAudio (clientConfig: WebRtcClientConfig) (audioTrack: LKRTCAudioTrack) =
        let audioSession = LKRTCAudioSession.SharedInstance()
        let routePolicy = clientConfig.IosAudioRoutePolicy
        let audioVolume = incomingAudioVolume routePolicy

        audioTrack.Source.Volume <- audioVolume
        audioTrack.IsEnabled <- true

        audioSession.LockForConfiguration()

        try
            enableManualAudio audioSession

            if audioSession.Category <> AVAudioSession.CategoryPlayAndRecord then
                applyWebRtcAudioConfiguration "reapply LiveKit WebRTC audio configuration for incoming audio" routePolicy audioSession
                |> ignore

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
    let mutable peerConnection: LKRTCPeerConnection = null
    let mutable peerConnectionFactory: LKRTCPeerConnectionFactory = null
    let mutable audioQueue = MailboxProcessor.Start(ignore >> async.Return)
    let mutable mediaConstraints = Connect.mediaConstraints ()
    let mutable audioSource: LKRTCAudioSource = null
    let mutable audioTrack: LKRTCAudioTrack = null
    let mutable audioSender: LKRTCRtpSender = null
    let mutable dataChannel: LKRTCDataChannel = Unchecked.defaultof<_>
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

    let logAudioDeviceModuleState phase =
        if peerConnectionFactory = null then
            Log.warn $"pc[{instanceId}]: audio module {phase}: peerConnectionFactory=null"
        else
            let audioModule = peerConnectionFactory.AudioDeviceModule

            if audioModule = null then
                Log.warn $"pc[{instanceId}]: audio module {phase}: null"
            else
                Log.info
                    $"pc[{instanceId}]: audio module {phase}: isPlayoutInitialized={audioModule.IsPlayoutInitialized}; isPlaying={audioModule.IsPlaying}; playing={audioModule.Playing}; isRecordingInitialized={audioModule.IsRecordingInitialized}; isRecording={audioModule.IsRecording}; recording={audioModule.Recording}; isEngineRunning={audioModule.IsEngineRunning}; manualRenderingMode={audioModule.ManualRenderingMode}; muteMode={audioModule.MuteMode}; platformVoiceProcessingAllowed={audioModule.PlatformVoiceProcessingAllowed}; voiceProcessingBypassed={audioModule.VoiceProcessingBypassed}; voiceProcessingAGCEnabled={audioModule.VoiceProcessingAgcEnabled}"

    let ensurePlayoutStarted phase =
        if peerConnectionFactory = null then
            Log.warn $"pc[{instanceId}]: audio module {phase}: cannot start playout because peerConnectionFactory=null"
        else
            let audioModule = peerConnectionFactory.AudioDeviceModule

            if audioModule = null then
                Log.warn $"pc[{instanceId}]: audio module {phase}: cannot start playout because audio module=null"
            else
                logAudioDeviceModuleState $"{phase} before playout ensure"

                if not audioModule.IsPlayoutInitialized then
                    let initResult = audioModule.InitPlayout()
                    Log.info $"pc[{instanceId}]: audio module {phase}: InitPlayout result={initResult}"

                if not audioModule.IsPlaying then
                    let startResult = audioModule.StartPlayout()
                    Log.info $"pc[{instanceId}]: audio module {phase}: StartPlayout result={startResult}"

                logAudioDeviceModuleState $"{phase} after playout ensure"

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
                    && peerConnection.IceGatheringState = LKRTCIceGatheringState.Complete)
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
        peerConnectionFactory <-
            new LKRTCPeerConnectionFactory(LKRTCAudioDeviceModuleType.AudioEngine, false, null, null, null)

        logAudioDeviceModuleState "after factory create"
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
                LKRTCCreateSessionDescriptionCompletionHandler(fun sdp err ->
                    if sdp <> null then
                        peerConnection.SetLocalDescription(
                            sdp,
                            LKRTCSetSessionDescriptionCompletionHandler(fun err ->
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
                                                    new LKRTCSessionDescription(
                                                        ``type`` = LKRTCSdpType.Answer,
                                                        sdp = answer
                                                    )

                                                peerConnection.SetRemoteDescription(
                                                    answerSdp,
                                                    LKRTCSetSessionDescriptionCompletionHandler(fun err ->
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
                use buffer = new LKRTCDataBuffer(data, isBinary = false)
                dataChannel.SendData(buffer)
            else
                false

        member _.SetMicrophoneEnabled(enabled: bool) = applyMicrophoneEnabled enabled

    interface ILKRTCPeerConnectionDelegate with
        member this.DidAddReceiver
            (peerConnection: LKRTCPeerConnection, rtpReceiver: LKRTCRtpReceiver, mediaStreams: LKRTCMediaStream array)
            : unit =
            Log.info $"pc: added receiver: {rtpReceiver.Description}"

        member this.DidAddStream(peerConnection: LKRTCPeerConnection, stream: LKRTCMediaStream) : unit =
            Log.info $"pc: added remote stream: {stream.Description}"

            stream.AudioTracks
            |> Seq.tryHead
            |> Option.map (AudioUtils.configureIncomingAudio activeClientConfig)
            |> Option.defaultWith (fun () -> Log.warn "No audio track in remote stream")

            ensurePlayoutStarted "remote stream"

        member this.DidChangeConnectionState
            (peerConnection: LKRTCPeerConnection, newState: LKRTCPeerConnectionState)
            : unit =
            Log.info $"pc: connection state changed to %A{newState}"

            match newState with
            | LKRTCPeerConnectionState.Disconnected
            | LKRTCPeerConnectionState.Failed
            | LKRTCPeerConnectionState.Closed -> setState State.Disconnected
            | _ -> ()

        member this.DidChangeIceConnectionState
            (peerConnection: LKRTCPeerConnection, newState: LKRTCIceConnectionState)
            : unit =
            Log.info $"pc: ice connection state changed to %A{newState}"

        member this.DidChangeIceGatheringState
            (peerConnection: LKRTCPeerConnection, newState: LKRTCIceGatheringState)
            : unit =
            Log.info $"pc: ice gathering state changed to %A{newState}"

            if newState = LKRTCIceGatheringState.Complete then
                tryCompleteIceGathering ()

        member this.DidChangeLocalCandidate
            (
                peerConnection: LKRTCPeerConnection,
                local: LKRTCIceCandidate,
                remote: LKRTCIceCandidate,
                lastDataReceivedMs: int,
                reason: string
            ) : unit =
            Log.info $"pc: ice ic local candidate changed, reason: %A{reason}"

        member this.DidChangeSignalingState(peerConnection: LKRTCPeerConnection, stateChanged: LKRTCSignalingState) : unit =
            Log.info $"pc: signaling state changed %A{stateChanged}"

        member this.DidChangeStandardizedIceConnectionState
            (peerConnection: LKRTCPeerConnection, newState: LKRTCIceConnectionState)
            : unit =
            Log.info $"pc: standardized ice connection state changed %A{newState}"

        member this.DidFailToGatherIceCandidate
            (peerConnection: LKRTCPeerConnection, event: LKRTCIceCandidateErrorEvent)
            : unit =
            Log.info $"pc: failed to gather ice candidate %A{event.ErrorText}"

        member this.DidGenerateIceCandidate(peerConnection: LKRTCPeerConnection, candidate: LKRTCIceCandidate) : unit =
            Log.info $"pc: generated ice candidate %A{candidate.Description}"

        member this.DidOpenDataChannel(peerConnection: LKRTCPeerConnection, dataChannel: LKRTCDataChannel) : unit =
            Log.info $"pc: opened data channel %A{dataChannel.Description}"

        member this.DidRemoveIceCandidates
            (peerConnection: LKRTCPeerConnection, candidates: LKRTCIceCandidate array)
            : unit =
            Log.info $"pc: removed ice candidates %A{candidates |> Seq.map _.Description |> Seq.toList}"

        member this.DidRemoveReceiver(peerConnection: LKRTCPeerConnection, rtpReceiver: LKRTCRtpReceiver) : unit =
            Log.info $"pc: removed receiver %A{rtpReceiver.Description}"

        member this.DidRemoveStream(peerConnection: LKRTCPeerConnection, stream: LKRTCMediaStream) : unit =
            Log.info $"pc: removed stream %A{stream.Description}"

        member this.DidStartReceivingOnTransceiver
            (peerConnection: LKRTCPeerConnection, transceiver: LKRTCRtpTransceiver)
            : unit =
            Log.info $"pc: started receiving on transceiver %A{transceiver.Description}"

        member this.ShouldNegotiate(peerConnection: LKRTCPeerConnection) : unit = Log.info $"pc: should negotiate called"

    interface ILKRTCDataChannelDelegate with
        member _.DataChannelDidChangeState(dataChannel: LKRTCDataChannel) =
            Log.info $"dc: channel changed {dataChannel.Description}"

        member _.DidChangeBufferedAmount(dataChannel: LKRTCDataChannel, amount: uint64) =
            Log.info $"dc: buffer amount changed {dataChannel.Description}"

        member this.DidReceiveMessageWithBuffer(dataChannel: LKRTCDataChannel, buffer: LKRTCDataBuffer) =
            let json =
                System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(buffer.Data.AsStream())

            if outputChannel <> Unchecked.defaultof<_> then
                if not (outputChannel.Writer.TryWrite json) then
                    Log.warn $"dropped message from server {json}"

#endif
