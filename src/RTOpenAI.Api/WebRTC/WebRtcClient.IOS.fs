namespace RTOpenAI.WebRTC.IOS
#if IOS || MACCATALYST
open System.Threading
open RTOpenAI.Api
open RTOpenAI.WebRTC
open IOS.WebRTC
open Foundation
open AVFoundation
open System.Text.Json

module Connect =
    let createPeerConnection (fac:RTCPeerConnectionFactory) config constraints dlg =
        try 
            fac.PeerConnectionWithConfiguration(config, constraints, dlg)            
        with ex -> 
            Log.exn(ex,"unable to create peer connection")
            raise ex

    let createAudioTrack(fac:RTCPeerConnectionFactory) = 
        let audioConstraints = new RTCMediaConstraints(null,null)        
        let audioSource = fac.AudioSourceWithConstraints(audioConstraints)        
        let audioTrack = fac.AudioTrackWithSource(audioSource,trackId = "audio0")
        audioTrack

    let createDataChannel (pc:RTCPeerConnection) =
        let config = new RTCDataChannelConfiguration()
        try
            pc.DataChannelForLabel(C.OPENAI_RT_DATA_CHANNEL, config) |> Some
        with ex -> 
            Log.exn (ex,"createDataChannel")
            None

    let createMediaSenders fac (pc:RTCPeerConnection)   = 
        let audioTrack = createAudioTrack fac
        let dataChannel = createDataChannel pc
        audioTrack,dataChannel

    let mediaConstraints() = 
        let mandatory = 
            [
                "OfferToReceiveAudio" :> obj, "true" :> obj
                "OfferToReceiveVideo", "false"
            ]
            |> dict
        let optional = dict ["DtlsSrtpKeyAgreement" :> obj, "true" :> obj]
        let nMandatory = NSDictionary<NSString,NSString>.FromObjectsAndKeys(
            Seq.toArray mandatory.Values, Seq.toArray mandatory.Keys)
        let nOptional = NSDictionary<NSString,NSString>.FromObjectsAndKeys(
            Seq.toArray optional.Values, Seq.toArray optional.Keys)
        new RTCMediaConstraints(null,nOptional)


  
module AudioUtils = 
    let checkError src f =
        let err : NSError = f()
        if err <> null then
            let msg = $"error with recording session. Src {src}; Error {err}"
            failwith msg                   
    let checkErrorB src f =
        match f() with 
        | true,null -> ()
        | _,err -> 
            if err <> null then
                let msg = $"error with recording session. Src {src}; Error {err}"
                failwith msg                   

    let activate() =
        let sess = AVAudioSession.SharedInstance()      
        checkError "activate:SetActive" (fun () -> sess.SetActive(true))
            
    let release() =
        let sess = AVAudioSession.SharedInstance()
        checkError "release" (fun () -> sess.SetActive(false))

    let configureAudioSession () =
        let opts = AVAudioSessionCategoryOptions.AllowBluetooth ||| AVAudioSessionCategoryOptions.DefaultToSpeaker        
        checkError "set cat. playRec" (fun () -> AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord,opts ))        
        checkErrorB "set mode" (fun () -> AVAudioSession.SharedInstance().SetMode(AVAudioSessionMode.SpokenAudio))

    let configureIncomingAudio (audioTrack:RTCAudioTrack) =
        audioTrack.Source.Volume <- 1.0
        audioTrack.IsEnabled <- true        
        checkErrorB "port override" (fun () -> AVAudioSession.SharedInstance().OverrideOutputAudioPort(AVAudioSessionPortOverride.Speaker))

//encapsulates peer connection for IOS
type WebRtcClientIOS() =    
    inherit NSObject() 
    //weak var delegate: WebRTCClientDelegate?
    let mutable peerConnection: RTCPeerConnection = null   
    let mutable audioQueue = MailboxProcessor.Start(ignore>>async.Return)
    let mutable mediaConstraints = Connect.mediaConstraints()
    let mutable dataChannel: RTCDataChannel = Unchecked.defaultof<_>
    let mutable outputChannel = Channels.Channel.CreateBounded<JsonDocument>(30)
    let mutable state = Disconnected
    let stateEvent = Event<State>()

    let setState s = state <- s; stateEvent.Trigger(s)

    interface ObjCRuntime.INativeObject with
        member this.Handle = base.Handle        

    interface System.IDisposable with 
        member this.Dispose (): unit = 
            match peerConnection with null -> () | x -> x.Close(); x.Dispose(); peerConnection <- null
            mediaConstraints.Dispose()
            if dataChannel <> Unchecked.defaultof<_> then 
                dataChannel.Close()
                dataChannel.Dispose()
                dataChannel <- Unchecked.defaultof<_>
            if outputChannel <>  Unchecked.defaultof<_> then
                outputChannel.Writer.Complete()
                outputChannel <- Unchecked.defaultof<_>
            setState Disconnected            
            base.Dispose()            

    //initialize WebRTCClient
    member this.Init() =
        let config = new RTCConfiguration()
        config.SdpSemantics <- RTCSdpSemantics.UnifiedPlan
        let fac = new RTCPeerConnectionFactory(null,null)
        peerConnection <- Connect.createPeerConnection fac config mediaConstraints this    
        let _audioTrack, _dataChannel = Connect.createMediaSenders fac peerConnection 
        let audioSender = peerConnection.AddTrack(_audioTrack, streamIds = [|"stream0"|])        
        match _dataChannel with 
        | Some d -> dataChannel <- d; dataChannel.Delegate <- this
        | None   -> failwith "unable to create data channel"
        AudioUtils.configureAudioSession()

    member this.SendOffer(ephemeralKey:string,url:string) =
        task {
            use sem = new ManualResetEvent(false)
            let completionHandler = RTCCreateSessionDescriptionCompletionHandler(fun sdp err  ->
                if sdp <> null then
                    peerConnection.SetLocalDescription(sdp, RTCSetSessionDescriptionCompletionHandler(fun err ->
                        if err <> null then 
                            Log.error($"pc: error set local description {err.Description}")
                        else
                            Log.info $"pc: local sdp {sdp.Sdp}"
                            task {
                                let! answer = Utils.getAnswerSdp ephemeralKey url sdp.Sdp
                                Log.info $"pc: remote sdp {answer}"
                                let answerSdp = new RTCSessionDescription(``type`` = RTCSdpType.Answer, sdp = answer)
                                peerConnection.SetRemoteDescription(answerSdp, RTCSetSessionDescriptionCompletionHandler(fun err -> 
                                    if err <> null then 
                                        Log.error $"pc: error setting remote description {err.Description}"                                
                                ))
                                sem.Set() |> ignore

                            }
                            |> ignore
                    ))

                elif err <> null then 
                    Log.error $"pc: sdp offer error {err.Description}"
                else
                    Log.error "pc: unknown issue when setting local error"
                )
            peerConnection.OfferForConstraints(mediaConstraints,completionHandler)
            let! r = Async.AwaitWaitHandle(sem,30000)
            if r then 
                task{ setState Connected } |> ignore
                return ()
            else
                task {setState Disconnected} |> ignore
                let msg = "SDP answer timeout"
                Log.error msg
                return failwith msg
        }

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
                use buffer = new RTCDataBuffer(data, isBinary=false)
                dataChannel.SendData(buffer)
            else
                false
             
    interface IRTCPeerConnectionDelegate with        
        member this.DidAddReceiver(peerConnection: RTCPeerConnection, rtpReceiver: RTCRtpReceiver, mediaStreams: RTCMediaStream array): unit = 
            Log.info $"pc: added receiver: {rtpReceiver.Description}"
        member this.DidAddStream(peerConnection: RTCPeerConnection, stream: RTCMediaStream): unit =
            Log.info $"pc: added remote stream: {stream.Description}"
            stream.AudioTracks
            |> Seq.tryHead
            |> Option.map AudioUtils.configureIncomingAudio                     
            |> Option.defaultWith(fun () -> Log.warn "No audio track in remote stream")            
        member this.DidChangeConnectionState(peerConnection: RTCPeerConnection, newState: RTCPeerConnectionState): unit = 
            Log.info $"pc: connection state changed to %A{newState}"
            match newState with 
            | RTCPeerConnectionState.Disconnected 
            | RTCPeerConnectionState.Failed
            | RTCPeerConnectionState.Closed -> setState State.Disconnected
            | _ -> ()
        member this.DidChangeIceConnectionState(peerConnection: RTCPeerConnection, newState: RTCIceConnectionState): unit = 
            Log.info $"pc: ice connection state changed to %A{newState}"
        member this.DidChangeIceGatheringState(peerConnection: RTCPeerConnection, newState: RTCIceGatheringState): unit = 
            Log.info $"pc: ice gathering state changed to %A{newState}"   
        member this.DidChangeLocalCandidate(peerConnection: RTCPeerConnection, local: RTCIceCandidate, remote: RTCIceCandidate, lastDataReceivedMs: int, reason: string): unit = 
            Log.info $"pc: ice ic local candidate changed, reason: %A{reason}"   
        member this.DidChangeSignalingState(peerConnection: RTCPeerConnection, stateChanged: RTCSignalingState): unit = 
            Log.info $"pc: signaling state changed %A{stateChanged}"   
        member this.DidChangeStandardizedIceConnectionState(peerConnection: RTCPeerConnection, newState: RTCIceConnectionState): unit = 
            Log.info $"pc: standardized ice connection state changed %A{newState}"   
        member this.DidFailToGatherIceCandidate(peerConnection: RTCPeerConnection, event: RTCIceCandidateErrorEvent): unit = 
            Log.info $"pc: failed to gather ice candidate %A{event.ErrorText}"   
        member this.DidGenerateIceCandidate(peerConnection: RTCPeerConnection, candidate: RTCIceCandidate): unit = 
            Log.info $"pc: generated ice candidate %A{candidate.Description}"   
        member this.DidOpenDataChannel(peerConnection: RTCPeerConnection, dataChannel: RTCDataChannel): unit = 
            Log.info $"pc: opened data channel %A{dataChannel.Description}"   
        member this.DidRemoveIceCandidates(peerConnection: RTCPeerConnection, candidates: RTCIceCandidate array): unit = 
            Log.info $"pc: removed ice candidates %A{candidates |> Seq.map _.Description |> Seq.toList}"   
        member this.DidRemoveReceiver(peerConnection: RTCPeerConnection, rtpReceiver: RTCRtpReceiver): unit = 
            Log.info $"pc: removed receiver %A{rtpReceiver.Description}"   
        member this.DidRemoveStream(peerConnection: RTCPeerConnection, stream: RTCMediaStream): unit = 
            Log.info $"pc: removed stream %A{stream.Description}"   
        member this.DidStartReceivingOnTransceiver(peerConnection: RTCPeerConnection, transceiver: RTCRtpTransceiver): unit = 
            Log.info $"pc: started receiving on transceiver %A{transceiver.Description}"   
        member this.ShouldNegotiate(peerConnection: RTCPeerConnection): unit = 
            Log.info $"pc: should negotiate called"   
    
    interface IRTCDataChannelDelegate with 
        member _.DataChannelDidChangeState (dataChannel: RTCDataChannel) =
            Log.info $"dc: channel changed {dataChannel.Description}"
        member _.DidChangeBufferedAmount(dataChannel: RTCDataChannel, amount: uint64) =
            Log.info $"dc: buffer amount changed {dataChannel.Description}"
        member this.DidReceiveMessageWithBuffer( dataChannel: RTCDataChannel,  buffer: RTCDataBuffer) =            
            let json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonDocument>(buffer.Data.AsStream())
            if outputChannel <> Unchecked.defaultof<_> then 
                if not(outputChannel.Writer.TryWrite json) then 
                    Log.warn $"dropped message from server {json}"

#endif
