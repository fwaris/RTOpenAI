namespace RTOpenAI.WebRTC.Android
#if ANDROID
open RTOpenAI.Api
open System
open System.Threading
open System.Threading.Tasks
open Org.Webrtc.Audio
open RTOpenAI.WebRTC
open Org.Webrtc
open System.Text.Json

module Connect =
    let createIceServers (clientConfig: WebRtcClientConfig) =
        clientConfig.IceServerUrls
        |> List.choose (fun url ->
            let builder = PeerConnection.IceServer.InvokeBuilder(url)

            if isNull builder then
                None
            else
                builder.CreateIceServer() |> Option.ofObj)
        |> ResizeArray

    let createPeerConnection
        (fac: PeerConnectionFactory)
        (config: PeerConnection.RTCConfiguration)
        (dlg: PeerConnection.IObserver)
        =
        try
            fac.CreatePeerConnection(config, dlg)
        with ex ->
            Log.exn (ex, "unable to create peer connection")
            raise ex

    let createAudioTrack (fac: PeerConnectionFactory) =
        let audioConstraints = new MediaConstraints()
        let audioSource = fac.CreateAudioSource(audioConstraints)
        let audioTrack = fac.CreateAudioTrack("audio0", audioSource)
        audioTrack

    let createDataChannel (pc: PeerConnection) =
        let config = new DataChannel.Init()

        try
            pc.CreateDataChannel(Env.OPENAI_RT_DATA_CHANNEL.Value, config) |> Some
        with ex ->
            Log.exn (ex, "createDataChannel")
            None

    let createMediaSenders fac (pc: PeerConnection) =
        let audioTrack = createAudioTrack fac
        let dataChannel = createDataChannel pc
        audioTrack, dataChannel

    let mediaConstraints () =
        let mandatory = [ "OfferToReceiveAudio", "true"; "OfferToReceiveVideo", "false" ]
        let optional = [ "DtlsSrtpKeyAgreement", "true" ]

        let nMandatory =
            mandatory
            |> List.map (fun (k, v) -> new MediaConstraints.KeyValuePair(k, v))
            |> ResizeArray

        let nOptional =
            optional
            |> List.map (fun (k, v) -> new MediaConstraints.KeyValuePair(k, v))
            |> ResizeArray

        new MediaConstraints(Mandatory = nMandatory, Optional = nOptional)

module AudioUtils =
    open Android.OS

    let configureIncomingAudio (audioTrack: AudioTrack) =
        audioTrack.SetVolume(1.0)
        audioTrack.SetEnabled(true) |> ignore

    type RecordErrorErrorCb() =
        inherit Java.Lang.Object()

        interface JavaAudioDeviceModule.IAudioRecordErrorCallback with
            member _.OnWebRtcAudioRecordError(p0: string) : unit =
                Log.error $"pc: audio record error {p0}"

            member _.OnWebRtcAudioRecordInitError(p0: string) : unit =
                Log.error $"pc: audio record init error {p0}"

            member _.OnWebRtcAudioRecordStartError
                (p0: JavaAudioDeviceModule.AudioRecordStartErrorCode, p1: string)
                : unit =
                Log.error $"pc: audio record init error %A{p0}, {p1}"

    type RecordStateCb() =
        inherit Java.Lang.Object()

        interface JavaAudioDeviceModule.IAudioRecordStateCallback with
            member _.OnWebRtcAudioRecordStart() : unit = Log.info $"pc: audio record start"
            member _.OnWebRtcAudioRecordStop() : unit = Log.info $"pc: audio record stop"

    type TrackErrorCb() =
        inherit Java.Lang.Object()

        interface JavaAudioDeviceModule.IAudioTrackErrorCallback with
            member _.OnWebRtcAudioTrackError(p0: string) : unit = Log.error $"pc: audio track error {p0}"

            member _.OnWebRtcAudioTrackInitError(p0: string) : unit =
                Log.error $"pc: audio track init error {p0}"

            member _.OnWebRtcAudioTrackStartError
                (p0: JavaAudioDeviceModule.AudioTrackStartErrorCode, p1: string)
                : unit =
                Log.error $"pc: audio track start error %A{p0}, {p1}"

    type TrackStateCb() =
        inherit Java.Lang.Object()

        interface JavaAudioDeviceModule.IAudioTrackStateCallback with
            member _.OnWebRtcAudioTrackStart() : unit = Log.info "pc: audio track start"
            member _.OnWebRtcAudioTrackStop() : unit = Log.info "pc: audio track stop"

    let deviceModule (context) =
        let recErrCb = new RecordErrorErrorCb()
        let recStateCb = new RecordStateCb()
        let trackErrCb = new TrackErrorCb()
        let trackStateCb = new TrackStateCb()

        let audioModule =
            JavaAudioDeviceModule
                .InvokeBuilder(context)
                .SetUseHardwareAcousticEchoCanceler(Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                .SetUseHardwareNoiseSuppressor(Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                .SetAudioRecordErrorCallback(recErrCb)
                .SetAudioRecordStateCallback(recStateCb)
                .SetAudioTrackErrorCallback(trackErrCb)
                .SetAudioTrackStateCallback(trackStateCb)
                .CreateAudioDeviceModule()
            |> fun m ->
                m.SetMicrophoneMute(false)
                m.SetSpeakerMute(false)
                m

        audioModule, [ recErrCb :> IDisposable; recStateCb; trackErrCb; trackStateCb ]


type SdpObserver(fCreated: SessionDescription -> unit, fSet: unit -> unit) =
    inherit Java.Lang.Object()

    interface ISdpObserver with
        member this.OnCreateFailure(p0: string) =
            let msg = $"sdp create failed {p0}"
            Log.error msg
            failwith msg

        member this.OnSetFailure(p0: string) =
            let msg = $"sdp set failure {p0}"
            Log.error msg
            failwith msg

        member this.OnCreateSuccess(p0: SessionDescription) : unit = fCreated p0
        member this.OnSetSuccess() = fSet ()

type DataChannelObserver(f: DataChannel.Buffer -> unit) =
    inherit Java.Lang.Object()

    interface DataChannel.IObserver with
        member this.OnMessage(p0: DataChannel.Buffer) : unit = f (p0)
        member this.OnStateChange() : unit = Log.info "data channel state changed"

        member this.OnBufferedAmountChange(p0: int64) : unit =
            Log.info $"dc: buffered amount changed to {p0}"

//encapsulates peer connection for Android
type WebRtcClientAndroid() =
    inherit Java.Lang.Object()

    //weak var delegate: WebRTCClientDelegate?
    let mutable peerConnection: PeerConnection = null
    let mutable audioQueue = MailboxProcessor.Start(ignore >> async.Return)
    let mutable mediaConstraints = Connect.mediaConstraints ()
    let mutable dataChannel: DataChannel = Unchecked.defaultof<_>
    let mutable outputChannel = Channels.Channel.CreateBounded<JsonDocument>(30)
    let mutable disposables: IDisposable list = []
    let mutable state = Disconnected
    let mutable audioTrack: AudioTrack = null
    let mutable microphoneEnabled = true
    let stateEvent = Event<State>()

    let mutable iceGatheringCompleted =
        TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let addDisposables xs = disposables <- disposables @ xs

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
        | null -> Log.info $"pc: microphone enabled requested before local audio track exists: enabled={enabled}"
        | track ->
            track.SetEnabled(enabled) |> ignore
            Log.info $"pc: microphone enabled={enabled}"

    let tryGetLocalOfferSdp () =
        match peerConnection with
        | null -> None
        | pc ->
            match pc.LocalDescription with
            | null -> None
            | description when String.IsNullOrWhiteSpace description.Description -> None
            | description -> Some description.Description

    let hasLocalCandidates () =
        tryGetLocalOfferSdp ()
        |> Option.exists (fun sdp -> sdp.Contains("a=candidate:") || sdp.Contains("a=end-of-candidates"))

    let waitForLocalCandidatesAsync timeoutMs =
        task {
            if hasLocalCandidates () then
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

    interface System.IDisposable with
        member this.Dispose() : unit =
            match peerConnection with
            | null -> ()
            | x ->
                x.Close()
                x.Dispose()
                peerConnection <- null

            mediaConstraints.Dispose()

            if dataChannel <> Unchecked.defaultof<_> then
                dataChannel.Close()
                dataChannel.Dispose()
                dataChannel <- Unchecked.defaultof<_>

            if outputChannel <> Unchecked.defaultof<_> then
                outputChannel.Writer.Complete()
                outputChannel <- Unchecked.defaultof<_>

            setState Disconnected
            disposables |> List.iter _.Dispose()
            disposables <- []
            base.Dispose()

    member this.OnMessage(buff: DataChannel.Buffer) =
        let count = buff.Data.Remaining()
        let bytes = Array.zeroCreate count
        buff.Data.Get(bytes) |> ignore
        let json = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(bytes)
        let r = outputChannel.Writer.TryWrite json

        if not r then
            Log.warn $"dropped incoming {count} bytes"

    //initialize WebRTCClient
    member this.Init(clientConfig: WebRtcClientConfig) =
        resetIceGathering ()

        let context = Microsoft.Maui.ApplicationModel.Platform.AppContext

        let opts =
            PeerConnectionFactory.InitializationOptions.InvokeBuilder(context).CreateInitializationOptions()

        PeerConnectionFactory.Initialize(opts)
        let audioModule, audioDisps = AudioUtils.deviceModule context
        addDisposables audioDisps

        let fac =
            PeerConnectionFactory.InvokeBuilder().SetAudioDeviceModule(audioModule).CreatePeerConnectionFactory()

        let config =
            new PeerConnection.RTCConfiguration(Connect.createIceServers clientConfig)

        config.SdpSemantics <- PeerConnection.SdpSemantics.UnifiedPlan
        peerConnection <- Connect.createPeerConnection fac config this
        let _audioTrack, _dataChannel = Connect.createMediaSenders fac peerConnection
        audioTrack <- _audioTrack
        applyMicrophoneEnabled microphoneEnabled
        let audioSender = peerConnection.AddTrack(_audioTrack, streamIds = [| "stream0" |])

        match _dataChannel with
        | Some d ->
            let obs = new DataChannelObserver(this.OnMessage)
            addDisposables [ obs ]
            dataChannel <- d
            dataChannel.RegisterObserver obs
        | None -> logAndFail "unable to create data channel"

        match clientConfig.BindAddress with
        | Some address -> Log.warn $"pc: bind address {address} was requested but is not supported on Android; ignoring"
        | None -> ()

    member this.SendOffer(ephemeralKey: string, url: string, clientConfig: WebRtcClientConfig) =
        task {
            use sem = new ManualResetEvent(false)

            //create offer
            use obsSet = new SdpObserver(ignore >> id, id)

            let setLocal sdp =
                peerConnection.SetLocalDescription(obsSet, sdp)
                sem.Set() |> ignore

            use obsOffer = new SdpObserver(setLocal, id)
            task { peerConnection.CreateOffer(obsOffer, mediaConstraints) } |> ignore
            let! rOffer = Async.AwaitWaitHandle(sem, 1000)

            if not rOffer then
                return Utils.logAndFail "timeout on creating local offer"

            do! waitForLocalCandidatesAsync clientConfig.IceGatherTimeoutMs
            Log.info $"pc: local sdp {peerConnection.LocalDescription.Description}"

            //send offer and get answer
            let! answer = Utils.getAnswerSdp ephemeralKey url peerConnection.LocalDescription.Description

            let remoteDesc =
                new SessionDescription(``type`` = SessionDescription.Type.Answer, description = answer)

            let ansSetSucc () = sem.Set() |> ignore
            use obsAns = new SdpObserver(ignore >> id, ansSetSucc)
            sem.Reset() |> ignore
            task { peerConnection.SetRemoteDescription(obsAns, remoteDesc) } |> ignore
            let! rAnswer = Async.AwaitWaitHandle(sem, 30_000)

            if not rAnswer then
                task { setState Disconnected } |> ignore
                return Utils.logAndFail "timeout on setting remote sdp as answer"
            else
                Log.info $"pc: remote sdp {answer}"
                task { setState Connected } |> ignore
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
                let dataBytes: byte[] = Text.UTF8Encoding.Default.GetBytes data
                use bytes = RTCJUtils.Wrap(dataBytes)
                use buffer = new DataChannel.Buffer(bytes, false)
                dataChannel.Send(buffer)
            else
                false

        member _.SetMicrophoneEnabled(enabled: bool) = applyMicrophoneEnabled enabled

    interface PeerConnection.IObserver with
        member this.OnAddStream(stream: MediaStream) : unit =
            Log.info $"pc: added remote stream: {stream.Id}"

            stream.AudioTracks
            |> Seq.cast<_>
            |> Seq.tryHead
            |> Option.map AudioUtils.configureIncomingAudio
            |> Option.defaultWith (fun () -> Log.warn "No audio track in remote stream")

        member this.OnAddTrack(receiver: RtpReceiver, mediaStreams: MediaStream array) : unit =
            Log.info $"pc: added track from receiver {receiver.Id}"

        member this.OnConnectionChange(newState: PeerConnection.PeerConnectionState) : unit =
            Log.info $"pc: connection state changed to %A{newState}"

            if
                newState = PeerConnection.PeerConnectionState.Disconnected
                || newState = PeerConnection.PeerConnectionState.Failed
                || newState = PeerConnection.PeerConnectionState.Closed
            then
                setState State.Disconnected

        member this.OnDataChannel(p0: DataChannel) : unit =
            Log.info $"pc: opened data channel %A{p0.Id}"

        member this.OnIceCandidate(p0: IceCandidate) : unit =
            Log.info $"pc: generated ice candidate %A{p0.ServerUrl}"

        member this.OnIceCandidateError(e: IceCandidateErrorEvent) : unit =
            Log.info $"pc: ice candidate error {e.Url}"

        member this.OnIceCandidatesRemoved(p0: IceCandidate array) : unit =
            Log.info $"pc: ice candidate removed {p0 |> Array.map _.ServerUrl}"

        member this.OnIceConnectionChange(p0: PeerConnection.IceConnectionState) : unit =
            Log.info $"pc: ice connection state chnaged %A{p0}"

        member this.OnIceConnectionReceivingChange(p0: bool) : unit =
            Log.info $"pc: ice connection receiving changed {p0}"

        member this.OnIceGatheringChange(p0: PeerConnection.IceGatheringState) : unit =
            Log.info $"pc: ice gathering change %A{p0}"

            if p0 = PeerConnection.IceGatheringState.Complete then
                tryCompleteIceGathering ()

        member this.OnRemoveStream(p0: MediaStream) : unit = Log.info $"pc: removed stream {p0.Id}"

        member this.OnRemoveTrack(receiver: RtpReceiver) : unit =
            Log.info $"pc: removed track {receiver.Id()}"

        member this.OnRenegotiationNeeded() : unit = Log.info $"pc: renegotiation needed"

        member this.OnSelectedCandidatePairChanged(e: CandidatePairChangeEvent) : unit =
            Log.info $"pc: selected candidate pair changed {e.Reason}"

        member this.OnSignalingChange(p0: PeerConnection.SignalingState) : unit =
            Log.info $"pc: signaling change %A{p0}"

        member this.OnStandardizedIceConnectionChange(newState: PeerConnection.IceConnectionState) : unit =
            Log.info $"pc: standardized ice connection state changed %A{newState}"

        member this.OnTrack(transceiver: RtpTransceiver) : unit =
            Log.info $"pc: on track %A{transceiver.MediaType}"

#endif
