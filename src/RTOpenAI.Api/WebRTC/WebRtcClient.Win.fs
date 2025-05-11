namespace RTOpenAI.WebRTC.Windows
#if WINDOWS
open RTOpenAI.WebRTC
open System
open System.Threading
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

    let addDisposables xs = disposables <- disposables @ xs
    
    let setState s = state <- s; stateEvent.Trigger(s)

    let onMessage dataChannel payload (bytes:byte[]) = 
        let msg = JsonSerializer.Deserialize(bytes)
        outputChannel.Writer.TryWrite(msg) |> ignore

    let audioDelegate (pc:RTCPeerConnection) = EncodedSampleDelegate(fun duration bytes -> pc.SendAudio(duration,bytes))

    let addAudio(pc:RTCPeerConnection) = 
        let winAudioEP = new WindowsAudioEndPoint(new AudioEncoder(includeOpus=true),-1,-1,false,false)
        winAudioEP.RestrictFormats(fun x -> x.FormatName = "OPUS")         
        winAudioEP.add_OnAudioSinkError(SourceErrorDelegate(fun err -> Log.info $"Audio sink error {err}"))
        let audioDlg = audioDelegate pc
        winAudioEP.add_OnAudioSourceEncodedSample(audioDlg)
        let audioTrack = new MediaStreamTrack(winAudioEP.GetAudioSourceFormats(), MediaStreamStatusEnum.SendRecv)
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
                }
                |> ignore)

    let hookAudioStream (winAudioEP:WindowsAudioEndPoint) (pc:RTCPeerConnection) = 
        pc.add_OnRtpPacketReceived (fun ep media packet -> 
            if media = SDPMediaTypesEnum.audio then 
                let ph = packet.Header
                winAudioEP.GotAudioRtp(
                    ep,
                    ph.SyncSource,
                    uint32 ph.SequenceNumber,
                    ph.Timestamp,
                    ph.PayloadType,
                    ph.MarkerBit = 1,
                    packet.Payload)
        )

    let createPcConnection() = 
        task {
            let pcConfig = RTCConfiguration(X_UseRtpFeedbackProfile = true)
            let pc = new RTCPeerConnection(pcConfig)
            let! dataChannel = pc.createDataChannel(C.OPENAI_RT_DATA_CHANNEL)
            let winAudioEP = addAudio pc
            pc.add_OnTimeout(fun mediaType -> Log.info $"Timeout on media {mediaType}")
            hookConnectionHandling winAudioEP pc
            hookAudioStream winAudioEP pc
            return pc,dataChannel,winAudioEP
        }


    member this.Init () =               
        task {
            let! pc,dc,winAudioEP = createPcConnection()
            peerConnection <- pc
            dataChannel <- dc
            dataChannel.add_onmessage(OnDataChannelMessageDelegate(onMessage))
        }

    member this.SendOffer(ephemeralKey:string,url:string) =
        task {
            let offer = peerConnection.createOffer()
            //Log.info $"Offer SDP:\r\n{offer.sdp}"
            do! peerConnection.setLocalDescription(offer)
            let! answer = Utils.getAnswerSdp ephemeralKey url offer.sdp
            //Log.info $"Answer SDP:\r\n{answer}"
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
                    
        member this.Connect (key,url,_) = 
            task {
                try
                    do! this.Init()
                    setState Connecting
                    do!this.SendOffer(key,url)
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
