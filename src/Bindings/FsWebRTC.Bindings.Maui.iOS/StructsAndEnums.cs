using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
////using WebRTC;

namespace FsWebRTC.Bindings
{
	[Native]
	public enum LKRTCVideoRotation : long
	{
		LKRTCVideoRotation_0 = 0,
		LKRTCVideoRotation_90 = 90,
		LKRTCVideoRotation_180 = 180,
		LKRTCVideoRotation_270 = 270
	}

	[Native]
	public enum LKRTCFrameType : ulong
	{
		EmptyFrame = 0,
		AudioFrameSpeech = 1,
		AudioFrameCN = 2,
		VideoFrameKey = 3,
		VideoFrameDelta = 4
	}

	[Native]
	public enum LKRTCVideoContentType : ulong
	{
		Unspecified,
		Screenshare
	}

	[Native]
	public enum LKRTCLoggingSeverity : long
	{
		Verbose,
		Info,
		Warning,
		Error,
		None
	}

	static class CFunctions
	{
		// extern void LKRTCLogEx (LKRTCLoggingSeverity severity, NSString *log_string);
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void LKRTCLogEx (LKRTCLoggingSeverity severity, NSString log_string);

		// extern void LKRTCSetMinDebugLogLevel (LKRTCLoggingSeverity severity);
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void LKRTCSetMinDebugLogLevel (LKRTCLoggingSeverity severity);

		// extern NSString * LKRTCFileName (const char *filePath);
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern unsafe NSString LKRTCFileName (sbyte* filePath);

		// extern void LKRTCInitFieldTrialDictionary (NSDictionary<NSString *,NSString *> *fieldTrials);
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void LKRTCInitFieldTrialDictionary (NSDictionary<NSString, NSString> fieldTrials);

		// extern void RTCEnableMetrics ();
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void RTCEnableMetrics ();

		// extern NSArray<LKRTCMetricsSampleInfo *> * RTCGetAndResetMetrics ();
		//[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		//static extern LKRTCMetricsSampleInfo[] RTCGetAndResetMetrics ();

		// extern BOOL LKRTCInitializeSSL ();
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern bool LKRTCInitializeSSL ();

		// extern BOOL LKRTCCleanupSSL ();
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern bool LKRTCCleanupSSL ();

		// extern void LKRTCSetupInternalTracer ();
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void LKRTCSetupInternalTracer ();

		// extern BOOL LKRTCStartInternalCapture (NSString *filePath);
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern bool LKRTCStartInternalCapture (NSString filePath);

		// extern void LKRTCStopInternalCapture ();
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void LKRTCStopInternalCapture ();

		// extern void LKRTCShutdownInternalTracer ();
		[DllImport ("__Internal")]
		////[Verify (PlatformInvoke)]
		static extern void LKRTCShutdownInternalTracer ();
	}

	[Native]
	public enum LKRTCVideoCodecMode : ulong
	{
		RealtimeVideo,
		Screensharing
	}

	[Native]
	public enum LKRTCH264PacketizationMode : ulong
	{
		NonInterleaved = 0,
		SingleNalUnit
	}

	[Native]
	public enum LKRTCH264Profile : ulong
	{
		ConstrainedBaseline,
		Baseline,
		Main,
		ConstrainedHigh,
		High
	}

	[Native]
	public enum LKRTCH264Level : ulong
	{
		LKRTCH264Level1_b = 0,
		LKRTCH264Level1 = 10,
		LKRTCH264Level1_1 = 11,
		LKRTCH264Level1_2 = 12,
		LKRTCH264Level1_3 = 13,
		LKRTCH264Level2 = 20,
		LKRTCH264Level2_1 = 21,
		LKRTCH264Level2_2 = 22,
		LKRTCH264Level3 = 30,
		LKRTCH264Level3_1 = 31,
		LKRTCH264Level3_2 = 32,
		LKRTCH264Level4 = 40,
		LKRTCH264Level4_1 = 41,
		LKRTCH264Level4_2 = 42,
		LKRTCH264Level5 = 50,
		LKRTCH264Level5_1 = 51,
		LKRTCH264Level5_2 = 52
	}

	[Native]
	public enum LKRTCDispatcherQueueType : long
	{
		Main,
		CaptureSession,
		AudioSession,
		NetworkMonitor
	}

	[Native]
	public enum LKRTCSourceState : long
	{
		Initializing,
		Live,
		Ended,
		Muted
	}

	[Native]
	public enum LKRTCMediaStreamTrackState : long
	{
		Live,
		Ended
	}

	[Native]
	public enum LKRTCIceTransportPolicy : long
	{
		None,
		Relay,
		NoHost,
		All
	}

	[Native]
	public enum LKRTCBundlePolicy : long
	{
		Balanced,
		MaxCompat,
		MaxBundle
	}

	[Native]
	public enum LKRTCRtcpMuxPolicy : long
	{
		Negotiate,
		Require
	}

	[Native]
	public enum LKRTCTcpCandidatePolicy : long
	{
		Enabled,
		Disabled
	}

	[Native]
	public enum LKRTCCandidateNetworkPolicy : long
	{
		All,
		LowCost
	}

	[Native]
	public enum LKRTCContinualGatheringPolicy : long
	{
		Once,
		Continually
	}

	[Native]
	public enum LKRTCEncryptionKeyType : long
	{
		Rsa,
		Ecdsa
	}

	[Native]
	public enum LKRTCSdpSemantics : long
	{
		PlanB,
		UnifiedPlan
	}

	[Native]
	public enum LKRTCDataChannelState : long
	{
		Connecting,
		Open,
		Closing,
		Closed
	}

	[Native]
	public enum LKRTCTlsCertPolicy : ulong
	{
		Secure,
		InsecureNoCheck
	}

	[Native]
	public enum LKRTCSignalingState : long
	{
		Stable,
		HaveLocalOffer,
		HaveLocalPrAnswer,
		HaveRemoteOffer,
		HaveRemotePrAnswer,
		Closed
	}

	[Native]
	public enum LKRTCIceConnectionState : long
	{
		New,
		Checking,
		Connected,
		Completed,
		Failed,
		Disconnected,
		Closed,
		Count
	}

	[Native]
	public enum LKRTCPeerConnectionState : long
	{
		New,
		Connecting,
		Connected,
		Disconnected,
		Failed,
		Closed
	}

	[Native]
	public enum LKRTCIceGatheringState : long
	{
		New,
		Gathering,
		Complete
	}

	[Native]
	public enum LKRTCStatsOutputLevel : long
	{
		Standard,
		Debug
	}

	[Native]
	public enum LKRTCPriority : long
	{
		VeryLow,
		Low,
		Medium,
		High
	}

	[Native]
	public enum LKRTCDegradationPreference : long
	{
		Disabled,
		MaintainFramerate,
		MaintainResolution,
		Balanced
	}

	[Native]
	public enum LKRTCRtpMediaType : long
	{
		Audio,
		Video,
		Data,
		Unsupported
	}

	[Native]
	public enum RTCRtpSourceType : long
	{
		Contributing,
		Synchronization
	}

	[Native]
	public enum LKRTCRtpTransceiverDirection : long
	{
		SendRecv,
		SendOnly,
		RecvOnly,
		Inactive,
		Stopped
	}

	[Native]
	public enum LKRTCSdpType : long
	{
		Offer,
		PrAnswer,
		Answer,
		Rollback
	}

	[Native]
	public enum LKRTCFileLoggerSeverity : ulong
	{
		Verbose,
		Info,
		Warning,
		Error
	}

	[Native]
	public enum LKRTCFileLoggerRotationType : ulong
	{
		Call,
		App
	}

	[Native]
	public enum LKRTCIODeviceType : long
	{
		Output,
		Input
	}

	[Native]
	public enum LKRTCAudioDeviceModuleType : long
	{
		PlatformDefault,
		AudioEngine
	}

	[Native]
	public enum LKRTCSpeechActivityEvent : long
	{
		Started,
		Ended
	}

	[Native]
	public enum LKRTCAudioEngineMuteMode : long
	{
		Unknown = -1,
		VoiceProcessing = 0,
		RestartEngine = 1,
		InputMixer = 2
	}

	[Native]
	public enum LKRTCAudioDuckingLevel : long
	{
		Default = 0,
		Min = 1,
		Mid = 2,
		Max = 3
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LKRTCAudioEngineState
	{
		public bool outputEnabled;
		public bool outputRunning;
		public bool inputEnabled;
		public bool inputRunning;
		public bool inputMuted;
		public LKRTCAudioEngineMuteMode muteMode;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LKRTCAudioEngineAvailability
	{
		public bool isInputAvailable;
		public bool isOutputAvailable;
	}

	[Native]
	public enum LKRTCPlatformAudioProcessingTopology : long
	{
		Independent = 0,
		EchoCancellationAndNoiseSuppressionCoupled = 1
	}

	[Native]
	public enum LKRTCAudioProcessingMode : long
	{
		Automatic = 0,
		Platform = 1,
		Software = 2
	}

	[Native]
	public enum LKRTCAudioProcessingOptionsResultCode : long
	{
		Applied = 0,
		Stored = 1,
		RejectedRemoteTrack = 2,
		RejectedInvalidCombination = 3,
		RejectedPlatformUnavailable = 4,
		ApplyFailed = 5
	}

	[Native]
	public enum LKRTCAudioProcessingImplementation : long
	{
		Unknown = 0,
		Disabled = 1,
		Software = 2,
		Platform = 3,
		SoftwareAndPlatform = 4
	}

	[Native]
	public enum LKRTCCryptorAlgorithm : ulong
	{
		AesGcm = 0
	}

	[Native]
	public enum LKRTCFrameCryptorState : long
	{
		New = 0,
		Ok,
		EncryptionFailed,
		DecryptionFailed,
		MissingKey,
		KeyRatcheted,
		InternalError
	}

	[Native]
	public enum LKRTCKeyDerivationAlgorithm : ulong
	{
		Pbkdf2 = 0,
		Hkdf
	}

	[Native]
	public enum RTCH265PacketizationMode : ulong
	{
		NonInterleaved = 0,
		SingleNalUnit
	}
}
