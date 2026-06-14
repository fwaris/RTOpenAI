using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace FsWebRTC.Bindings
{
	interface ILKRTCAudioCustomProcessingDelegate { }
	interface ILKRTCAudioDeviceModuleDelegate { }
	interface ILKRTCAudioProcessingModule { }
	interface ILKRTCAudioRenderer { }
	interface ILKRTCFrameCryptorDelegate { }
	interface ILKRTCRtpSource { }

	[BaseType(typeof(NSObject))]
	interface LKRTCAudioBuffer
	{
		[Export("channels")]
		nuint Channels { get; }

		[Export("frames")]
		nuint Frames { get; }

		[Export("framesPerBand")]
		nuint FramesPerBand { get; }

		[Export("bands")]
		nuint Bands { get; }

		[Export("rawBufferForChannel:")]
		unsafe IntPtr RawBufferForChannel(nuint channel);
	}

	[Protocol, Model]
	[BaseType(typeof(NSObject))]
	interface LKRTCAudioCustomProcessingDelegate
	{
		[Abstract]
		[Export("audioProcessingInitializeWithSampleRate:channels:")]
		void AudioProcessingInitialize(nuint sampleRateHz, nuint channels);

		[Abstract]
		[Export("audioProcessingProcess:")]
		void AudioProcessingProcess(LKRTCAudioBuffer audioBuffer);

		[Abstract]
		[Export("audioProcessingRelease")]
		void AudioProcessingRelease();
	}

	[Protocol, Model]
	[BaseType(typeof(NSObject))]
	interface LKRTCAudioRenderer
	{
		[Abstract]
		[Export("renderPCMBuffer:")]
		void Render(AVAudioPcmBuffer pcmBuffer);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCAudioProcessingConfig
	{
		[Export("isEchoCancellationEnabled")]
		bool IsEchoCancellationEnabled { get; set; }

		[Export("isEchoCancellationMobileMode")]
		bool IsEchoCancellationMobileMode { get; set; }

		[Export("isNoiseSuppressionEnabled")]
		bool IsNoiseSuppressionEnabled { get; set; }

		[Export("isHighpassFilterEnabled")]
		bool IsHighpassFilterEnabled { get; set; }

		[Export("isAutoGainControl1Enabled")]
		bool IsAutoGainControl1Enabled { get; set; }

		[Export("isAutoGainControl2Enabled")]
		bool IsAutoGainControl2Enabled { get; set; }
	}

	[Protocol]
	[BaseType(typeof(NSObject))]
	interface LKRTCAudioProcessingModule
	{
		[Abstract]
		[Export("config", ArgumentSemantic.Strong)]
		LKRTCAudioProcessingConfig Config { get; set; }
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCDefaultAudioProcessingModule : ILKRTCAudioProcessingModule
	{
		[Export("initWithConfig:capturePostProcessingDelegate:renderPreProcessingDelegate:")]
		[DesignatedInitializer]
		NativeHandle Constructor(
			[NullAllowed] LKRTCAudioProcessingConfig config,
			[NullAllowed] ILKRTCAudioCustomProcessingDelegate capturePostProcessingDelegate,
			[NullAllowed] ILKRTCAudioCustomProcessingDelegate renderPreProcessingDelegate);

		[Export("config", ArgumentSemantic.Strong)]
		LKRTCAudioProcessingConfig Config { get; set; }

		[Export("muted")]
		bool Muted { [Bind("isMuted")] get; set; }

		[Wrap("WeakCapturePostProcessingDelegate")]
		[NullAllowed]
		ILKRTCAudioCustomProcessingDelegate CapturePostProcessingDelegate { get; set; }

		[NullAllowed, Export("capturePostProcessingDelegate", ArgumentSemantic.Weak)]
		NSObject WeakCapturePostProcessingDelegate { get; set; }

		[Wrap("WeakRenderPreProcessingDelegate")]
		[NullAllowed]
		ILKRTCAudioCustomProcessingDelegate RenderPreProcessingDelegate { get; set; }

		[NullAllowed, Export("renderPreProcessingDelegate", ArgumentSemantic.Weak)]
		NSObject WeakRenderPreProcessingDelegate { get; set; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCAudioProcessingOptionsResult
	{
		[Export("success")]
		bool Success { [Bind("isSuccess")] get; }

		[Export("code")]
		LKRTCAudioProcessingOptionsResultCode Code { get; }

		[Export("message")]
		string Message { get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCAudioProcessingComponentOptions
	{
		[Export("enabled")]
		bool Enabled { [Bind("isEnabled")] get; }

		[Export("mode")]
		LKRTCAudioProcessingMode Mode { get; }

		[Export("initWithEnabled:")]
		NativeHandle Constructor(bool enabled);

		[Export("initWithEnabled:mode:")]
		[DesignatedInitializer]
		NativeHandle Constructor(bool enabled, LKRTCAudioProcessingMode mode);
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCAudioProcessingOptions
	{
		[Export("echoCancellation")]
		bool EchoCancellation { get; }

		[Export("noiseSuppression")]
		bool NoiseSuppression { get; }

		[Export("autoGainControl")]
		bool AutoGainControl { get; }

		[Export("highPassFilter")]
		bool HighPassFilter { get; }

		[Export("echoCancellationMode")]
		LKRTCAudioProcessingMode EchoCancellationMode { get; }

		[Export("noiseSuppressionMode")]
		LKRTCAudioProcessingMode NoiseSuppressionMode { get; }

		[Export("autoGainControlMode")]
		LKRTCAudioProcessingMode AutoGainControlMode { get; }

		[Export("highPassFilterMode")]
		LKRTCAudioProcessingMode HighPassFilterMode { get; }

		[Export("initWithEchoCancellation:noiseSuppression:autoGainControl:highPassFilter:")]
		NativeHandle Constructor(bool echoCancellation, bool noiseSuppression, bool autoGainControl, bool highPassFilter);

		[Export("initWithEchoCancellationOptions:noiseSuppressionOptions:autoGainControlOptions:highPassFilterOptions:")]
		[DesignatedInitializer]
		NativeHandle Constructor(
			LKRTCAudioProcessingComponentOptions echoCancellationOptions,
			LKRTCAudioProcessingComponentOptions noiseSuppressionOptions,
			LKRTCAudioProcessingComponentOptions autoGainControlOptions,
			LKRTCAudioProcessingComponentOptions highPassFilterOptions);

		[Static]
		[Export("communicationOptions")]
		LKRTCAudioProcessingOptions CommunicationOptions { get; }

		[Static]
		[Export("rawOptions")]
		LKRTCAudioProcessingOptions RawOptions { get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCAudioProcessingComponentState
	{
		[NullAllowed, Export("requested")]
		LKRTCAudioProcessingComponentOptions Requested { get; }

		[Export("softwareResolved")]
		bool SoftwareResolved { [Bind("isSoftwareResolved")] get; }

		[Export("softwareActive")]
		bool SoftwareActive { [Bind("isSoftwareActive")] get; }

		[Export("platformAvailable")]
		bool PlatformAvailable { [Bind("isPlatformAvailable")] get; }

		[Export("platformResolved")]
		bool PlatformResolved { [Bind("isPlatformResolved")] get; }

		[Export("platformActive")]
		bool PlatformActive { [Bind("isPlatformActive")] get; }

		[Export("effective")]
		LKRTCAudioProcessingImplementation Effective { get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCAudioProcessingState
	{
		[Export("hasAudioProcessingModule")]
		bool HasAudioProcessingModule { get; }

		[Export("echoCancellation")]
		LKRTCAudioProcessingComponentState EchoCancellation { get; }

		[Export("noiseSuppression")]
		LKRTCAudioProcessingComponentState NoiseSuppression { get; }

		[Export("autoGainControl")]
		LKRTCAudioProcessingComponentState AutoGainControl { get; }

		[Export("highPassFilter")]
		LKRTCAudioProcessingComponentState HighPassFilter { get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCPlatformAudioProcessingComponentState
	{
		[Export("available")]
		bool Available { [Bind("isAvailable")] get; }

		[Export("requested")]
		bool Requested { [Bind("isRequested")] get; }

		[Export("active")]
		bool Active { [Bind("isActive")] get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCPlatformAudioProcessingState
	{
		[Export("topology")]
		LKRTCPlatformAudioProcessingTopology Topology { get; }

		[Export("echoCancellation")]
		LKRTCPlatformAudioProcessingComponentState EchoCancellation { get; }

		[Export("noiseSuppression")]
		LKRTCPlatformAudioProcessingComponentState NoiseSuppression { get; }

		[Export("autoGainControl")]
		LKRTCPlatformAudioProcessingComponentState AutoGainControl { get; }

		[Export("voiceProcessingEnabledRequested")]
		bool VoiceProcessingEnabledRequested { [Bind("isVoiceProcessingEnabledRequested")] get; }

		[Export("voiceProcessingBypassedRequested")]
		bool VoiceProcessingBypassedRequested { [Bind("isVoiceProcessingBypassedRequested")] get; }

		[Export("voiceProcessingAGCEnabledRequested")]
		bool VoiceProcessingAgcEnabledRequested { [Bind("isVoiceProcessingAGCEnabledRequested")] get; }

		[Export("voiceProcessingEnabledActive")]
		bool VoiceProcessingEnabledActive { [Bind("isVoiceProcessingEnabledActive")] get; }

		[Export("voiceProcessingBypassedActive")]
		bool VoiceProcessingBypassedActive { [Bind("isVoiceProcessingBypassedActive")] get; }

		[Export("voiceProcessingAGCEnabledActive")]
		bool VoiceProcessingAgcEnabledActive { [Bind("isVoiceProcessingAGCEnabledActive")] get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCIODevice
	{
		[Static]
		[Export("defaultDeviceWithType:")]
		LKRTCIODevice DefaultDevice(LKRTCIODeviceType type);

		[Export("isDefault")]
		bool IsDefault { get; }

		[Export("type")]
		LKRTCIODeviceType Type { get; }

		[Export("deviceId")]
		string DeviceId { get; }

		[Export("name")]
		string Name { get; }
	}

	[Protocol, Model]
	[BaseType(typeof(NSObject))]
	interface LKRTCAudioDeviceModuleDelegate
	{
		[Abstract]
		[Export("audioDeviceModule:didReceiveSpeechActivityEvent:")]
		void DidReceiveSpeechActivityEvent(LKRTCAudioDeviceModule audioDeviceModule, LKRTCSpeechActivityEvent speechActivityEvent);

		[Abstract]
		[Export("audioDeviceModule:didCreateEngine:")]
		nint DidCreateEngine(LKRTCAudioDeviceModule audioDeviceModule, AVAudioEngine engine);

		[Abstract]
		[Export("audioDeviceModule:willEnableEngine:isPlayoutEnabled:isRecordingEnabled:")]
		nint WillEnableEngine(LKRTCAudioDeviceModule audioDeviceModule, AVAudioEngine engine, bool isPlayoutEnabled, bool isRecordingEnabled);

		[Abstract]
		[Export("audioDeviceModule:willStartEngine:isPlayoutEnabled:isRecordingEnabled:")]
		nint WillStartEngine(LKRTCAudioDeviceModule audioDeviceModule, AVAudioEngine engine, bool isPlayoutEnabled, bool isRecordingEnabled);

		[Abstract]
		[Export("audioDeviceModule:didStopEngine:isPlayoutEnabled:isRecordingEnabled:")]
		nint DidStopEngine(LKRTCAudioDeviceModule audioDeviceModule, AVAudioEngine engine, bool isPlayoutEnabled, bool isRecordingEnabled);

		[Abstract]
		[Export("audioDeviceModule:didDisableEngine:isPlayoutEnabled:isRecordingEnabled:")]
		nint DidDisableEngine(LKRTCAudioDeviceModule audioDeviceModule, AVAudioEngine engine, bool isPlayoutEnabled, bool isRecordingEnabled);

		[Abstract]
		[Export("audioDeviceModule:willReleaseEngine:")]
		nint WillReleaseEngine(LKRTCAudioDeviceModule audioDeviceModule, AVAudioEngine engine);

		[Abstract]
		[Export("audioDeviceModule:engine:configureInputFromSource:toDestination:withFormat:context:")]
		nint ConfigureInput(
			LKRTCAudioDeviceModule audioDeviceModule,
			AVAudioEngine engine,
			[NullAllowed] AVAudioNode source,
			AVAudioNode destination,
			AVAudioFormat format,
			NSDictionary context);

		[Abstract]
		[Export("audioDeviceModule:engine:configureOutputFromSource:toDestination:withFormat:context:")]
		nint ConfigureOutput(
			LKRTCAudioDeviceModule audioDeviceModule,
			AVAudioEngine engine,
			AVAudioNode source,
			[NullAllowed] AVAudioNode destination,
			AVAudioFormat format,
			NSDictionary context);

		[Abstract]
		[Export("audioDeviceModuleDidUpdateDevices:")]
		void DidUpdateDevices(LKRTCAudioDeviceModule audioDeviceModule);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCAudioDeviceModule
	{
		[Export("outputDevices")]
		LKRTCIODevice[] OutputDevices { get; }

		[Export("inputDevices")]
		LKRTCIODevice[] InputDevices { get; }

		[Export("playing")]
		bool Playing { get; }

		[Export("recording")]
		bool Recording { get; }

		[Export("outputDevice", ArgumentSemantic.Strong)]
		LKRTCIODevice OutputDevice { get; set; }

		[Export("inputDevice", ArgumentSemantic.Strong)]
		LKRTCIODevice InputDevice { get; set; }

		[Export("trySetOutputDevice:")]
		bool TrySetOutputDevice([NullAllowed] LKRTCIODevice device);

		[Export("trySetInputDevice:")]
		bool TrySetInputDevice([NullAllowed] LKRTCIODevice device);

		[Export("startPlayout")]
		nint StartPlayout();

		[Export("stopPlayout")]
		nint StopPlayout();

		[Export("initPlayout")]
		nint InitPlayout();

		[Export("startRecording")]
		nint StartRecording();

		[Export("stopRecording")]
		nint StopRecording();

		[Export("initRecording")]
		nint InitRecording();

		[Export("initAndStartRecording")]
		nint InitAndStartRecording();

		[Export("initAndStartRecordingWithAudioProcessingOptions:")]
		nint InitAndStartRecording([NullAllowed] LKRTCAudioProcessingOptions options);

		[Export("setEngineAvailability:")]
		nint SetEngineAvailability(LKRTCAudioEngineAvailability availability);

		[Export("isPlayoutInitialized")]
		bool IsPlayoutInitialized { get; }

		[Export("isRecordingInitialized")]
		bool IsRecordingInitialized { get; }

		[Export("isPlaying")]
		bool IsPlaying { get; }

		[Export("isRecording")]
		bool IsRecording { get; }

		[Export("isEngineRunning")]
		bool IsEngineRunning { get; }

		[Export("isMicrophoneMuted")]
		bool IsMicrophoneMuted { get; }

		[Export("setMicrophoneMuted:")]
		nint SetMicrophoneMuted(bool muted);

		[Export("engineState", ArgumentSemantic.Assign)]
		LKRTCAudioEngineState EngineState { get; set; }

		[Export("recordingAlwaysPreparedMode")]
		bool RecordingAlwaysPreparedMode { [Bind("isRecordingAlwaysPreparedMode")] get; }

		[Export("setRecordingAlwaysPreparedMode:")]
		nint SetRecordingAlwaysPreparedMode(bool enabled);

		[Export("setRecordingAlwaysPreparedMode:audioProcessingOptions:")]
		nint SetRecordingAlwaysPreparedMode(bool enabled, [NullAllowed] LKRTCAudioProcessingOptions options);

		[Wrap("WeakObserver")]
		[NullAllowed]
		ILKRTCAudioDeviceModuleDelegate Observer { get; set; }

		[NullAllowed, Export("observer", ArgumentSemantic.Weak)]
		NSObject WeakObserver { get; set; }

		[Export("manualRenderingMode")]
		bool ManualRenderingMode { [Bind("isManualRenderingMode")] get; }

		[Export("setManualRenderingMode:")]
		nint SetManualRenderingMode(bool enabled);

		[Export("advancedDuckingEnabled")]
		bool AdvancedDuckingEnabled { [Bind("isAdvancedDuckingEnabled")] get; set; }

		[Export("duckingLevel", ArgumentSemantic.Assign)]
		LKRTCAudioDuckingLevel DuckingLevel { get; set; }

		[Export("muteMode")]
		LKRTCAudioEngineMuteMode MuteMode { get; }

		[Export("setMuteMode:")]
		nint SetMuteMode(LKRTCAudioEngineMuteMode mode);

		[Export("platformVoiceProcessingAllowed")]
		bool PlatformVoiceProcessingAllowed { [Bind("isPlatformVoiceProcessingAllowed")] get; }

		[Export("setPlatformVoiceProcessingAllowed:")]
		nint SetPlatformVoiceProcessingAllowed(bool allowed);

		[Export("voiceProcessingBypassed")]
		bool VoiceProcessingBypassed { [Bind("isVoiceProcessingBypassed")] get; set; }

		[Export("voiceProcessingAGCEnabled")]
		bool VoiceProcessingAgcEnabled { [Bind("isVoiceProcessingAGCEnabled")] get; set; }

		[Export("engineAvailability")]
		LKRTCAudioEngineAvailability EngineAvailability { get; }

		[Export("platformAudioProcessingState")]
		LKRTCPlatformAudioProcessingState PlatformAudioProcessingState { get; }
	}

	[Category]
	[BaseType(typeof(LKRTCAudioTrack))]
	interface LKRTCAudioTrack_LiveKitAudio
	{
		[Export("addRenderer:")]
		void AddRenderer(ILKRTCAudioRenderer renderer);

		[Export("removeRenderer:")]
		void RemoveRenderer(ILKRTCAudioRenderer renderer);

		[Export("removeAllRenderers")]
		void RemoveAllRenderers();

		[Export("setAudioProcessingOptions:")]
		LKRTCAudioProcessingOptionsResult SetAudioProcessingOptions(LKRTCAudioProcessingOptions options);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCCodecSpecificInfoH265 : ILKRTCCodecSpecificInfo
	{
		[Export("packetizationMode", ArgumentSemantic.Assign)]
		RTCH265PacketizationMode PacketizationMode { get; set; }
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCVideoDecoderH265 : ILKRTCVideoDecoder
	{
		[Export("setHVCCFormat:size:width:height:")]
		unsafe nint SetHvccFormat(IntPtr data, nuint size, ushort width, ushort height);

		[Export("decodeData:size:timeStamp:")]
		unsafe nint DecodeData(IntPtr data, nuint size, long timeStamp);

		[Export("flush")]
		void Flush();
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCVideoEncoderH265 : ILKRTCVideoEncoder
	{
		[Export("initWithCodecInfo:")]
		NativeHandle Constructor(LKRTCVideoCodecInfo codecInfo);

		[Export("flush")]
		void Flush();
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCVideoEncoderCodecSupport
	{
		[Export("initWithSupported:")]
		NativeHandle Constructor(bool isSupported);

		[Export("initWithSupported:isPowerEfficient:")]
		[DesignatedInitializer]
		NativeHandle Constructor(bool isSupported, bool isPowerEfficient);

		[Export("isSupported")]
		bool IsSupported { get; }

		[Export("isPowerEfficient")]
		bool IsPowerEfficient { get; }
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCVideoEncoderFactorySimulcast : ILKRTCVideoEncoderFactory
	{
		[Export("initWithPrimary:fallback:")]
		NativeHandle Constructor(ILKRTCVideoEncoderFactory primary, ILKRTCVideoEncoderFactory fallback);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCVideoEncoderSimulcast
	{
		[Static]
		[Export("simulcastEncoderWithPrimary:fallback:videoCodecInfo:")]
		ILKRTCVideoEncoder CreateSimulcastEncoder(
			ILKRTCVideoEncoderFactory primary,
			ILKRTCVideoEncoderFactory fallback,
			LKRTCVideoCodecInfo videoCodecInfo);
	}

	[Protocol, Model]
	[BaseType(typeof(NSObject))]
	interface LKRTCFrameCryptorDelegate
	{
		[Abstract]
		[Export("frameCryptor:didStateChangeWithParticipantId:withState:")]
		void DidStateChange(LKRTCFrameCryptor frameCryptor, string participantId, LKRTCFrameCryptorState stateChanged);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCFrameCryptor
	{
		[Export("enabled")]
		bool Enabled { get; set; }

		[Export("keyIndex")]
		int KeyIndex { get; set; }

		[Export("participantId")]
		string ParticipantId { get; }

		[Wrap("WeakDelegate")]
		[NullAllowed]
		ILKRTCFrameCryptorDelegate Delegate { get; set; }

		[NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		[Export("initWithFactory:rtpSender:participantId:algorithm:keyProvider:")]
		NativeHandle Constructor(
			LKRTCPeerConnectionFactory factory,
			LKRTCRtpSender sender,
			string participantId,
			LKRTCCryptorAlgorithm algorithm,
			LKRTCFrameCryptorKeyProvider keyProvider);

		[Export("initWithFactory:rtpReceiver:participantId:algorithm:keyProvider:")]
		NativeHandle Constructor(
			LKRTCPeerConnectionFactory factory,
			LKRTCRtpReceiver receiver,
			string participantId,
			LKRTCCryptorAlgorithm algorithm,
			LKRTCFrameCryptorKeyProvider keyProvider);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCEncryptedPacket
	{
		[Export("data")]
		NSData Data { get; }

		[Export("iv")]
		NSData Iv { get; }

		[Export("keyIndex")]
		uint KeyIndex { get; }

		[Export("initWithData:iv:keyIndex:")]
		NativeHandle Constructor(NSData data, NSData iv, uint keyIndex);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCDataPacketCryptor
	{
		[Export("initWithAlgorithm:keyProvider:")]
		NativeHandle Constructor(LKRTCCryptorAlgorithm algorithm, LKRTCFrameCryptorKeyProvider keyProvider);

		[Export("encrypt:keyIndex:data:")]
		[return: NullAllowed]
		LKRTCEncryptedPacket Encrypt(string participantId, uint keyIndex, NSData data);

		[Export("decrypt:encryptedPacket:")]
		[return: NullAllowed]
		NSData Decrypt(string participantId, LKRTCEncryptedPacket packet);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCFrameCryptorKeyProvider
	{
		[Export("initWithRatchetSalt:ratchetWindowSize:sharedKeyMode:uncryptedMagicBytes:")]
		NativeHandle Constructor(NSData salt, int windowSize, bool sharedKey, [NullAllowed] NSData uncryptedMagicBytes);

		[Export("initWithRatchetSalt:ratchetWindowSize:sharedKeyMode:uncryptedMagicBytes:failureTolerance:keyRingSize:")]
		NativeHandle Constructor(NSData salt, int windowSize, bool sharedKey, [NullAllowed] NSData uncryptedMagicBytes, int failureTolerance, int keyRingSize);

		[Export("initWithRatchetSalt:ratchetWindowSize:sharedKeyMode:uncryptedMagicBytes:failureTolerance:keyRingSize:discardFrameWhenCryptorNotReady:")]
		NativeHandle Constructor(NSData salt, int windowSize, bool sharedKey, [NullAllowed] NSData uncryptedMagicBytes, int failureTolerance, int keyRingSize, bool discardFrameWhenCryptorNotReady);

		[Export("initWithRatchetSalt:ratchetWindowSize:sharedKeyMode:uncryptedMagicBytes:failureTolerance:keyRingSize:discardFrameWhenCryptorNotReady:keyDerivationAlgorithm:")]
		NativeHandle Constructor(NSData salt, int windowSize, bool sharedKey, [NullAllowed] NSData uncryptedMagicBytes, int failureTolerance, int keyRingSize, bool discardFrameWhenCryptorNotReady, LKRTCKeyDerivationAlgorithm keyDerivationAlgorithm);

		[Export("setSharedKey:withIndex:")]
		void SetSharedKey(NSData key, int index);

		[Export("ratchetSharedKey:")]
		NSData RatchetSharedKey(int index);

		[Export("exportSharedKey:")]
		NSData ExportSharedKey(int index);

		[Export("setKey:withIndex:forParticipant:")]
		void SetKey(NSData key, int index, string participantId);

		[Export("ratchetKey:withIndex:")]
		NSData RatchetKey(string participantId, int index);

		[Export("exportKey:withIndex:")]
		NSData ExportKey(string participantId, int index);

		[Export("setSifTrailer:")]
		void SetSifTrailer(NSData trailer);
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCRtpCapabilities
	{
		[Export("codecs", ArgumentSemantic.Copy)]
		LKRTCRtpCodecCapability[] Codecs { get; set; }

		[Export("headerExtensions", ArgumentSemantic.Copy)]
		LKRTCRtpHeaderExtensionCapability[] HeaderExtensions { get; set; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCRtpCodecCapability
	{
		[NullAllowed, Export("preferredPayloadType")]
		NSNumber PreferredPayloadType { get; }

		[Export("name")]
		string Name { get; }

		[Export("kind")]
		string Kind { get; }

		[NullAllowed, Export("clockRate")]
		NSNumber ClockRate { get; }

		[NullAllowed, Export("numChannels")]
		NSNumber NumChannels { get; }

		[Export("parameters")]
		NSDictionary<NSString, NSString> Parameters { get; }

		[Export("mimeType")]
		string MimeType { get; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCRtpHeaderExtensionCapability
	{
		[Export("uri")]
		string Uri { get; }

		[NullAllowed, Export("preferredId")]
		NSNumber PreferredId { get; }

		[Export("preferredEncrypted")]
		bool PreferredEncrypted { [Bind("isPreferredEncrypted")] get; }

		[Export("direction", ArgumentSemantic.Assign)]
		LKRTCRtpTransceiverDirection Direction { get; set; }
	}

	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCRtpSource : ILKRTCRtpSource
	{
		[Export("sourceId")]
		uint SourceId { get; }

		[Export("sourceType")]
		RTCRtpSourceType SourceType { get; }

		[NullAllowed, Export("audioLevel")]
		NSNumber AudioLevel { get; }

		[Export("timestampUs")]
		double TimestampUs { get; }

		[Export("rtpTimestamp")]
		uint RtpTimestamp { get; }
	}

	[BaseType(typeof(NSObject))]
	interface LKRTCYUVHelper
	{
		[Static]
		[Export("I420Rotate:srcStrideY:srcU:srcStrideU:srcV:srcStrideV:dstY:dstStrideY:dstU:dstStrideU:dstV:dstStrideV:width:height:mode:")]
		nint I420Rotate(
			IntPtr srcY,
			int srcStrideY,
			IntPtr srcU,
			int srcStrideU,
			IntPtr srcV,
			int srcStrideV,
			IntPtr dstY,
			int dstStrideY,
			IntPtr dstU,
			int dstStrideU,
			IntPtr dstV,
			int dstStrideV,
			int width,
			int height,
			LKRTCVideoRotation mode);

		[Static]
		[Export("I420ToNV12:srcStrideY:srcU:srcStrideU:srcV:srcStrideV:dstY:dstStrideY:dstUV:dstStrideUV:width:height:")]
		int I420ToNV12(IntPtr srcY, int srcStrideY, IntPtr srcU, int srcStrideU, IntPtr srcV, int srcStrideV, IntPtr dstY, int dstStrideY, IntPtr dstUv, int dstStrideUv, int width, int height);

		[Static]
		[Export("I420ToNV21:srcStrideY:srcU:srcStrideU:srcV:srcStrideV:dstY:dstStrideY:dstUV:dstStrideUV:width:height:")]
		int I420ToNV21(IntPtr srcY, int srcStrideY, IntPtr srcU, int srcStrideU, IntPtr srcV, int srcStrideV, IntPtr dstY, int dstStrideY, IntPtr dstUv, int dstStrideUv, int width, int height);

		[Static]
		[Export("I420ToARGB:srcStrideY:srcU:srcStrideU:srcV:srcStrideV:dstARGB:dstStrideARGB:width:height:")]
		int I420ToArgb(IntPtr srcY, int srcStrideY, IntPtr srcU, int srcStrideU, IntPtr srcV, int srcStrideV, IntPtr dstArgb, int dstStrideArgb, int width, int height);
	}

	partial interface Constants
	{
		[Field("kLKRTCAudioEngineInputMixerNodeKey", "__Internal")]
		NSString kLKRTCAudioEngineInputMixerNodeKey { get; }

		[Field("kLKRTCVideoCodecH265Name", "__Internal")]
		NSString kLKRTCVideoCodecH265Name { get; }

		[Field("kLKRTCLevel31Main", "__Internal")]
		NSString kLKRTCLevel31Main { get; }
	}
}
