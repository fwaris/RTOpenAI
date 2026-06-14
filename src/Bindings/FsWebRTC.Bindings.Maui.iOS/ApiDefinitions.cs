using System;
using AVFoundation;
using AudioToolbox;
using AudioUnit;
////using CoreAudioTypes;
using CoreGraphics;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using UIKit;
////using WebRTC;

namespace FsWebRTC.Bindings
{
    //// WebRTCme additions.
    delegate void dispatch_block_t();

    interface ILKRTCVideoFrameBuffer 
	{
        [Abstract]
        [Export("width")]
        int Width { get; }

        // @required @property (readonly, nonatomic) int height;
        [Abstract]
        [Export("height")]
        int Height { get; }

        // @required -(id<LKRTCI420Buffer> _Nonnull)toI420;
        [Abstract]
        [Export("toI420")]
        ////[Verify (MethodToProperty)]
        LKRTCI420Buffer ToI420 { get; }

    }
    interface ILKRTCMutableI420Buffer { }
	interface ILKRTCVideoRenderer 
	{
        [Abstract]
        [Export("setSize:")]
        void SetSize(CGSize size);

        // @required -(void)renderFrame:(LKRTCVideoFrame * _Nullable)frame;
        [Abstract]
        [Export("renderFrame:")]
        void RenderFrame([NullAllowed] LKRTCVideoFrame frame);
    }
    interface ILKRTCYUVPlanarBuffer : ILKRTCVideoFrameBuffer
	{
        [Abstract]
        [Export("chromaWidth")]
        int ChromaWidth { get; }

        // @required @property (readonly, nonatomic) int chromaHeight;
        [Abstract]
        [Export("chromaHeight")]
        int ChromaHeight { get; }

        // @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataY;
        [Abstract]
        [Export("dataY")]
        /****unsafe byte* ****/
        IntPtr DataY { get; }

        // @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataU;
        [Abstract]
        [Export("dataU")]
        /****unsafebyte* ****/
        IntPtr DataU { get; }

        // @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataV;
        [Abstract]
        [Export("dataV")]
        /****unsafebyte* ****/
        IntPtr DataV { get; }

        // @required @property (readonly, nonatomic) int strideY;
        [Abstract]
        [Export("strideY")]
        int StrideY { get; }

        // @required @property (readonly, nonatomic) int strideU;
        [Abstract]
        [Export("strideU")]
        int StrideU { get; }

        // @required @property (readonly, nonatomic) int strideV;
        [Abstract]
        [Export("strideV")]
        int StrideV { get; }

        // @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height dataY:(const uint8_t * _Nonnull)dataY dataU:(const uint8_t * _Nonnull)dataU dataV:(const uint8_t * _Nonnull)dataV;
        [Abstract]
        [Export("initWithWidth:height:dataY:dataU:dataV:")]
        /****unsafe****/
        NativeHandle /****Height****/InitWithWidth(int width, int height, /****byte* ****/IntPtr dataY, /****byte* ****/IntPtr dataU, /****byte* ***/IntPtr dataV);

        // @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height;
        [Abstract]
        [Export("initWithWidth:height:")]
        NativeHandle /****Height****/InitWithWidth(int width, int height);

        // @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height strideY:(int)strideY strideU:(int)strideU strideV:(int)strideV;
        [Abstract]
        [Export("initWithWidth:height:strideY:strideU:strideV:")]
        NativeHandle /****Height****/InitWithWidth(int width, int height, int strideY, int strideU, int strideV);

    }
    interface ILKRTCVideoDecoderFactory { }
    interface ILKRTCVideoEncoderFactory { }
	interface ILKRTCVideoEncoder { }
	interface ILKRTCI420Buffer { }
	interface ILKRTCAudioSessionActivationDelegate { }
    interface ILKRTCVideoDecoder { }
    interface ILKRTCCodecSpecificInfo { }
	interface ILKRTCVideoCapturerDelegate 
	{
        [Abstract]
        [Export("capturer:didCaptureVideoFrame:")]
        void DidCaptureVideoFrame(LKRTCVideoCapturer capturer, LKRTCVideoFrame frame);

    }
    interface ILKRTCDataChannelDelegate { }
	interface ILKRTCPeerConnectionDelegate { }
	interface ILKRTCVideoViewDelegate { }
    interface ILKRTCRtpReceiverDelegate { }

    // @protocol LKRTCCodecSpecificInfo <NSObject>
    /*
    Check whether adding [Model] to this declaration is appropriate.
    [Model] is used to generate a C# class that implements this protocol,
    and might be useful for protocols that consumers are supposed to implement,
    since consumers can subclass the generated class instead of implementing
    the generated interface. If consumers are not supposed to implement this
    protocol, then [Model] is redundant and will generate code that will never
    be used.
    */
    [Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCCodecSpecificInfo
	{
	}

	// @interface LKRTCVideoFrame : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCVideoFrame
	{
		// @property (readonly, nonatomic) int width;
		[Export ("width")]
		int Width { get; }

		// @property (readonly, nonatomic) int height;
		[Export ("height")]
		int Height { get; }

		// @property (readonly, nonatomic) LKRTCVideoRotation rotation;
		[Export ("rotation")]
		LKRTCVideoRotation Rotation { get; }

		// @property (readonly, nonatomic) int64_t timeStampNs;
		[Export ("timeStampNs")]
		long TimeStampNs { get; }

		// @property (assign, nonatomic) int32_t timeStamp;
		[Export ("timeStamp")]
		int TimeStamp { get; set; }

		// @property (readonly, nonatomic) id<LKRTCVideoFrameBuffer> _Nonnull buffer;
		[Export ("buffer")]
		LKRTCVideoFrameBuffer Buffer { get; }

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer rotation:(LKRTCVideoRotation)rotation timeStampNs:(int64_t)timeStampNs __attribute__((deprecated("use initWithBuffer instead")));
		[Export ("initWithPixelBuffer:rotation:timeStampNs:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer, LKRTCVideoRotation rotation, long timeStampNs);

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer scaledWidth:(int)scaledWidth scaledHeight:(int)scaledHeight cropWidth:(int)cropWidth cropHeight:(int)cropHeight cropX:(int)cropX cropY:(int)cropY rotation:(LKRTCVideoRotation)rotation timeStampNs:(int64_t)timeStampNs __attribute__((deprecated("use initWithBuffer instead")));
		[Export ("initWithPixelBuffer:scaledWidth:scaledHeight:cropWidth:cropHeight:cropX:cropY:rotation:timeStampNs:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer, int scaledWidth, int scaledHeight, int cropWidth, int cropHeight, int cropX, int cropY, LKRTCVideoRotation rotation, long timeStampNs);

		// -(instancetype _Nonnull)initWithBuffer:(id<LKRTCVideoFrameBuffer> _Nonnull)frameBuffer rotation:(LKRTCVideoRotation)rotation timeStampNs:(int64_t)timeStampNs;
		[Export ("initWithBuffer:rotation:timeStampNs:")]
		NativeHandle Constructor (LKRTCVideoFrameBuffer frameBuffer, LKRTCVideoRotation rotation, long timeStampNs);

		// -(LKRTCVideoFrame * _Nonnull)newI420VideoFrame;
		[Export ("newI420VideoFrame")]
		////[Verify (MethodToProperty)]
		LKRTCVideoFrame NewI420VideoFrame { get; }
	}

	// @interface LKRTCEncodedImage : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCEncodedImage
	{
		// @property (nonatomic, strong) NSData * _Nonnull buffer;
		[Export ("buffer", ArgumentSemantic.Strong)]
		NSData Buffer { get; set; }

		// @property (assign, nonatomic) int32_t encodedWidth;
		[Export ("encodedWidth")]
		int EncodedWidth { get; set; }

		// @property (assign, nonatomic) int32_t encodedHeight;
		[Export ("encodedHeight")]
		int EncodedHeight { get; set; }

		// @property (assign, nonatomic) uint32_t timeStamp;
		[Export ("timeStamp")]
		uint TimeStamp { get; set; }

		// @property (assign, nonatomic) int64_t captureTimeMs;
		[Export ("captureTimeMs")]
		long CaptureTimeMs { get; set; }

		// @property (assign, nonatomic) int64_t ntpTimeMs;
		[Export ("ntpTimeMs")]
		long NtpTimeMs { get; set; }

		// @property (assign, nonatomic) uint8_t flags;
		[Export ("flags")]
		byte Flags { get; set; }

		// @property (assign, nonatomic) int64_t encodeStartMs;
		[Export ("encodeStartMs")]
		long EncodeStartMs { get; set; }

		// @property (assign, nonatomic) int64_t encodeFinishMs;
		[Export ("encodeFinishMs")]
		long EncodeFinishMs { get; set; }

		// @property (assign, nonatomic) LKRTCFrameType frameType;
		[Export ("frameType", ArgumentSemantic.Assign)]
		LKRTCFrameType FrameType { get; set; }

		// @property (assign, nonatomic) LKRTCVideoRotation rotation;
		[Export ("rotation", ArgumentSemantic.Assign)]
		LKRTCVideoRotation Rotation { get; set; }

		// @property (nonatomic, strong) NSNumber * _Nonnull qp;
		[Export ("qp", ArgumentSemantic.Strong)]
		NSNumber Qp { get; set; }

		// @property (assign, nonatomic) LKRTCVideoContentType contentType;
		[Export ("contentType", ArgumentSemantic.Assign)]
		LKRTCVideoContentType ContentType { get; set; }
	}

	// @protocol LKRTCVideoFrameBuffer <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoFrameBuffer
	{
		// @required @property (readonly, nonatomic) int width;
		[Abstract]
		[Export ("width")]
		int Width { get; }

		// @required @property (readonly, nonatomic) int height;
		[Abstract]
		[Export ("height")]
		int Height { get; }

		// @required -(id<LKRTCI420Buffer> _Nonnull)toI420;
		[Abstract]
		[Export ("toI420")]
		////[Verify (MethodToProperty)]
		LKRTCI420Buffer ToI420 { get; }

		// @optional -(id<LKRTCVideoFrameBuffer> _Nonnull)cropAndScaleWith:(int)offsetX offsetY:(int)offsetY cropWidth:(int)cropWidth cropHeight:(int)cropHeight scaleWidth:(int)scaleWidth scaleHeight:(int)scaleHeight;
		[Export ("cropAndScaleWith:offsetY:cropWidth:cropHeight:scaleWidth:scaleHeight:")]
		LKRTCVideoFrameBuffer OffsetY (int offsetX, int offsetY, int cropWidth, int cropHeight, int scaleWidth, int scaleHeight);
	}

	// @protocol LKRTCYUVPlanarBuffer <LKRTCVideoFrameBuffer>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	interface LKRTCYUVPlanarBuffer : /****I****/LKRTCVideoFrameBuffer
	{
		// @required @property (readonly, nonatomic) int chromaWidth;
		[Abstract]
		[Export ("chromaWidth")]
		int ChromaWidth { get; }

		// @required @property (readonly, nonatomic) int chromaHeight;
		[Abstract]
		[Export ("chromaHeight")]
		int ChromaHeight { get; }

		// @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataY;
		[Abstract]
		[Export ("dataY")]
        /****unsafe byte* ****/IntPtr DataY { get; }

		// @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataU;
		[Abstract]
		[Export ("dataU")]
        /****unsafebyte* ****/IntPtr DataU { get; }

		// @required @property (readonly, nonatomic) const uint8_t * _Nonnull dataV;
		[Abstract]
		[Export ("dataV")]
        /****unsafebyte* ****/IntPtr DataV { get; }

		// @required @property (readonly, nonatomic) int strideY;
		[Abstract]
		[Export ("strideY")]
		int StrideY { get; }

		// @required @property (readonly, nonatomic) int strideU;
		[Abstract]
		[Export ("strideU")]
		int StrideU { get; }

		// @required @property (readonly, nonatomic) int strideV;
		[Abstract]
		[Export ("strideV")]
		int StrideV { get; }

		// @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height dataY:(const uint8_t * _Nonnull)dataY dataU:(const uint8_t * _Nonnull)dataU dataV:(const uint8_t * _Nonnull)dataV;
		[Abstract]
		[Export ("initWithWidth:height:dataY:dataU:dataV:")]
		/****unsafe****/ NativeHandle /****Height****/InitWithWidth(int width, int height, /****byte* ****/IntPtr dataY, /****byte* ****/IntPtr dataU, /****byte* ***/IntPtr dataV);

		// @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height;
		[Abstract]
		[Export ("initWithWidth:height:")]
		NativeHandle /****Height****/InitWithWidth(int width, int height);

		// @required -(instancetype _Nonnull)initWithWidth:(int)width height:(int)height strideY:(int)strideY strideU:(int)strideU strideV:(int)strideV;
		[Abstract]
		[Export ("initWithWidth:height:strideY:strideU:strideV:")]
		NativeHandle /****Height****/InitWithWidth(int width, int height, int strideY, int strideU, int strideV);
	}

	// @protocol LKRTCI420Buffer <LKRTCYUVPlanarBuffer>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/////[Protocol]
	////interface LKRTCI420Buffer : ILKRTCYUVPlanarBuffer
	////{
	////}

	// @protocol LKRTCMutableYUVPlanarBuffer <LKRTCYUVPlanarBuffer>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	interface LKRTCMutableYUVPlanarBuffer : /****I****/LKRTCYUVPlanarBuffer
	{
		// @required @property (readonly, nonatomic) uint8_t * _Nonnull mutableDataY;
		[Abstract]
		[Export ("mutableDataY")]
		/****unsafe byte* ****/IntPtr MutableDataY { get; }

		// @required @property (readonly, nonatomic) uint8_t * _Nonnull mutableDataU;
		[Abstract]
		[Export ("mutableDataU")]
		/****unsafe byte* ****/IntPtr MutableDataU { get; }

		// @required @property (readonly, nonatomic) uint8_t * _Nonnull mutableDataV;
		[Abstract]
		[Export ("mutableDataV")]
		/**** unsafe byte* ****/IntPtr MutableDataV { get; }
	}

    // @protocol LKRTCMutableI420Buffer <LKRTCI420Buffer, LKRTCMutableYUVPlanarBuffer>
    /*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/////[Protocol]
  ////interface LKRTCMutableI420Buffer : ILKRTCI420Buffer, ILKRTCMutableYUVPlanarBuffer
  ////{
  ////}

    // @protocol LKRTCSSLCertificateVerifier <NSObject>
    /*
    Check whether adding [Model] to this declaration is appropriate.
    [Model] is used to generate a C# class that implements this protocol,
    and might be useful for protocols that consumers are supposed to implement,
    since consumers can subclass the generated class instead of implementing
    the generated interface. If consumers are not supposed to implement this
    protocol, then [Model] is redundant and will generate code that will never
    be used.
    */
    [Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCSSLCertificateVerifier
	{
		// @required -(BOOL)verify:(NSData * _Nonnull)derCertificate;
		[Abstract]
		[Export ("verify:")]
		bool Verify (NSData derCertificate);
	}

	// @protocol LKRTCVideoCapturerDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoCapturerDelegate
	{
		// @required -(void)capturer:(LKRTCVideoCapturer * _Nonnull)capturer didCaptureVideoFrame:(LKRTCVideoFrame * _Nonnull)frame;
		[Abstract]
		[Export ("capturer:didCaptureVideoFrame:")]
		void DidCaptureVideoFrame (LKRTCVideoCapturer capturer, LKRTCVideoFrame frame);
	}

	// @interface LKRTCVideoCapturer : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoCapturer
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ILKRTCVideoCapturerDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<LKRTCVideoCapturerDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// -(instancetype _Nonnull)initWithDelegate:(id<LKRTCVideoCapturerDelegate> _Nonnull)delegate;
		[Export ("initWithDelegate:")]
		NativeHandle Constructor (LKRTCVideoCapturerDelegate @delegate);
	}

	// @interface LKRTCVideoCodecInfo : NSObject <NSCoding>
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCVideoCodecInfo : INSCoding
	{
		// -(instancetype _Nonnull)initWithName:(NSString * _Nonnull)name;
		[Export ("initWithName:")]
		NativeHandle Constructor (string name);

		// -(instancetype _Nonnull)initWithName:(NSString * _Nonnull)name parameters:(NSDictionary<NSString *,NSString *> * _Nullable)parameters __attribute__((objc_designated_initializer));
		[Export ("initWithName:parameters:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string name, [NullAllowed] NSDictionary<NSString, NSString> parameters);

		// -(BOOL)isEqualToCodecInfo:(LKRTCVideoCodecInfo * _Nonnull)info;
		[Export ("isEqualToCodecInfo:")]
		bool IsEqualToCodecInfo (LKRTCVideoCodecInfo info);

		// @property (readonly, nonatomic) NSString * _Nonnull name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,NSString *> * _Nonnull parameters;
		[Export ("parameters")]
		NSDictionary<NSString, NSString> Parameters { get; }
	}

	// @interface LKRTCVideoEncoderSettings : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderSettings
	{
		// @property (nonatomic, strong) NSString * _Nonnull name;
		[Export ("name", ArgumentSemantic.Strong)]
		string Name { get; set; }

		// @property (assign, nonatomic) unsigned short width;
		[Export ("width")]
		ushort Width { get; set; }

		// @property (assign, nonatomic) unsigned short height;
		[Export ("height")]
		ushort Height { get; set; }

		// @property (assign, nonatomic) unsigned int startBitrate;
		[Export ("startBitrate")]
		uint StartBitrate { get; set; }

		// @property (assign, nonatomic) unsigned int maxBitrate;
		[Export ("maxBitrate")]
		uint MaxBitrate { get; set; }

		// @property (assign, nonatomic) unsigned int minBitrate;
		[Export ("minBitrate")]
		uint MinBitrate { get; set; }

		// @property (assign, nonatomic) uint32_t maxFramerate;
		[Export ("maxFramerate")]
		uint MaxFramerate { get; set; }

		// @property (assign, nonatomic) unsigned int qpMax;
		[Export ("qpMax")]
		uint QpMax { get; set; }

		// @property (assign, nonatomic) LKRTCVideoCodecMode mode;
		[Export ("mode", ArgumentSemantic.Assign)]
		LKRTCVideoCodecMode Mode { get; set; }
	}

	// typedef void (^LKRTCVideoDecoderCallback)(LKRTCVideoFrame * _Nonnull);
	delegate void LKRTCVideoDecoderCallback (LKRTCVideoFrame arg0);

	// @protocol LKRTCVideoDecoder <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoder
	{
		// @required -(void)setCallback:(LKRTCVideoDecoderCallback _Nonnull)callback;
		[Abstract]
		[Export ("setCallback:")]
		void SetCallback (LKRTCVideoDecoderCallback callback);

		// @required -(NSInteger)startDecodeWithNumberOfCores:(int)numberOfCores;
		[Abstract]
		[Export ("startDecodeWithNumberOfCores:")]
		nint StartDecodeWithNumberOfCores (int numberOfCores);

		// @required -(NSInteger)releaseDecoder;
		[Abstract]
		[Export ("releaseDecoder")]
		////[Verify (MethodToProperty)]
		nint ReleaseDecoder { get; }

		// @required -(NSInteger)decode:(LKRTCEncodedImage * _Nonnull)encodedImage missingFrames:(BOOL)missingFrames codecSpecificInfo:(id<LKRTCCodecSpecificInfo> _Nullable)info renderTimeMs:(int64_t)renderTimeMs;
		[Abstract]
		[Export ("decode:missingFrames:codecSpecificInfo:renderTimeMs:")]
		nint Decode (LKRTCEncodedImage encodedImage, bool missingFrames, [NullAllowed] LKRTCCodecSpecificInfo info, long renderTimeMs);

		// @required -(NSString * _Nonnull)implementationName;
		[Abstract]
		[Export ("implementationName")]
		////[Verify (MethodToProperty)]
		string ImplementationName { get; }
	}

	// @protocol LKRTCVideoDecoderFactory <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoderFactory
	{
		// @required -(id<LKRTCVideoDecoder> _Nullable)createDecoder:(LKRTCVideoCodecInfo * _Nonnull)info;
		[Abstract]
		[Export ("createDecoder:")]
		[return: NullAllowed]
		LKRTCVideoDecoder CreateDecoder (LKRTCVideoCodecInfo info);

		// @required -(NSArray<LKRTCVideoCodecInfo *> * _Nonnull)supportedCodecs;
		[Abstract]
		[Export ("supportedCodecs")]
		////[Verify (MethodToProperty)]
		LKRTCVideoCodecInfo[] SupportedCodecs { get; }
	}

	// @interface LKRTCVideoEncoderQpThresholds : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderQpThresholds
	{
		// -(instancetype _Nonnull)initWithThresholdsLow:(NSInteger)low high:(NSInteger)high;
		[Export ("initWithThresholdsLow:high:")]
		NativeHandle Constructor (nint low, nint high);

		// @property (readonly, nonatomic) NSInteger low;
		[Export ("low")]
		nint Low { get; }

		// @property (readonly, nonatomic) NSInteger high;
		[Export ("high")]
		nint High { get; }
	}

	// typedef BOOL (^LKRTCVideoEncoderCallback)(LKRTCEncodedImage * _Nonnull, id<LKRTCCodecSpecificInfo> _Nonnull);
	delegate bool LKRTCVideoEncoderCallback (LKRTCEncodedImage arg0, LKRTCCodecSpecificInfo arg1);

	// @protocol LKRTCVideoEncoder <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoder
	{
		// @required -(void)setCallback:(LKRTCVideoEncoderCallback _Nullable)callback;
		[Abstract]
		[Export ("setCallback:")]
		void SetCallback ([NullAllowed] LKRTCVideoEncoderCallback callback);

		// @required -(NSInteger)startEncodeWithSettings:(LKRTCVideoEncoderSettings * _Nonnull)settings numberOfCores:(int)numberOfCores;
		[Abstract]
		[Export ("startEncodeWithSettings:numberOfCores:")]
		nint StartEncodeWithSettings (LKRTCVideoEncoderSettings settings, int numberOfCores);

		// @required -(NSInteger)releaseEncoder;
		[Abstract]
		[Export ("releaseEncoder")]
		////[Verify (MethodToProperty)]
		nint ReleaseEncoder { get; }

		// @required -(NSInteger)encode:(LKRTCVideoFrame * _Nonnull)frame codecSpecificInfo:(id<LKRTCCodecSpecificInfo> _Nullable)info frameTypes:(NSArray<NSNumber *> * _Nonnull)frameTypes;
		[Abstract]
		[Export ("encode:codecSpecificInfo:frameTypes:")]
		nint Encode (LKRTCVideoFrame frame, [NullAllowed] LKRTCCodecSpecificInfo info, NSNumber[] frameTypes);

		// @required -(int)setBitrate:(uint32_t)bitrateKbit framerate:(uint32_t)framerate;
		[Abstract]
		[Export ("setBitrate:framerate:")]
		int SetBitrate (uint bitrateKbit, uint framerate);

		// @required -(NSString * _Nonnull)implementationName;
		[Abstract]
		[Export ("implementationName")]
		////[Verify (MethodToProperty)]
		string ImplementationName { get; }

		// @required -(LKRTCVideoEncoderQpThresholds * _Nullable)scalingSettings;
		[Abstract]
		[NullAllowed, Export ("scalingSettings")]
		////[Verify (MethodToProperty)]
		LKRTCVideoEncoderQpThresholds ScalingSettings { get; }

		// @required @property (readonly, nonatomic) NSInteger resolutionAlignment;
		[Abstract]
		[Export ("resolutionAlignment")]
		nint ResolutionAlignment { get; }

		// @required @property (readonly, nonatomic) BOOL applyAlignmentToAllSimulcastLayers;
		[Abstract]
		[Export ("applyAlignmentToAllSimulcastLayers")]
		bool ApplyAlignmentToAllSimulcastLayers { get; }

		// @required @property (readonly, nonatomic) BOOL supportsNativeHandle;
		[Abstract]
		[Export ("supportsNativeHandle")]
		bool SupportsNativeHandle { get; }
	}

	// @protocol LKRTCVideoEncoderSelector <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderSelector
	{
		// @required -(void)registerCurrentEncoderInfo:(LKRTCVideoCodecInfo * _Nonnull)info;
		[Abstract]
		[Export ("registerCurrentEncoderInfo:")]
		void RegisterCurrentEncoderInfo (LKRTCVideoCodecInfo info);

		// @required -(LKRTCVideoCodecInfo * _Nullable)encoderForBitrate:(NSInteger)bitrate;
		[Abstract]
		[Export ("encoderForBitrate:")]
		[return: NullAllowed]
		LKRTCVideoCodecInfo EncoderForBitrate (nint bitrate);

		// @required -(LKRTCVideoCodecInfo * _Nullable)encoderForBrokenEncoder;
		[Abstract]
		[NullAllowed, Export ("encoderForBrokenEncoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoCodecInfo EncoderForBrokenEncoder { get; }

		// @optional -(LKRTCVideoCodecInfo * _Nullable)encoderForResolutionChangeBySize:(CGSize)size;
		[Export ("encoderForResolutionChangeBySize:")]
		[return: NullAllowed]
		LKRTCVideoCodecInfo EncoderForResolutionChangeBySize (CGSize size);
	}

	// @protocol LKRTCVideoEncoderFactory <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderFactory
	{
		// @required -(id<LKRTCVideoEncoder> _Nullable)createEncoder:(LKRTCVideoCodecInfo * _Nonnull)info;
		[Abstract]
		[Export ("createEncoder:")]
		[return: NullAllowed]
		LKRTCVideoEncoder CreateEncoder (LKRTCVideoCodecInfo info);

		// @required -(NSArray<LKRTCVideoCodecInfo *> * _Nonnull)supportedCodecs;
		[Abstract]
		[Export ("supportedCodecs")]
		////[Verify (MethodToProperty)]
		LKRTCVideoCodecInfo[] SupportedCodecs { get; }

		// @optional -(NSArray<LKRTCVideoCodecInfo *> * _Nonnull)implementations;
		[Export ("implementations")]
		////[Verify (MethodToProperty)]
		LKRTCVideoCodecInfo[] Implementations { get; }

		// @optional -(id<LKRTCVideoEncoderSelector> _Nullable)encoderSelector;
		[NullAllowed, Export ("encoderSelector")]
		////[Verify (MethodToProperty)]
		LKRTCVideoEncoderSelector EncoderSelector { get; }
	}

	// @protocol LKRTCVideoRenderer <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoRenderer
	{
		// @required -(void)setSize:(CGSize)size;
		[Abstract]
		[Export ("setSize:")]
		void SetSize (CGSize size);

		// @required -(void)renderFrame:(LKRTCVideoFrame * _Nullable)frame;
		[Abstract]
		[Export ("renderFrame:")]
		void RenderFrame ([NullAllowed] LKRTCVideoFrame frame);
	}

	// @protocol LKRTCVideoViewDelegate
	[Protocol, Model /****(AutoGeneratedName = true)****/]
[BaseType(typeof(NSObject))]
    interface LKRTCVideoViewDelegate
	{
		// @required -(void)videoView:(id<LKRTCVideoRenderer> _Nonnull)videoView didChangeVideoSize:(CGSize)size;
		[Abstract]
		[Export ("videoView:didChangeVideoSize:")]
		void DidChangeVideoSize (ILKRTCVideoRenderer videoView, CGSize size);
	}

	// typedef OSStatus (^LKRTCAudioDeviceGetPlayoutDataBlock)(AudioUnitRenderActionFlags * _Nonnull, const AudioTimeStamp * _Nonnull, NSInteger, UInt32, AudioBufferList * _Nonnull);
	/****unsafe****/ delegate int LKRTCAudioDeviceGetPlayoutDataBlock (/****AudioUnitRenderActionFlags* ****/IntPtr arg0, /****AudioTimeStamp* ****/IntPtr arg1, nint arg2, uint arg3, /****AudioBufferList* ****/IntPtr arg4);

	// typedef OSStatus (^LKRTCAudioDeviceRenderRecordedDataBlock)(AudioUnitRenderActionFlags * _Nonnull, const AudioTimeStamp * _Nonnull, NSInteger, UInt32, AudioBufferList * _Nonnull, void * _Nullable);
	/****unsafe****/ delegate int LKRTCAudioDeviceRenderRecordedDataBlock (/****AudioUnitRenderActionFlags* ****/IntPtr arg0, /****AudioTimeStamp* ****/IntPtr arg1, nint arg2, uint arg3, /****AudioBufferList* ****/IntPtr arg4, [NullAllowed] /****void* ****/IntPtr arg5);

	// typedef OSStatus (^LKRTCAudioDeviceDeliverRecordedDataBlock)(AudioUnitRenderActionFlags * _Nonnull, const AudioTimeStamp * _Nonnull, NSInteger, UInt32, const AudioBufferList * _Nullable, void * _Nullable, LKRTCAudioDeviceRenderRecordedDataBlock _Nullable);
	/****unsafe****/ delegate int LKRTCAudioDeviceDeliverRecordedDataBlock (/****AudioUnitRenderActionFlags* ****/IntPtr arg0, /****AudioTimeStamp* ***/IntPtr arg1, nint arg2, uint arg3, [NullAllowed] /****AudioBufferList* ****/IntPtr arg4, [NullAllowed] /****void* ****/IntPtr arg5, [NullAllowed] LKRTCAudioDeviceRenderRecordedDataBlock arg6);

	// @protocol LKRTCAudioDeviceDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCAudioDeviceDelegate
	{
		// @required @property (readonly) LKRTCAudioDeviceDeliverRecordedDataBlock _Nonnull deliverRecordedData;
		[Abstract]
		[Export ("deliverRecordedData")]
		LKRTCAudioDeviceDeliverRecordedDataBlock DeliverRecordedData { get; }

		// @required @property (readonly) double preferredInputSampleRate;
		[Abstract]
		[Export ("preferredInputSampleRate")]
		double PreferredInputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval preferredInputIOBufferDuration;
		[Abstract]
		[Export ("preferredInputIOBufferDuration")]
		double PreferredInputIOBufferDuration { get; }

		// @required @property (readonly) double preferredOutputSampleRate;
		[Abstract]
		[Export ("preferredOutputSampleRate")]
		double PreferredOutputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval preferredOutputIOBufferDuration;
		[Abstract]
		[Export ("preferredOutputIOBufferDuration")]
		double PreferredOutputIOBufferDuration { get; }

		// @required @property (readonly) LKRTCAudioDeviceGetPlayoutDataBlock _Nonnull getPlayoutData;
		[Abstract]
		[Export ("getPlayoutData")]
		LKRTCAudioDeviceGetPlayoutDataBlock GetPlayoutData { get; }

		// @required -(void)notifyAudioInputParametersChange;
		[Abstract]
		[Export ("notifyAudioInputParametersChange")]
		void NotifyAudioInputParametersChange ();

		// @required -(void)notifyAudioOutputParametersChange;
		[Abstract]
		[Export ("notifyAudioOutputParametersChange")]
		void NotifyAudioOutputParametersChange ();

		// @required -(void)notifyAudioInputInterrupted;
		[Abstract]
		[Export ("notifyAudioInputInterrupted")]
		void NotifyAudioInputInterrupted ();

		// @required -(void)notifyAudioOutputInterrupted;
		[Abstract]
		[Export ("notifyAudioOutputInterrupted")]
		void NotifyAudioOutputInterrupted ();

		// @required -(void)dispatchAsync:(dispatch_block_t _Nonnull)block;
		[Abstract]
		[Export ("dispatchAsync:")]
		void DispatchAsync (dispatch_block_t block);

		// @required -(void)dispatchSync:(dispatch_block_t _Nonnull)block;
		[Abstract]
		[Export ("dispatchSync:")]
		void DispatchSync (dispatch_block_t block);
	}

	// @protocol LKRTCAudioDevice <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCAudioDevice
	{
		// @required @property (readonly) double deviceInputSampleRate;
		[Abstract]
		[Export ("deviceInputSampleRate")]
		double DeviceInputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval inputIOBufferDuration;
		[Abstract]
		[Export ("inputIOBufferDuration")]
		double InputIOBufferDuration { get; }

		// @required @property (readonly) NSInteger inputNumberOfChannels;
		[Abstract]
		[Export ("inputNumberOfChannels")]
		nint InputNumberOfChannels { get; }

		// @required @property (readonly) NSTimeInterval inputLatency;
		[Abstract]
		[Export ("inputLatency")]
		double InputLatency { get; }

		// @required @property (readonly) double deviceOutputSampleRate;
		[Abstract]
		[Export ("deviceOutputSampleRate")]
		double DeviceOutputSampleRate { get; }

		// @required @property (readonly) NSTimeInterval outputIOBufferDuration;
		[Abstract]
		[Export ("outputIOBufferDuration")]
		double OutputIOBufferDuration { get; }

		// @required @property (readonly) NSInteger outputNumberOfChannels;
		[Abstract]
		[Export ("outputNumberOfChannels")]
		nint OutputNumberOfChannels { get; }

		// @required @property (readonly) NSTimeInterval outputLatency;
		[Abstract]
		[Export ("outputLatency")]
		double OutputLatency { get; }

		// @required @property (readonly) BOOL isInitialized;
		[Abstract]
		[Export ("isInitialized")]
		bool IsInitialized { get; }

		// @required -(BOOL)initializeWithDelegate:(id<LKRTCAudioDeviceDelegate> _Nonnull)delegate;
		[Abstract]
		[Export ("initializeWithDelegate:")]
		bool InitializeWithDelegate (LKRTCAudioDeviceDelegate @delegate);

		// @required -(BOOL)terminateDevice;
		[Abstract]
		[Export ("terminateDevice")]
		////[Verify (MethodToProperty)]
		bool TerminateDevice { get; }

		// @required @property (readonly) BOOL isPlayoutInitialized;
		[Abstract]
		[Export ("isPlayoutInitialized")]
		bool IsPlayoutInitialized { get; }

		// @required -(BOOL)initializePlayout;
		[Abstract]
		[Export ("initializePlayout")]
		////[Verify (MethodToProperty)]
		bool InitializePlayout { get; }

		// @required @property (readonly) BOOL isPlaying;
		[Abstract]
		[Export ("isPlaying")]
		bool IsPlaying { get; }

		// @required -(BOOL)startPlayout;
		[Abstract]
		[Export ("startPlayout")]
		////[Verify (MethodToProperty)]
		bool StartPlayout { get; }

		// @required -(BOOL)stopPlayout;
		[Abstract]
		[Export ("stopPlayout")]
		////[Verify (MethodToProperty)]
		bool StopPlayout { get; }

		// @required @property (readonly) BOOL isRecordingInitialized;
		[Abstract]
		[Export ("isRecordingInitialized")]
		bool IsRecordingInitialized { get; }

		// @required -(BOOL)initializeRecording;
		[Abstract]
		[Export ("initializeRecording")]
		////[Verify (MethodToProperty)]
		bool InitializeRecording { get; }

		// @required @property (readonly) BOOL isRecording;
		[Abstract]
		[Export ("isRecording")]
		bool IsRecording { get; }

		// @required -(BOOL)startRecording;
		[Abstract]
		[Export ("startRecording")]
		////[Verify (MethodToProperty)]
		bool StartRecording { get; }

		// @required -(BOOL)stopRecording;
		[Abstract]
		[Export ("stopRecording")]
		////[Verify (MethodToProperty)]
		bool StopRecording { get; }
	}

	[Static]
	////[Verify (ConstantsInterfaceAssociation)]
	partial interface Constants
	{
		/*
		// extern NSString *const _Nonnull kLKRTCAudioSessionErrorDomain;
		[Field ("kLKRTCAudioSessionErrorDomain", "__Internal")]
		NSString kLKRTCAudioSessionErrorDomain { get; }
		*/
		
		/*
		// extern const NSInteger kLKRTCAudioSessionErrorLockRequired;
		[Field ("kLKRTCAudioSessionErrorLockRequired", "__Internal")]
		nint kLKRTCAudioSessionErrorLockRequired { get; }
		*/
		
		/*
		// extern const NSInteger kLKRTCAudioSessionErrorConfiguration;
		[Field ("kLKRTCAudioSessionErrorConfiguration", "__Internal")]
		nint kLKRTCAudioSessionErrorConfiguration { get; }
		*/
	}

	// @protocol LKRTCAudioSessionDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCAudioSessionDelegate
	{
		// @optional -(void)audioSessionDidBeginInterruption:(LKRTCAudioSession * _Nonnull)session;
		[Export ("audioSessionDidBeginInterruption:")]
		void AudioSessionDidBeginInterruption (LKRTCAudioSession session);

		// @optional -(void)audioSessionDidEndInterruption:(LKRTCAudioSession * _Nonnull)session shouldResumeSession:(BOOL)shouldResumeSession;
		[Export ("audioSessionDidEndInterruption:shouldResumeSession:")]
		void AudioSessionDidEndInterruption (LKRTCAudioSession session, bool shouldResumeSession);

		// @optional -(void)audioSessionDidChangeRoute:(LKRTCAudioSession * _Nonnull)session reason:(AVAudioSessionRouteChangeReason)reason previousRoute:(AVAudioSessionRouteDescription * _Nonnull)previousRoute;
		[Export ("audioSessionDidChangeRoute:reason:previousRoute:")]
		void AudioSessionDidChangeRoute (LKRTCAudioSession session, AVAudioSessionRouteChangeReason reason, AVAudioSessionRouteDescription previousRoute);

		// @optional -(void)audioSessionMediaServerTerminated:(LKRTCAudioSession * _Nonnull)session;
		[Export ("audioSessionMediaServerTerminated:")]
		void AudioSessionMediaServerTerminated (LKRTCAudioSession session);

		// @optional -(void)audioSessionMediaServerReset:(LKRTCAudioSession * _Nonnull)session;
		[Export ("audioSessionMediaServerReset:")]
		void AudioSessionMediaServerReset (LKRTCAudioSession session);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)session didChangeCanPlayOrRecord:(BOOL)canPlayOrRecord;
		[Export ("audioSession:didChangeCanPlayOrRecord:")]
		void AudioSessionDidChangeCanPlayOrRecord(LKRTCAudioSession session, bool canPlayOrRecord);

		// @optional -(void)audioSessionDidStartPlayOrRecord:(LKRTCAudioSession * _Nonnull)session;
		[Export ("audioSessionDidStartPlayOrRecord:")]
		void AudioSessionDidStartPlayOrRecord (LKRTCAudioSession session);

		// @optional -(void)audioSessionDidStopPlayOrRecord:(LKRTCAudioSession * _Nonnull)session;
		[Export ("audioSessionDidStopPlayOrRecord:")]
		void AudioSessionDidStopPlayOrRecord (LKRTCAudioSession session);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)audioSession didChangeOutputVolume:(float)outputVolume;
		[Export ("audioSession:didChangeOutputVolume:")]
		void AudioSessionDidChangeOutputVolume(LKRTCAudioSession audioSession, float outputVolume);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)audioSession didDetectPlayoutGlitch:(int64_t)totalNumberOfGlitches;
		[Export ("audioSession:didDetectPlayoutGlitch:")]
		void AudioSessionDidDetectPlayoutGlitch(LKRTCAudioSession audioSession, long totalNumberOfGlitches);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)audioSession willSetActive:(BOOL)active;
		[Export ("audioSession:willSetActive:")]
		void AudioSessionWillSetActive(LKRTCAudioSession audioSession, bool active);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)audioSession didSetActive:(BOOL)active;
		[Export ("audioSession:didSetActive:")]
		void AudioSessionDidSetActive(LKRTCAudioSession audioSession, bool active);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)audioSession failedToSetActive:(BOOL)active error:(NSError * _Nonnull)error;
		[Export ("audioSession:failedToSetActive:error:")]
		void AudioSessionFailedToSetActive(LKRTCAudioSession audioSession, bool active, NSError error);

		// @optional -(void)audioSession:(LKRTCAudioSession * _Nonnull)audioSession audioUnitStartFailedWithError:(NSError * _Nonnull)error;
		[Export ("audioSession:audioUnitStartFailedWithError:")]
		void AudioSessionAudioUnitStartFailedWithError(LKRTCAudioSession audioSession, NSError error);
	}

	// @protocol LKRTCAudioSessionActivationDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCAudioSessionActivationDelegate
	{
		// @required -(void)audioSessionDidActivate:(AVAudioSession * _Nonnull)session;
		[Abstract]
		[Export ("audioSessionDidActivate:")]
		void AudioSessionDidActivate (AVAudioSession session);

		// @required -(void)audioSessionDidDeactivate:(AVAudioSession * _Nonnull)session;
		[Abstract]
		[Export ("audioSessionDidDeactivate:")]
		void AudioSessionDidDeactivate (AVAudioSession session);
	}

	// @interface LKRTCAudioSession : NSObject <LKRTCAudioSessionActivationDelegate>
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCAudioSession : ILKRTCAudioSessionActivationDelegate
	{
		// @property (readonly, nonatomic) AVAudioSession * _Nonnull session;
		[Export ("session")]
		AVAudioSession Session { get; }

		// @property (readonly, nonatomic) BOOL isActive;
		[Export ("isActive")]
		bool IsActive { get; }

		// @property (assign, nonatomic) BOOL useManualAudio;
		[Export ("useManualAudio")]
		bool UseManualAudio { get; set; }

		// @property (assign, nonatomic) BOOL isAudioEnabled;
		[Export ("isAudioEnabled")]
		bool IsAudioEnabled { get; set; }

		// @property (readonly) NSString * _Nonnull category;
		[Export ("category")]
		string Category { get; }

		// @property (readonly) AVAudioSessionCategoryOptions categoryOptions;
		[Export ("categoryOptions")]
		AVAudioSessionCategoryOptions CategoryOptions { get; }

		// @property (readonly) NSString * _Nonnull mode;
		[Export ("mode")]
		string Mode { get; }

		// @property (readonly) BOOL secondaryAudioShouldBeSilencedHint;
		[Export ("secondaryAudioShouldBeSilencedHint")]
		bool SecondaryAudioShouldBeSilencedHint { get; }

		// @property (readonly) AVAudioSessionRouteDescription * _Nonnull currentRoute;
		[Export ("currentRoute")]
		AVAudioSessionRouteDescription CurrentRoute { get; }

		// @property (readonly) NSInteger maximumInputNumberOfChannels;
		[Export ("maximumInputNumberOfChannels")]
		nint MaximumInputNumberOfChannels { get; }

		// @property (readonly) NSInteger maximumOutputNumberOfChannels;
		[Export ("maximumOutputNumberOfChannels")]
		nint MaximumOutputNumberOfChannels { get; }

		// @property (readonly) float inputGain;
		[Export ("inputGain")]
		float InputGain { get; }

		// @property (readonly) BOOL inputGainSettable;
		[Export ("inputGainSettable")]
		bool InputGainSettable { get; }

		// @property (readonly) BOOL inputAvailable;
		[Export ("inputAvailable")]
		bool InputAvailable { get; }

		// @property (readonly) NSArray<AVAudioSessionDataSourceDescription *> * _Nullable inputDataSources;
		[NullAllowed, Export ("inputDataSources")]
		AVAudioSessionDataSourceDescription[] InputDataSources { get; }

		[Wrap ("WeakInputDataSource")]
		[NullAllowed]
		AVAudioSessionDataSourceDescription InputDataSource { get; }

		// @property (readonly) AVAudioSessionDataSourceDescription * _Nullable inputDataSource;
		[NullAllowed, Export ("inputDataSource")]
		NSObject WeakInputDataSource { get; }

		// @property (readonly) NSArray<AVAudioSessionDataSourceDescription *> * _Nullable outputDataSources;
		[NullAllowed, Export ("outputDataSources")]
		AVAudioSessionDataSourceDescription[] OutputDataSources { get; }

		[Wrap ("WeakOutputDataSource")]
		[NullAllowed]
		AVAudioSessionDataSourceDescription OutputDataSource { get; }

		// @property (readonly) AVAudioSessionDataSourceDescription * _Nullable outputDataSource;
		[NullAllowed, Export ("outputDataSource")]
		NSObject WeakOutputDataSource { get; }

		// @property (readonly) double sampleRate;
		[Export ("sampleRate")]
		double SampleRate { get; }

		// @property (readonly) double preferredSampleRate;
		[Export ("preferredSampleRate")]
		double PreferredSampleRate { get; }

		// @property (readonly) NSInteger inputNumberOfChannels;
		[Export ("inputNumberOfChannels")]
		nint InputNumberOfChannels { get; }

		// @property (readonly) NSInteger outputNumberOfChannels;
		[Export ("outputNumberOfChannels")]
		nint OutputNumberOfChannels { get; }

		// @property (readonly) float outputVolume;
		[Export ("outputVolume")]
		float OutputVolume { get; }

		// @property (readonly) NSTimeInterval inputLatency;
		[Export ("inputLatency")]
		double InputLatency { get; }

		// @property (readonly) NSTimeInterval outputLatency;
		[Export ("outputLatency")]
		double OutputLatency { get; }

		// @property (readonly) NSTimeInterval IOBufferDuration;
		[Export ("IOBufferDuration")]
		double IOBufferDuration { get; }

		// @property (readonly) NSTimeInterval preferredIOBufferDuration;
		[Export ("preferredIOBufferDuration")]
		double PreferredIOBufferDuration { get; }

		// @property (nonatomic) BOOL ignoresPreferredAttributeConfigurationErrors;
		[Export ("ignoresPreferredAttributeConfigurationErrors")]
		bool IgnoresPreferredAttributeConfigurationErrors { get; set; }

		// +(instancetype _Nonnull)sharedInstance;
		[Static]
		[Export ("sharedInstance")]
		LKRTCAudioSession SharedInstance ();

		// -(void)addDelegate:(id<LKRTCAudioSessionDelegate> _Nonnull)delegate;
		[Export ("addDelegate:")]
		void AddDelegate (LKRTCAudioSessionDelegate @delegate);

		// -(void)removeDelegate:(id<LKRTCAudioSessionDelegate> _Nonnull)delegate;
		[Export ("removeDelegate:")]
		void RemoveDelegate (LKRTCAudioSessionDelegate @delegate);

		// -(void)lockForConfiguration;
		[Export ("lockForConfiguration")]
		void LockForConfiguration ();

		// -(void)unlockForConfiguration;
		[Export ("unlockForConfiguration")]
		void UnlockForConfiguration ();

		// -(BOOL)setActive:(BOOL)active error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setActive:error:")]
		bool SetActive (bool active, [NullAllowed] out NSError outError);

		// -(BOOL)setCategory:(AVAudioSessionCategory _Nonnull)category mode:(AVAudioSessionMode _Nonnull)mode options:(AVAudioSessionCategoryOptions)options error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setCategory:mode:options:error:")]
		bool SetCategory (string category, string mode, AVAudioSessionCategoryOptions options, [NullAllowed] out NSError outError);

		// -(BOOL)setCategory:(AVAudioSessionCategory _Nonnull)category withOptions:(AVAudioSessionCategoryOptions)options error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setCategory:withOptions:error:")]
		bool SetCategory (string category, AVAudioSessionCategoryOptions options, [NullAllowed] out NSError outError);

		// -(BOOL)setMode:(AVAudioSessionMode _Nonnull)mode error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setMode:error:")]
		bool SetMode (string mode, [NullAllowed] out NSError outError);

		// -(BOOL)setInputGain:(float)gain error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setInputGain:error:")]
		bool SetInputGain (float gain, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredSampleRate:(double)sampleRate error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredSampleRate:error:")]
		bool SetPreferredSampleRate (double sampleRate, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredIOBufferDuration:(NSTimeInterval)duration error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredIOBufferDuration:error:")]
		bool SetPreferredIOBufferDuration (double duration, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredInputNumberOfChannels:(NSInteger)count error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredInputNumberOfChannels:error:")]
		bool SetPreferredInputNumberOfChannels (nint count, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredOutputNumberOfChannels:(NSInteger)count error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredOutputNumberOfChannels:error:")]
		bool SetPreferredOutputNumberOfChannels (nint count, [NullAllowed] out NSError outError);

		// -(BOOL)overrideOutputAudioPort:(AVAudioSessionPortOverride)portOverride error:(NSError * _Nullable * _Nullable)outError;
		[Export ("overrideOutputAudioPort:error:")]
		bool OverrideOutputAudioPort (AVAudioSessionPortOverride portOverride, [NullAllowed] out NSError outError);

		// -(BOOL)setPreferredInput:(AVAudioSessionPortDescription * _Nonnull)inPort error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setPreferredInput:error:")]
		bool SetPreferredInput (AVAudioSessionPortDescription inPort, [NullAllowed] out NSError outError);

		// -(BOOL)setInputDataSource:(AVAudioSessionDataSourceDescription * _Nonnull)dataSource error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setInputDataSource:error:")]
		bool SetInputDataSource (AVAudioSessionDataSourceDescription dataSource, [NullAllowed] out NSError outError);

		// -(BOOL)setOutputDataSource:(AVAudioSessionDataSourceDescription * _Nonnull)dataSource error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setOutputDataSource:error:")]
		bool SetOutputDataSource (AVAudioSessionDataSourceDescription dataSource, [NullAllowed] out NSError outError);
	}

	// @interface Configuration (LKRTCAudioSession)
	[Category]
	[BaseType (typeof(LKRTCAudioSession))]
	interface LKRTCAudioSession_Configuration
	{
		// -(BOOL)setConfiguration:(LKRTCAudioSessionConfiguration * _Nonnull)configuration error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setConfiguration:error:")]
		bool SetConfiguration (LKRTCAudioSessionConfiguration configuration, [NullAllowed] out NSError outError);

		// -(BOOL)setConfiguration:(LKRTCAudioSessionConfiguration * _Nonnull)configuration active:(BOOL)active error:(NSError * _Nullable * _Nullable)outError;
		[Export ("setConfiguration:active:error:")]
		bool SetConfiguration (LKRTCAudioSessionConfiguration configuration, bool active, [NullAllowed] out NSError outError);
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern const int kLKRTCAudioSessionPreferredNumberOfChannels;
		[Field ("kLKRTCAudioSessionPreferredNumberOfChannels", "__Internal")]
		int kLKRTCAudioSessionPreferredNumberOfChannels { get; }

		// extern const double kLKRTCAudioSessionHighPerformanceSampleRate;
		[Field ("kLKRTCAudioSessionHighPerformanceSampleRate", "__Internal")]
		double kLKRTCAudioSessionHighPerformanceSampleRate { get; }

		// extern const double kLKRTCAudioSessionHighPerformanceIOBufferDuration;
		[Field ("kLKRTCAudioSessionHighPerformanceIOBufferDuration", "__Internal")]
		double kLKRTCAudioSessionHighPerformanceIOBufferDuration { get; }
	}

	// @interface LKRTCAudioSessionConfiguration : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCAudioSessionConfiguration
	{
		// @property (nonatomic, strong) NSString * _Nonnull category;
		[Export ("category", ArgumentSemantic.Strong)]
		string Category { get; set; }

		// @property (assign, nonatomic) AVAudioSessionCategoryOptions categoryOptions;
		[Export ("categoryOptions", ArgumentSemantic.Assign)]
		AVAudioSessionCategoryOptions CategoryOptions { get; set; }

		// @property (nonatomic, strong) NSString * _Nonnull mode;
		[Export ("mode", ArgumentSemantic.Strong)]
		string Mode { get; set; }

		// @property (assign, nonatomic) double sampleRate;
		[Export ("sampleRate")]
		double SampleRate { get; set; }

		// @property (assign, nonatomic) NSTimeInterval ioBufferDuration;
		[Export ("ioBufferDuration")]
		double IoBufferDuration { get; set; }

		// @property (assign, nonatomic) NSInteger inputNumberOfChannels;
		[Export ("inputNumberOfChannels")]
		nint InputNumberOfChannels { get; set; }

		// @property (assign, nonatomic) NSInteger outputNumberOfChannels;
		[Export ("outputNumberOfChannels")]
		nint OutputNumberOfChannels { get; set; }

		// +(instancetype _Nonnull)currentConfiguration;
		[Static]
		[Export ("currentConfiguration")]
		LKRTCAudioSessionConfiguration CurrentConfiguration ();

		// +(instancetype _Nonnull)webRTCConfiguration;
		[Static]
		[Export ("webRTCConfiguration")]
		LKRTCAudioSessionConfiguration WebRTCConfiguration ();

		// +(void)setWebRTCConfiguration:(LKRTCAudioSessionConfiguration * _Nonnull)configuration;
		[Static]
		[Export ("setWebRTCConfiguration:")]
		void SetWebRTCConfiguration (LKRTCAudioSessionConfiguration configuration);
	}

	// @interface LKRTCCameraVideoCapturer : LKRTCVideoCapturer
	////[Unavailable (PlatformName.iOSAppExtension)]
	[BaseType (typeof(LKRTCVideoCapturer))]
	interface LKRTCCameraVideoCapturer
	{
		// @property (readonly, nonatomic) AVCaptureSession * _Nonnull captureSession;
		[Export ("captureSession")]
		AVCaptureSession CaptureSession { get; }

		// +(NSArray<AVCaptureDevice *> * _Nonnull)captureDevices;
		[Static]
		[Export ("captureDevices")]
		////[Verify (MethodToProperty)]
		AVCaptureDevice[] CaptureDevices { get; }

		// +(NSArray<AVCaptureDeviceFormat *> * _Nonnull)supportedFormatsForDevice:(AVCaptureDevice * _Nonnull)device;
		[Static]
		[Export ("supportedFormatsForDevice:")]
		AVCaptureDeviceFormat[] SupportedFormatsForDevice (AVCaptureDevice device);

		// -(FourCharCode)preferredOutputPixelFormat;
		[Export ("preferredOutputPixelFormat")]
		////[Verify (MethodToProperty)]
		uint PreferredOutputPixelFormat { get; }

		// -(void)startCaptureWithDevice:(AVCaptureDevice * _Nonnull)device format:(AVCaptureDeviceFormat * _Nonnull)format fps:(NSInteger)fps completionHandler:(void (^ _Nullable)(NSError * _Nullable))completionHandler;
		[Export ("startCaptureWithDevice:format:fps:completionHandler:")]
		void StartCaptureWithDevice (AVCaptureDevice device, AVCaptureDeviceFormat format, nint fps, [NullAllowed] Action<NSError> completionHandler);

		// -(void)stopCaptureWithCompletionHandler:(void (^ _Nullable)(void))completionHandler;
		[Export ("stopCaptureWithCompletionHandler:")]
		void StopCaptureWithCompletionHandler ([NullAllowed] Action completionHandler);

		// -(void)startCaptureWithDevice:(AVCaptureDevice * _Nonnull)device format:(AVCaptureDeviceFormat * _Nonnull)format fps:(NSInteger)fps;
		[Export ("startCaptureWithDevice:format:fps:")]
		void StartCaptureWithDevice (AVCaptureDevice device, AVCaptureDeviceFormat format, nint fps);

		// -(void)stopCapture;
		[Export ("stopCapture")]
		void StopCapture ();
	}

	// typedef void (^LKRTCFileVideoCapturerErrorBlock)(NSError * _Nonnull);
	delegate void LKRTCFileVideoCapturerErrorBlock (NSError arg0);

	// @interface LKRTCFileVideoCapturer : LKRTCVideoCapturer
	////[iOS (10,0)]
	[BaseType (typeof(LKRTCVideoCapturer))]
	interface LKRTCFileVideoCapturer
	{
		// -(void)startCapturingFromFileNamed:(NSString * _Nonnull)nameOfFile onError:(LKRTCFileVideoCapturerErrorBlock _Nullable)errorBlock;
		[Export ("startCapturingFromFileNamed:onError:")]
		void StartCapturingFromFileNamed (string nameOfFile, [NullAllowed] LKRTCFileVideoCapturerErrorBlock errorBlock);

		// -(void)stopCapture;
		[Export ("stopCapture")]
		void StopCapture ();
	}

	// @interface LKRTCNetworkMonitor : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
[Protocol]
	interface LKRTCNetworkMonitor
	{
	}

	// @interface LKRTCMTLVideoView : UIView <LKRTCVideoRenderer>
	////[iOS (9,0)]
	[BaseType (typeof(UIView))]
	interface LKRTCMTLVideoView : /****I****/LKRTCVideoRenderer
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ILKRTCVideoViewDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<LKRTCVideoViewDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// @property (nonatomic) UIViewContentMode videoContentMode;
		[Export ("videoContentMode", ArgumentSemantic.Assign)]
		UIViewContentMode VideoContentMode { get; set; }

		// @property (getter = isEnabled, nonatomic) BOOL enabled;
		[Export ("enabled")]
		bool Enabled { [Bind ("isEnabled")] get; set; }

		// @property (nonatomic) NSValue * _Nullable rotationOverride;
		[NullAllowed, Export ("rotationOverride", ArgumentSemantic.Assign)]
		NSValue RotationOverride { get; set; }
	}

	// @protocol LKRTCVideoViewShading <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoViewShading
	{
		// @required -(void)applyShadingForFrameWithWidth:(int)width height:(int)height rotation:(LKRTCVideoRotation)rotation yPlane:(GLuint)yPlane uPlane:(GLuint)uPlane vPlane:(GLuint)vPlane;
		[Abstract]
		[Export ("applyShadingForFrameWithWidth:height:rotation:yPlane:uPlane:vPlane:")]
		void Height (int width, int height, LKRTCVideoRotation rotation, uint yPlane, uint uPlane, uint vPlane);

		// @required -(void)applyShadingForFrameWithWidth:(int)width height:(int)height rotation:(LKRTCVideoRotation)rotation yPlane:(GLuint)yPlane uvPlane:(GLuint)uvPlane;
		[Abstract]
		[Export ("applyShadingForFrameWithWidth:height:rotation:yPlane:uvPlane:")]
		void Height (int width, int height, LKRTCVideoRotation rotation, uint yPlane, uint uvPlane);
	}

    // @interface LKRTCEAGLVideoView : UIView <LKRTCVideoRenderer>
    ////[Unavailable (PlatformName.iOSAppExtension)]
 [Protocol]
    [BaseType (typeof(UIView))]
	interface LKRTCEAGLVideoView : /****I****/LKRTCVideoRenderer
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ILKRTCVideoViewDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<LKRTCVideoViewDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// -(instancetype _Nonnull)initWithFrame:(CGRect)frame shader:(id<LKRTCVideoViewShading> _Nonnull)shader __attribute__((objc_designated_initializer));
		[Export ("initWithFrame:shader:")]
		[DesignatedInitializer]
		NativeHandle Constructor (CGRect frame, LKRTCVideoViewShading shader);

		// -(instancetype _Nonnull)initWithCoder:(NSCoder * _Nonnull)aDecoder shader:(id<LKRTCVideoViewShading> _Nonnull)shader __attribute__((objc_designated_initializer));
		[Export ("initWithCoder:shader:")]
		[DesignatedInitializer]
		NativeHandle Constructor (NSCoder aDecoder, LKRTCVideoViewShading shader);

		// @property (nonatomic) NSValue * _Nullable rotationOverride;
		[NullAllowed, Export ("rotationOverride", ArgumentSemantic.Assign)]
		NSValue RotationOverride { get; set; }
	}

	// @interface LKRTCCodecSpecificInfoH264 : NSObject <LKRTCCodecSpecificInfo>
	[BaseType (typeof(NSObject))]
	interface LKRTCCodecSpecificInfoH264 : ILKRTCCodecSpecificInfo
	{
		// @property (assign, nonatomic) LKRTCH264PacketizationMode packetizationMode;
		[Export ("packetizationMode", ArgumentSemantic.Assign)]
		LKRTCH264PacketizationMode PacketizationMode { get; set; }
	}

	// @interface LKRTCDefaultVideoDecoderFactory : NSObject <LKRTCVideoDecoderFactory>
	////[BaseType (typeof(NSObject))]
	[BaseType (typeof(LKRTCVideoDecoderFactory))]
    interface LKRTCDefaultVideoDecoderFactory ////: ILKRTCVideoDecoderFactory
	{
	}

    // @interface LKRTCDefaultVideoEncoderFactory : NSObject <LKRTCVideoEncoderFactory>
    ////[BaseType (typeof(NSObject))]
    [BaseType (typeof(LKRTCVideoEncoderFactory))]
    interface LKRTCDefaultVideoEncoderFactory ////: ILKRTCVideoEncoderFactory
	{
		// @property (retain, nonatomic) LKRTCVideoCodecInfo * _Nonnull preferredCodec;
		[Export ("preferredCodec", ArgumentSemantic.Retain)]
		LKRTCVideoCodecInfo PreferredCodec { get; set; }

		// +(NSArray<LKRTCVideoCodecInfo *> * _Nonnull)supportedCodecs;
		[Static]
		[Export ("supportedCodecs")]
		////[Verify (MethodToProperty)]
		LKRTCVideoCodecInfo[] SupportedCodecs { get; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const kLKRTCVideoCodecH264Name;
		[Field ("kLKRTCVideoCodecH264Name", "__Internal")]
		NSString kLKRTCVideoCodecH264Name { get; }

		// extern NSString *const kLKRTCLevel31ConstrainedHigh;
		[Field ("kLKRTCLevel31ConstrainedHigh", "__Internal")]
		NSString kLKRTCLevel31ConstrainedHigh { get; }

		// extern NSString *const kLKRTCLevel31ConstrainedBaseline;
		[Field ("kLKRTCLevel31ConstrainedBaseline", "__Internal")]
		NSString kLKRTCLevel31ConstrainedBaseline { get; }

		// extern NSString *const kLKRTCMaxSupportedH264ProfileLevelConstrainedHigh;
		[Field ("kLKRTCMaxSupportedH264ProfileLevelConstrainedHigh", "__Internal")]
		NSString kLKRTCMaxSupportedH264ProfileLevelConstrainedHigh { get; }

		// extern NSString *const kLKRTCMaxSupportedH264ProfileLevelConstrainedBaseline;
		[Field ("kLKRTCMaxSupportedH264ProfileLevelConstrainedBaseline", "__Internal")]
		NSString kLKRTCMaxSupportedH264ProfileLevelConstrainedBaseline { get; }
	}

	// @interface LKRTCH264ProfileLevelId : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCH264ProfileLevelId
	{
		// @property (readonly, nonatomic) LKRTCH264Profile profile;
		[Export ("profile")]
		LKRTCH264Profile Profile { get; }

		// @property (readonly, nonatomic) LKRTCH264Level level;
		[Export ("level")]
		LKRTCH264Level Level { get; }

		// @property (readonly, nonatomic) NSString * hexString;
		[Export ("hexString")]
		string HexString { get; }

		// -(instancetype)initWithHexString:(NSString *)hexString;
		[Export ("initWithHexString:")]
		NativeHandle Constructor (string hexString);

		// -(instancetype)initWithProfile:(LKRTCH264Profile)profile level:(LKRTCH264Level)level;
		[Export ("initWithProfile:level:")]
		NativeHandle Constructor (LKRTCH264Profile profile, LKRTCH264Level level);
	}

	// @interface LKRTCVideoDecoderFactoryH264 : NSObject <LKRTCVideoDecoderFactory>
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoderFactoryH264 : ILKRTCVideoDecoderFactory
	{
	}

	// @interface LKRTCVideoDecoderH264 : NSObject <LKRTCVideoDecoder>
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoderH264 : ILKRTCVideoDecoder
	{
	}

	// @interface LKRTCVideoEncoderFactoryH264 : NSObject <LKRTCVideoEncoderFactory>
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderFactoryH264 : ILKRTCVideoEncoderFactory
	{
	}

	// @interface LKRTCVideoEncoderH264 : NSObject <LKRTCVideoEncoder>
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderH264 : ILKRTCVideoEncoder
	{
		// -(instancetype)initWithCodecInfo:(LKRTCVideoCodecInfo *)codecInfo;
		[Export ("initWithCodecInfo:")]
		NativeHandle Constructor (LKRTCVideoCodecInfo codecInfo);
	}

	// @interface LKRTCCVPixelBuffer : NSObject <LKRTCVideoFrameBuffer>
	[BaseType (typeof(NSObject))]
	interface LKRTCCVPixelBuffer : ILKRTCVideoFrameBuffer
	{
		// @property (readonly, nonatomic) CVPixelBufferRef _Nonnull pixelBuffer;
		[Export ("pixelBuffer")]
		CVPixelBuffer PixelBuffer { get; }

		// @property (readonly, nonatomic) int cropX;
		[Export ("cropX")]
		int CropX { get; }

		// @property (readonly, nonatomic) int cropY;
		[Export ("cropY")]
		int CropY { get; }

		// @property (readonly, nonatomic) int cropWidth;
		[Export ("cropWidth")]
		int CropWidth { get; }

		// @property (readonly, nonatomic) int cropHeight;
		[Export ("cropHeight")]
		int CropHeight { get; }

		// +(NSSet<NSNumber *> * _Nonnull)supportedPixelFormats;
		[Static]
		[Export ("supportedPixelFormats")]
		////[Verify (MethodToProperty)]
		NSSet<NSNumber> SupportedPixelFormats { get; }

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer;
		[Export ("initWithPixelBuffer:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer);

		// -(instancetype _Nonnull)initWithPixelBuffer:(CVPixelBufferRef _Nonnull)pixelBuffer adaptedWidth:(int)adaptedWidth adaptedHeight:(int)adaptedHeight cropWidth:(int)cropWidth cropHeight:(int)cropHeight cropX:(int)cropX cropY:(int)cropY;
		[Export ("initWithPixelBuffer:adaptedWidth:adaptedHeight:cropWidth:cropHeight:cropX:cropY:")]
		NativeHandle Constructor (CVPixelBuffer pixelBuffer, int adaptedWidth, int adaptedHeight, int cropWidth, int cropHeight, int cropX, int cropY);

		// -(BOOL)requiresCropping;
		[Export ("requiresCropping")]
		////[Verify (MethodToProperty)]
		bool RequiresCropping { get; }

		// -(BOOL)requiresScalingToWidth:(int)width height:(int)height;
		[Export ("requiresScalingToWidth:height:")]
		bool RequiresScalingToWidth (int width, int height);

		// -(int)bufferSizeForCroppingAndScalingToWidth:(int)width height:(int)height;
		[Export ("bufferSizeForCroppingAndScalingToWidth:height:")]
		int BufferSizeForCroppingAndScalingToWidth (int width, int height);

		// -(BOOL)cropAndScaleTo:(CVPixelBufferRef _Nonnull)outputPixelBuffer withTempBuffer:(uint8_t * _Nullable)tmpBuffer;
		[Export ("cropAndScaleTo:withTempBuffer:")]
		/****unsafe****/ bool CropAndScaleTo (CVPixelBuffer outputPixelBuffer, [NullAllowed] /****byte* ****/IntPtr tmpBuffer);
	}

	// @interface LKRTCCameraPreviewView : UIView
	[BaseType (typeof(UIView))]
	interface LKRTCCameraPreviewView
	{
		// @property (nonatomic, strong) AVCaptureSession * captureSession;
		[Export ("captureSession", ArgumentSemantic.Strong)]
		AVCaptureSession CaptureSession { get; set; }
	}

	// @interface LKRTCDispatcher : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCDispatcher
	{
		// +(void)dispatchAsyncOnType:(LKRTCDispatcherQueueType)dispatchType block:(dispatch_block_t)block;
		[Static]
		[Export ("dispatchAsyncOnType:block:")]
		void DispatchAsyncOnType (LKRTCDispatcherQueueType dispatchType, dispatch_block_t block);

		// +(BOOL)isOnQueueForType:(LKRTCDispatcherQueueType)dispatchType;
		[Static]
		[Export ("isOnQueueForType:")]
		bool IsOnQueueForType (LKRTCDispatcherQueueType dispatchType);
	}

	// @interface LKRTCDevice (UIDevice)
	[Category]
	[BaseType (typeof(UIDevice))]
	interface UIDevice_RTCDevice
	{
		// +(NSString *)machineName;
		[Static]
		[Export ("machineName")]
		////[Verify (MethodToProperty)]
		string MachineName { get; }
	}

	// @interface LKRTCMediaSource : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCMediaSource
	{
		// @property (readonly, nonatomic) LKRTCSourceState state;
		[Export ("state")]
		LKRTCSourceState State { get; }
	}

	// @interface LKRTCAudioSource : LKRTCMediaSource
	[BaseType (typeof(LKRTCMediaSource))]
	[DisableDefaultCtor]
	interface LKRTCAudioSource
	{
		// @property (assign, nonatomic) double volume;
		[Export ("volume")]
		double Volume { get; set; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const _Nonnull kLKRTCMediaStreamTrackKindAudio;
		[Field ("kLKRTCMediaStreamTrackKindAudio", "__Internal")]
		NSString kLKRTCMediaStreamTrackKindAudio { get; }

		// extern NSString *const _Nonnull kLKRTCMediaStreamTrackKindVideo;
		[Field ("kLKRTCMediaStreamTrackKindVideo", "__Internal")]
		NSString kLKRTCMediaStreamTrackKindVideo { get; }
	}

	// @interface LKRTCMediaStreamTrack : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCMediaStreamTrack
	{
		// @property (readonly, nonatomic) NSString * _Nonnull kind;
		[Export ("kind")]
		string Kind { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull trackId;
		[Export ("trackId")]
		string TrackId { get; }

		// @property (assign, nonatomic) BOOL isEnabled;
		[Export ("isEnabled")]
		bool IsEnabled { get; set; }

		// @property (readonly, nonatomic) LKRTCMediaStreamTrackState readyState;
		[Export ("readyState")]
		LKRTCMediaStreamTrackState ReadyState { get; }
	}

	// @interface LKRTCAudioTrack : LKRTCMediaStreamTrack
	[BaseType (typeof(LKRTCMediaStreamTrack))]
	[DisableDefaultCtor]
	interface LKRTCAudioTrack
	{
		// @property (readonly, nonatomic) LKRTCAudioSource * _Nonnull source;
		[Export ("source")]
		LKRTCAudioSource Source { get; }
	}

	// @interface LKRTCCertificate : NSObject <NSCopying>
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCCertificate : INSCopying
	{
		// @property (readonly, copy, nonatomic) NSString * _Nonnull private_key;
		[Export ("private_key")]
		string Private_key { get; }

		// @property (readonly, copy, nonatomic) NSString * _Nonnull certificate;
		[Export ("certificate")]
		string Certificate { get; }

		// -(instancetype _Nonnull)initWithPrivateKey:(NSString * _Nonnull)private_key certificate:(NSString * _Nonnull)certificate __attribute__((objc_designated_initializer));
		[Export ("initWithPrivateKey:certificate:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string private_key, string certificate);

		// +(LKRTCCertificate * _Nullable)generateCertificateWithParams:(NSDictionary * _Nonnull)params;
		[Static]
		[Export ("generateCertificateWithParams:")]
		[return: NullAllowed]
		LKRTCCertificate GenerateCertificateWithParams (NSDictionary @params);
	}

	// @interface LKRTCCryptoOptions : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCCryptoOptions
	{
		// @property (assign, nonatomic) BOOL srtpEnableGcmCryptoSuites;
		[Export ("srtpEnableGcmCryptoSuites")]
		bool SrtpEnableGcmCryptoSuites { get; set; }

		// @property (assign, nonatomic) BOOL srtpEnableAes128Sha1_32CryptoCipher;
		[Export ("srtpEnableAes128Sha1_32CryptoCipher")]
		bool SrtpEnableAes128Sha1_32CryptoCipher { get; set; }

		// @property (assign, nonatomic) BOOL srtpEnableEncryptedRtpHeaderExtensions;
		[Export ("srtpEnableEncryptedRtpHeaderExtensions")]
		bool SrtpEnableEncryptedRtpHeaderExtensions { get; set; }

		// @property (assign, nonatomic) BOOL sframeRequireFrameEncryption;
		[Export ("sframeRequireFrameEncryption")]
		bool SframeRequireFrameEncryption { get; set; }

		// -(instancetype _Nonnull)initWithSrtpEnableGcmCryptoSuites:(BOOL)srtpEnableGcmCryptoSuites srtpEnableAes128Sha1_32CryptoCipher:(BOOL)srtpEnableAes128Sha1_32CryptoCipher srtpEnableEncryptedRtpHeaderExtensions:(BOOL)srtpEnableEncryptedRtpHeaderExtensions sframeRequireFrameEncryption:(BOOL)sframeRequireFrameEncryption __attribute__((objc_designated_initializer));
		[Export ("initWithSrtpEnableGcmCryptoSuites:srtpEnableAes128Sha1_32CryptoCipher:srtpEnableEncryptedRtpHeaderExtensions:sframeRequireFrameEncryption:")]
		[DesignatedInitializer]
		NativeHandle Constructor (bool srtpEnableGcmCryptoSuites, bool srtpEnableAes128Sha1_32CryptoCipher, bool srtpEnableEncryptedRtpHeaderExtensions, bool sframeRequireFrameEncryption);
	}

	// @interface LKRTCConfiguration : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCConfiguration
	{
		// @property (assign, nonatomic) BOOL enableDscp;
		[Export ("enableDscp")]
		bool EnableDscp { get; set; }

		// @property (copy, nonatomic) NSArray<LKRTCIceServer *> * _Nonnull iceServers;
		[Export ("iceServers", ArgumentSemantic.Copy)]
		LKRTCIceServer[] IceServers { get; set; }

		// @property (nonatomic) LKRTCCertificate * _Nullable certificate;
		[NullAllowed, Export ("certificate", ArgumentSemantic.Assign)]
		LKRTCCertificate Certificate { get; set; }

		// @property (assign, nonatomic) LKRTCIceTransportPolicy iceTransportPolicy;
		[Export ("iceTransportPolicy", ArgumentSemantic.Assign)]
		LKRTCIceTransportPolicy IceTransportPolicy { get; set; }

		// @property (assign, nonatomic) LKRTCBundlePolicy bundlePolicy;
		[Export ("bundlePolicy", ArgumentSemantic.Assign)]
		LKRTCBundlePolicy BundlePolicy { get; set; }

		// @property (assign, nonatomic) LKRTCRtcpMuxPolicy rtcpMuxPolicy;
		[Export ("rtcpMuxPolicy", ArgumentSemantic.Assign)]
		LKRTCRtcpMuxPolicy RtcpMuxPolicy { get; set; }

		// @property (assign, nonatomic) LKRTCTcpCandidatePolicy tcpCandidatePolicy;
		[Export ("tcpCandidatePolicy", ArgumentSemantic.Assign)]
		LKRTCTcpCandidatePolicy TcpCandidatePolicy { get; set; }

		// @property (assign, nonatomic) LKRTCCandidateNetworkPolicy candidateNetworkPolicy;
		[Export ("candidateNetworkPolicy", ArgumentSemantic.Assign)]
		LKRTCCandidateNetworkPolicy CandidateNetworkPolicy { get; set; }

		// @property (assign, nonatomic) LKRTCContinualGatheringPolicy continualGatheringPolicy;
		[Export ("continualGatheringPolicy", ArgumentSemantic.Assign)]
		LKRTCContinualGatheringPolicy ContinualGatheringPolicy { get; set; }

		// @property (assign, nonatomic) BOOL disableIPV6OnWiFi;
		[Export ("disableIPV6OnWiFi")]
		bool DisableIPV6OnWiFi { get; set; }

		// @property (assign, nonatomic) int maxIPv6Networks;
		[Export ("maxIPv6Networks")]
		int MaxIPv6Networks { get; set; }

		// @property (assign, nonatomic) BOOL disableLinkLocalNetworks;
		[Export ("disableLinkLocalNetworks")]
		bool DisableLinkLocalNetworks { get; set; }

		// @property (assign, nonatomic) int audioJitterBufferMaxPackets;
		[Export ("audioJitterBufferMaxPackets")]
		int AudioJitterBufferMaxPackets { get; set; }

		// @property (assign, nonatomic) BOOL audioJitterBufferFastAccelerate;
		[Export ("audioJitterBufferFastAccelerate")]
		bool AudioJitterBufferFastAccelerate { get; set; }

		// @property (assign, nonatomic) int iceConnectionReceivingTimeout;
		[Export ("iceConnectionReceivingTimeout")]
		int IceConnectionReceivingTimeout { get; set; }

		// @property (assign, nonatomic) int iceBackupCandidatePairPingInterval;
		[Export ("iceBackupCandidatePairPingInterval")]
		int IceBackupCandidatePairPingInterval { get; set; }

		// @property (assign, nonatomic) LKRTCEncryptionKeyType keyType;
		[Export ("keyType", ArgumentSemantic.Assign)]
		LKRTCEncryptionKeyType KeyType { get; set; }

		// @property (assign, nonatomic) int iceCandidatePoolSize;
		[Export ("iceCandidatePoolSize")]
		int IceCandidatePoolSize { get; set; }

		// @property (assign, nonatomic) BOOL shouldPruneTurnPorts;
		[Export ("shouldPruneTurnPorts")]
		bool ShouldPruneTurnPorts { get; set; }

		// @property (assign, nonatomic) BOOL shouldPresumeWritableWhenFullyRelayed;
		[Export ("shouldPresumeWritableWhenFullyRelayed")]
		bool ShouldPresumeWritableWhenFullyRelayed { get; set; }

		// @property (assign, nonatomic) BOOL shouldSurfaceIceCandidatesOnIceTransportTypeChanged;
		[Export ("shouldSurfaceIceCandidatesOnIceTransportTypeChanged")]
		bool ShouldSurfaceIceCandidatesOnIceTransportTypeChanged { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceCheckMinInterval;
		[NullAllowed, Export ("iceCheckMinInterval", ArgumentSemantic.Copy)]
		NSNumber IceCheckMinInterval { get; set; }

		// @property (assign, nonatomic) LKRTCSdpSemantics sdpSemantics;
		[Export ("sdpSemantics", ArgumentSemantic.Assign)]
		LKRTCSdpSemantics SdpSemantics { get; set; }

		// @property (assign, nonatomic) BOOL activeResetSrtpParams;
		[Export ("activeResetSrtpParams")]
		bool ActiveResetSrtpParams { get; set; }

		// @property (assign, nonatomic) BOOL allowCodecSwitching;
		[Export ("allowCodecSwitching")]
		bool AllowCodecSwitching { get; set; }

		// @property (nonatomic) LKRTCCryptoOptions * _Nullable cryptoOptions;
		[NullAllowed, Export ("cryptoOptions", ArgumentSemantic.Assign)]
		LKRTCCryptoOptions CryptoOptions { get; set; }

		// @property (copy, nonatomic) NSString * _Nullable turnLoggingId;
		[NullAllowed, Export ("turnLoggingId")]
		string TurnLoggingId { get; set; }

		// @property (assign, nonatomic) int rtcpAudioReportIntervalMs;
		[Export ("rtcpAudioReportIntervalMs")]
		int RtcpAudioReportIntervalMs { get; set; }

		// @property (assign, nonatomic) int rtcpVideoReportIntervalMs;
		[Export ("rtcpVideoReportIntervalMs")]
		int RtcpVideoReportIntervalMs { get; set; }

		// @property (assign, nonatomic) BOOL enableImplicitRollback;
		[Export ("enableImplicitRollback")]
		bool EnableImplicitRollback { get; set; }

		// @property (assign, nonatomic) BOOL offerExtmapAllowMixed;
		[Export ("offerExtmapAllowMixed")]
		bool OfferExtmapAllowMixed { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceCheckIntervalStrongConnectivity;
		[NullAllowed, Export ("iceCheckIntervalStrongConnectivity", ArgumentSemantic.Copy)]
		NSNumber IceCheckIntervalStrongConnectivity { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceCheckIntervalWeakConnectivity;
		[NullAllowed, Export ("iceCheckIntervalWeakConnectivity", ArgumentSemantic.Copy)]
		NSNumber IceCheckIntervalWeakConnectivity { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceUnwritableTimeout;
		[NullAllowed, Export ("iceUnwritableTimeout", ArgumentSemantic.Copy)]
		NSNumber IceUnwritableTimeout { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceUnwritableMinChecks;
		[NullAllowed, Export ("iceUnwritableMinChecks", ArgumentSemantic.Copy)]
		NSNumber IceUnwritableMinChecks { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable iceInactiveTimeout;
		[NullAllowed, Export ("iceInactiveTimeout", ArgumentSemantic.Copy)]
		NSNumber IceInactiveTimeout { get; set; }
	}

	// @interface LKRTCDataBuffer : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCDataBuffer
	{
		// @property (readonly, nonatomic) NSData * _Nonnull data;
		[Export ("data")]
		NSData Data { get; }

		// @property (readonly, nonatomic) BOOL isBinary;
		[Export ("isBinary")]
		bool IsBinary { get; }

		// -(instancetype _Nonnull)initWithData:(NSData * _Nonnull)data isBinary:(BOOL)isBinary;
		[Export ("initWithData:isBinary:")]
		NativeHandle Constructor (NSData data, bool isBinary);
	}

	// @protocol LKRTCDataChannelDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCDataChannelDelegate
	{
		// @required -(void)dataChannelDidChangeState:(LKRTCDataChannel * _Nonnull)dataChannel;
		[Abstract]
		[Export ("dataChannelDidChangeState:")]
		void DataChannelDidChangeState (LKRTCDataChannel dataChannel);

		// @required -(void)dataChannel:(LKRTCDataChannel * _Nonnull)dataChannel didReceiveMessageWithBuffer:(LKRTCDataBuffer * _Nonnull)buffer;
		[Abstract]
		[Export ("dataChannel:didReceiveMessageWithBuffer:")]
		void DidReceiveMessageWithBuffer(LKRTCDataChannel dataChannel, LKRTCDataBuffer buffer);

		// @optional -(void)dataChannel:(LKRTCDataChannel * _Nonnull)dataChannel didChangeBufferedAmount:(uint64_t)amount;
		[Export ("dataChannel:didChangeBufferedAmount:")]
		void DidChangeBufferedAmount(LKRTCDataChannel dataChannel, ulong amount);
	}

	// @interface LKRTCDataChannel : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCDataChannel
	{
		// @property (readonly, nonatomic) NSString * _Nonnull label;
		[Export ("label")]
		string Label { get; }

		// @property (readonly, nonatomic) BOOL isReliable __attribute__((deprecated("")));
		[Export ("isReliable")]
		bool IsReliable { get; }

		// @property (readonly, nonatomic) BOOL isOrdered;
		[Export ("isOrdered")]
		bool IsOrdered { get; }

		// @property (readonly, nonatomic) NSUInteger maxRetransmitTime __attribute__((deprecated("")));
		[Export ("maxRetransmitTime")]
		nuint MaxRetransmitTime { get; }

		// @property (readonly, nonatomic) uint16_t maxPacketLifeTime;
		[Export ("maxPacketLifeTime")]
		ushort MaxPacketLifeTime { get; }

		// @property (readonly, nonatomic) uint16_t maxRetransmits;
		[Export ("maxRetransmits")]
		ushort MaxRetransmits { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull protocol;
		[Export ("protocol")]
		string Protocol { get; }

		// @property (readonly, nonatomic) BOOL isNegotiated;
		[Export ("isNegotiated")]
		bool IsNegotiated { get; }

		// @property (readonly, nonatomic) NSInteger streamId __attribute__((deprecated("")));
		[Export ("streamId")]
		nint StreamId { get; }

		// @property (readonly, nonatomic) int channelId;
		[Export ("channelId")]
		int ChannelId { get; }

		// @property (readonly, nonatomic) LKRTCDataChannelState readyState;
		[Export ("readyState")]
		LKRTCDataChannelState ReadyState { get; }

		// @property (readonly, nonatomic) uint64_t bufferedAmount;
		[Export ("bufferedAmount")]
		ulong BufferedAmount { get; }

		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ILKRTCDataChannelDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<LKRTCDataChannelDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// -(void)close;
		[Export ("close")]
		void Close ();

		// -(BOOL)sendData:(LKRTCDataBuffer * _Nonnull)data;
		[Export ("sendData:")]
		bool SendData (LKRTCDataBuffer data);
	}

	// @interface LKRTCDataChannelConfiguration : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCDataChannelConfiguration
	{
		// @property (assign, nonatomic) BOOL isOrdered;
		[Export ("isOrdered")]
		bool IsOrdered { get; set; }

		// @property (assign, nonatomic) NSInteger maxRetransmitTimeMs __attribute__((deprecated("")));
		[Export ("maxRetransmitTimeMs")]
		nint MaxRetransmitTimeMs { get; set; }

		// @property (assign, nonatomic) int maxPacketLifeTime;
		[Export ("maxPacketLifeTime")]
		int MaxPacketLifeTime { get; set; }

		// @property (assign, nonatomic) int maxRetransmits;
		[Export ("maxRetransmits")]
		int MaxRetransmits { get; set; }

		// @property (assign, nonatomic) BOOL isNegotiated;
		[Export ("isNegotiated")]
		bool IsNegotiated { get; set; }

		// @property (assign, nonatomic) int streamId __attribute__((deprecated("")));
		[Export ("streamId")]
		int StreamId { get; set; }

		// @property (assign, nonatomic) int channelId;
		[Export ("channelId")]
		int ChannelId { get; set; }

		// @property (nonatomic) NSString * _Nonnull protocol;
		[Export ("protocol")]
		string Protocol { get; set; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const kLKRTCFieldTrialAudioForceABWENoTWCCKey;
		[Field ("kLKRTCFieldTrialAudioForceABWENoTWCCKey", "__Internal")]
		NSString kLKRTCFieldTrialAudioForceABWENoTWCCKey { get; }

		// extern NSString *const kLKRTCFieldTrialFlexFec03AdvertisedKey;
		[Field ("kLKRTCFieldTrialFlexFec03AdvertisedKey", "__Internal")]
		NSString kLKRTCFieldTrialFlexFec03AdvertisedKey { get; }

		// extern NSString *const kLKRTCFieldTrialFlexFec03Key;
		[Field ("kLKRTCFieldTrialFlexFec03Key", "__Internal")]
		NSString kLKRTCFieldTrialFlexFec03Key { get; }

		// extern NSString *const kLKRTCFieldTrialH264HighProfileKey;
		[Field ("kLKRTCFieldTrialH264HighProfileKey", "__Internal")]
		NSString kLKRTCFieldTrialH264HighProfileKey { get; }

		// extern NSString *const kLKRTCFieldTrialMinimizeResamplingOnMobileKey;
		[Field ("kLKRTCFieldTrialMinimizeResamplingOnMobileKey", "__Internal")]
		NSString kLKRTCFieldTrialMinimizeResamplingOnMobileKey { get; }

		// extern NSString *const kLKRTCFieldTrialUseNWPathMonitor;
		[Field ("kLKRTCFieldTrialUseNWPathMonitor", "__Internal")]
		NSString kLKRTCFieldTrialUseNWPathMonitor { get; }

		// extern NSString *const kLKRTCFieldTrialEnabledValue;
		[Field ("kLKRTCFieldTrialEnabledValue", "__Internal")]
		NSString kLKRTCFieldTrialEnabledValue { get; }
	}

	// @interface LKRTCIceCandidate : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCIceCandidate
	{
		// @property (readonly, nonatomic) NSString * _Nullable sdpMid;
		[NullAllowed, Export ("sdpMid")]
		string SdpMid { get; }

		// @property (readonly, nonatomic) int sdpMLineIndex;
		[Export ("sdpMLineIndex")]
		int SdpMLineIndex { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull sdp;
		[Export ("sdp")]
		string Sdp { get; }

		// @property (readonly, nonatomic) NSString * _Nullable serverUrl;
		[NullAllowed, Export ("serverUrl")]
		string ServerUrl { get; }

		// -(instancetype _Nonnull)initWithSdp:(NSString * _Nonnull)sdp sdpMLineIndex:(int)sdpMLineIndex sdpMid:(NSString * _Nullable)sdpMid __attribute__((objc_designated_initializer));
		[Export ("initWithSdp:sdpMLineIndex:sdpMid:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string sdp, int sdpMLineIndex, [NullAllowed] string sdpMid);
	}

	// @interface LKRTCIceCandidateErrorEvent : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCIceCandidateErrorEvent
	{
		// @property (readonly, nonatomic) NSString * _Nonnull address;
		[Export ("address")]
		string Address { get; }

		// @property (readonly, nonatomic) int port;
		[Export ("port")]
		int Port { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull url;
		[Export ("url")]
		string Url { get; }

		// @property (readonly, nonatomic) int errorCode;
		[Export ("errorCode")]
		int ErrorCode { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull errorText;
		[Export ("errorText")]
		string ErrorText { get; }
	}

	// @interface LKRTCIceServer : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCIceServer
	{
		// @property (readonly, nonatomic) NSArray<NSString *> * _Nonnull urlStrings;
		[Export ("urlStrings")]
		string[] UrlStrings { get; }

		// @property (readonly, nonatomic) NSString * _Nullable username;
		[NullAllowed, Export ("username")]
		string Username { get; }

		// @property (readonly, nonatomic) NSString * _Nullable credential;
		[NullAllowed, Export ("credential")]
		string Credential { get; }

		// @property (readonly, nonatomic) LKRTCTlsCertPolicy tlsCertPolicy;
		[Export ("tlsCertPolicy")]
		LKRTCTlsCertPolicy TlsCertPolicy { get; }

		// @property (readonly, nonatomic) NSString * _Nullable hostname;
		[NullAllowed, Export ("hostname")]
		string Hostname { get; }

		// @property (readonly, nonatomic) NSArray<NSString *> * _Nonnull tlsAlpnProtocols;
		[Export ("tlsAlpnProtocols")]
		string[] TlsAlpnProtocols { get; }

		// @property (readonly, nonatomic) NSArray<NSString *> * _Nonnull tlsEllipticCurves;
		[Export ("tlsEllipticCurves")]
		string[] TlsEllipticCurves { get; }

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings;
		[Export ("initWithURLStrings:")]
		NativeHandle Constructor (string[] urlStrings);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential;
		[Export ("initWithURLStrings:username:credential:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(LKRTCTlsCertPolicy)tlsCertPolicy;
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, LKRTCTlsCertPolicy tlsCertPolicy);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(LKRTCTlsCertPolicy)tlsCertPolicy hostname:(NSString * _Nullable)hostname;
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:hostname:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, LKRTCTlsCertPolicy tlsCertPolicy, [NullAllowed] string hostname);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(LKRTCTlsCertPolicy)tlsCertPolicy hostname:(NSString * _Nullable)hostname tlsAlpnProtocols:(NSArray<NSString *> * _Nonnull)tlsAlpnProtocols;
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:hostname:tlsAlpnProtocols:")]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, LKRTCTlsCertPolicy tlsCertPolicy, [NullAllowed] string hostname, string[] tlsAlpnProtocols);

		// -(instancetype _Nonnull)initWithURLStrings:(NSArray<NSString *> * _Nonnull)urlStrings username:(NSString * _Nullable)username credential:(NSString * _Nullable)credential tlsCertPolicy:(LKRTCTlsCertPolicy)tlsCertPolicy hostname:(NSString * _Nullable)hostname tlsAlpnProtocols:(NSArray<NSString *> * _Nullable)tlsAlpnProtocols tlsEllipticCurves:(NSArray<NSString *> * _Nullable)tlsEllipticCurves __attribute__((objc_designated_initializer));
		[Export ("initWithURLStrings:username:credential:tlsCertPolicy:hostname:tlsAlpnProtocols:tlsEllipticCurves:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string[] urlStrings, [NullAllowed] string username, [NullAllowed] string credential, LKRTCTlsCertPolicy tlsCertPolicy, [NullAllowed] string hostname, [NullAllowed] string[] tlsAlpnProtocols, [NullAllowed] string[] tlsEllipticCurves);
	}

	// @interface LKRTCLegacyStatsReport : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCLegacyStatsReport
		: INativeObject
	{
		// @property (readonly, nonatomic) CFTimeInterval timestamp;
		[Export ("timestamp")]
		double Timestamp { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull type;
		[Export ("type")]
		string Type { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull reportId;
		[Export ("reportId")]
		string ReportId { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,NSString *> * _Nonnull values;
		[Export ("values")]
		NSDictionary<NSString, NSString> Values { get; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const _Nonnull kLKRTCMediaConstraintsAudioNetworkAdaptorConfig;
		[Field ("kLKRTCMediaConstraintsAudioNetworkAdaptorConfig", "__Internal")]
		NSString kLKRTCMediaConstraintsAudioNetworkAdaptorConfig { get; }

		// extern NSString *const _Nonnull kLKRTCMediaConstraintsIceRestart;
		[Field ("kLKRTCMediaConstraintsIceRestart", "__Internal")]
		NSString kLKRTCMediaConstraintsIceRestart { get; }

		// extern NSString *const _Nonnull kLKRTCMediaConstraintsOfferToReceiveAudio;
		[Field ("kLKRTCMediaConstraintsOfferToReceiveAudio", "__Internal")]
		NSString kLKRTCMediaConstraintsOfferToReceiveAudio { get; }

		// extern NSString *const _Nonnull kLKRTCMediaConstraintsOfferToReceiveVideo;
		[Field ("kLKRTCMediaConstraintsOfferToReceiveVideo", "__Internal")]
		NSString kLKRTCMediaConstraintsOfferToReceiveVideo { get; }

		// extern NSString *const _Nonnull kLKRTCMediaConstraintsVoiceActivityDetection;
		[Field ("kLKRTCMediaConstraintsVoiceActivityDetection", "__Internal")]
		NSString kLKRTCMediaConstraintsVoiceActivityDetection { get; }

		// extern NSString *const _Nonnull kLKRTCMediaConstraintsValueTrue;
		[Field ("kLKRTCMediaConstraintsValueTrue", "__Internal")]
		NSString kLKRTCMediaConstraintsValueTrue { get; }

		// extern NSString *const _Nonnull kLKRTCMediaConstraintsValueFalse;
		[Field ("kLKRTCMediaConstraintsValueFalse", "__Internal")]
		NSString kLKRTCMediaConstraintsValueFalse { get; }
	}

	// @interface LKRTCMediaConstraints : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCMediaConstraints
	{
		// -(instancetype _Nonnull)initWithMandatoryConstraints:(NSDictionary<NSString *,NSString *> * _Nullable)mandatory optionalConstraints:(NSDictionary<NSString *,NSString *> * _Nullable)optional __attribute__((objc_designated_initializer));
		[Export ("initWithMandatoryConstraints:optionalConstraints:")]
		[DesignatedInitializer]
		NativeHandle Constructor ([NullAllowed] NSDictionary<NSString, NSString> mandatory, [NullAllowed] NSDictionary<NSString, NSString> optional);
	}

	// @interface LKRTCMediaStream : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCMediaStream
	{
		// @property (readonly, nonatomic, strong) NSArray<LKRTCAudioTrack *> * _Nonnull audioTracks;
		[Export ("audioTracks", ArgumentSemantic.Strong)]
		LKRTCAudioTrack[] AudioTracks { get; }

		// @property (readonly, nonatomic, strong) NSArray<LKRTCVideoTrack *> * _Nonnull videoTracks;
		[Export ("videoTracks", ArgumentSemantic.Strong)]
		LKRTCVideoTrack[] VideoTracks { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull streamId;
		[Export ("streamId")]
		string StreamId { get; }

		// -(void)addAudioTrack:(LKRTCAudioTrack * _Nonnull)audioTrack;
		[Export ("addAudioTrack:")]
		void AddAudioTrack (LKRTCAudioTrack audioTrack);

		// -(void)addVideoTrack:(LKRTCVideoTrack * _Nonnull)videoTrack;
		[Export ("addVideoTrack:")]
		void AddVideoTrack (LKRTCVideoTrack videoTrack);

		// -(void)removeAudioTrack:(LKRTCAudioTrack * _Nonnull)audioTrack;
		[Export ("removeAudioTrack:")]
		void RemoveAudioTrack (LKRTCAudioTrack audioTrack);

		// -(void)removeVideoTrack:(LKRTCVideoTrack * _Nonnull)videoTrack;
		[Export ("removeVideoTrack:")]
		void RemoveVideoTrack (LKRTCVideoTrack videoTrack);
	}

	// @interface LKRTCMetricsSampleInfo : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCMetricsSampleInfo
	{
		// @property (readonly, nonatomic) NSString * _Nonnull name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, nonatomic) int min;
		[Export ("min")]
		int Min { get; }

		// @property (readonly, nonatomic) int max;
		[Export ("max")]
		int Max { get; }

		// @property (readonly, nonatomic) int bucketCount;
		[Export ("bucketCount")]
		int BucketCount { get; }

		// @property (readonly, nonatomic) NSDictionary<NSNumber *,NSNumber *> * _Nonnull samples;
		[Export ("samples")]
		NSDictionary<NSNumber, NSNumber> Samples { get; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		/*
		// extern NSString *const _Nonnull kLKRTCPeerConnectionErrorDomain;
		[Field ("kLKRTCPeerConnectionErrorDomain", "__Internal")]
		NSString kLKRTCPeerConnectionErrorDomain { get; }
		*/
		
		/*
		// extern const int kLKRTCSessionDescriptionErrorCode;
		[Field ("kLKRTCSessionDescriptionErrorCode", "__Internal")]
		int kLKRTCSessionDescriptionErrorCode { get; }
		*/
	}

	// typedef void (^LKRTCCreateSessionDescriptionCompletionHandler)(LKRTCSessionDescription * _Nullable, NSError * _Nullable);
	delegate void LKRTCCreateSessionDescriptionCompletionHandler ([NullAllowed] LKRTCSessionDescription arg0, [NullAllowed] NSError arg1);

	// typedef void (^LKRTCSetSessionDescriptionCompletionHandler)(NSError * _Nullable);
	delegate void LKRTCSetSessionDescriptionCompletionHandler ([NullAllowed] NSError arg0);

	// @protocol LKRTCPeerConnectionDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCPeerConnectionDelegate
	{
		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didChangeSignalingState:(LKRTCSignalingState)stateChanged;
		[Abstract]
		[Export ("peerConnection:didChangeSignalingState:")]
		void DidChangeSignalingState(LKRTCPeerConnection peerConnection, LKRTCSignalingState stateChanged);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didAddStream:(LKRTCMediaStream * _Nonnull)stream;
		[Abstract]
		[Export ("peerConnection:didAddStream:")]
		void DidAddStream(LKRTCPeerConnection peerConnection, LKRTCMediaStream stream);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didRemoveStream:(LKRTCMediaStream * _Nonnull)stream;
		[Abstract]
		[Export ("peerConnection:didRemoveStream:")]
		void DidRemoveStream(LKRTCPeerConnection peerConnection, LKRTCMediaStream stream);

		// @required -(void)peerConnectionShouldNegotiate:(LKRTCPeerConnection * _Nonnull)peerConnection;
		[Abstract]
		[Export ("peerConnectionShouldNegotiate:")]
		void ShouldNegotiate (LKRTCPeerConnection peerConnection);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didChangeIceConnectionState:(LKRTCIceConnectionState)newState;
		[Abstract]
		[Export ("peerConnection:didChangeIceConnectionState:")]
		void DidChangeIceConnectionState(LKRTCPeerConnection peerConnection, LKRTCIceConnectionState newState);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didChangeIceGatheringState:(LKRTCIceGatheringState)newState;
		[Abstract]
		[Export ("peerConnection:didChangeIceGatheringState:")]
		void DidChangeIceGatheringState(LKRTCPeerConnection peerConnection, LKRTCIceGatheringState newState);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didGenerateIceCandidate:(LKRTCIceCandidate * _Nonnull)candidate;
		[Abstract]
		[Export ("peerConnection:didGenerateIceCandidate:")]
		void DidGenerateIceCandidate(LKRTCPeerConnection peerConnection, LKRTCIceCandidate candidate);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didRemoveIceCandidates:(NSArray<LKRTCIceCandidate *> * _Nonnull)candidates;
		[Abstract]
		[Export ("peerConnection:didRemoveIceCandidates:")]
		void DidRemoveIceCandidates(LKRTCPeerConnection peerConnection, LKRTCIceCandidate[] candidates);

		// @required -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didOpenDataChannel:(LKRTCDataChannel * _Nonnull)dataChannel;
		[Abstract]
		[Export ("peerConnection:didOpenDataChannel:")]
		void DidOpenDataChannel(LKRTCPeerConnection peerConnection, LKRTCDataChannel dataChannel);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didChangeStandardizedIceConnectionState:(LKRTCIceConnectionState)newState;
		[Export ("peerConnection:didChangeStandardizedIceConnectionState:")]
		void DidChangeStandardizedIceConnectionState(LKRTCPeerConnection peerConnection, LKRTCIceConnectionState newState);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didChangeConnectionState:(LKRTCPeerConnectionState)newState;
		[Abstract]		
		[Export ("peerConnection:didChangeConnectionState:")]
		void DidChangeConnectionState(LKRTCPeerConnection peerConnection, LKRTCPeerConnectionState newState);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didStartReceivingOnTransceiver:(LKRTCRtpTransceiver * _Nonnull)transceiver;
		[Export ("peerConnection:didStartReceivingOnTransceiver:")]
		void DidStartReceivingOnTransceiver(LKRTCPeerConnection peerConnection, LKRTCRtpTransceiver transceiver);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didAddReceiver:(LKRTCRtpReceiver * _Nonnull)rtpReceiver streams:(NSArray<LKRTCMediaStream *> * _Nonnull)mediaStreams;
		[Export ("peerConnection:didAddReceiver:streams:")]
		void DidAddReceiver(LKRTCPeerConnection peerConnection, LKRTCRtpReceiver rtpReceiver, LKRTCMediaStream[] mediaStreams);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didRemoveReceiver:(LKRTCRtpReceiver * _Nonnull)rtpReceiver;
		[Export ("peerConnection:didRemoveReceiver:")]
		void DidRemoveReceiver(LKRTCPeerConnection peerConnection, LKRTCRtpReceiver rtpReceiver);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didChangeLocalCandidate:(LKRTCIceCandidate * _Nonnull)local remoteCandidate:(LKRTCIceCandidate * _Nonnull)remote lastReceivedMs:(int)lastDataReceivedMs changeReason:(NSString * _Nonnull)reason;
		[Export ("peerConnection:didChangeLocalCandidate:remoteCandidate:lastReceivedMs:changeReason:")]
		void DidChangeLocalCandidate(LKRTCPeerConnection peerConnection, LKRTCIceCandidate local, LKRTCIceCandidate remote, int lastDataReceivedMs, string reason);

		// @optional -(void)peerConnection:(LKRTCPeerConnection * _Nonnull)peerConnection didFailToGatherIceCandidate:(LKRTCIceCandidateErrorEvent * _Nonnull)event;
		[Export ("peerConnection:didFailToGatherIceCandidate:")]
		void DidFailToGatherIceCandidate(LKRTCPeerConnection peerConnection, LKRTCIceCandidateErrorEvent @event);
	}

	// @interface LKRTCPeerConnection : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCPeerConnection
	{
		[Wrap ("WeakDelegate")]
		[NullAllowed]
		ILKRTCPeerConnectionDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<LKRTCPeerConnectionDelegate> _Nullable delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// @property (readonly, nonatomic) NSArray<LKRTCMediaStream *> * _Nonnull localStreams;
		[Export ("localStreams")]
		LKRTCMediaStream[] LocalStreams { get; }

		// @property (readonly, nonatomic) LKRTCSessionDescription * _Nullable localDescription;
		[NullAllowed, Export ("localDescription")]
		LKRTCSessionDescription LocalDescription { get; }

		// @property (readonly, nonatomic) LKRTCSessionDescription * _Nullable remoteDescription;
		[NullAllowed, Export ("remoteDescription")]
		LKRTCSessionDescription RemoteDescription { get; }

		// @property (readonly, nonatomic) LKRTCSignalingState signalingState;
		[Export ("signalingState")]
		LKRTCSignalingState SignalingState { get; }

		// @property (readonly, nonatomic) LKRTCIceConnectionState iceConnectionState;
		[Export ("iceConnectionState")]
		LKRTCIceConnectionState IceConnectionState { get; }

		// @property (readonly, nonatomic) LKRTCPeerConnectionState connectionState;
		[Export ("connectionState")]
		LKRTCPeerConnectionState ConnectionState { get; }

		// @property (readonly, nonatomic) LKRTCIceGatheringState iceGatheringState;
		[Export ("iceGatheringState")]
		LKRTCIceGatheringState IceGatheringState { get; }

		// @property (readonly, copy, nonatomic) LKRTCConfiguration * _Nonnull configuration;
		[Export ("configuration", ArgumentSemantic.Copy)]
		LKRTCConfiguration Configuration { get; }

		// @property (readonly, nonatomic) NSArray<LKRTCRtpSender *> * _Nonnull senders;
		[Export ("senders")]
		LKRTCRtpSender[] Senders { get; }

		// @property (readonly, nonatomic) NSArray<LKRTCRtpReceiver *> * _Nonnull receivers;
		[Export ("receivers")]
		LKRTCRtpReceiver[] Receivers { get; }

		// @property (readonly, nonatomic) NSArray<LKRTCRtpTransceiver *> * _Nonnull transceivers;
		[Export ("transceivers")]
		LKRTCRtpTransceiver[] Transceivers { get; }

		// -(BOOL)setConfiguration:(LKRTCConfiguration * _Nonnull)configuration;
		[Export ("setConfiguration:")]
		bool SetConfiguration (LKRTCConfiguration configuration);

		// -(void)close;
		[Export ("close")]
		void Close ();

		// -(void)addIceCandidate:(LKRTCIceCandidate * _Nonnull)candidate __attribute__((deprecated("Please use addIceCandidate:completionHandler: instead")));
		[Export ("addIceCandidate:")]
		void AddIceCandidate (LKRTCIceCandidate candidate);

		// -(void)addIceCandidate:(LKRTCIceCandidate * _Nonnull)candidate completionHandler:(void (^ _Nonnull)(NSError * _Nullable))completionHandler;
		[Export ("addIceCandidate:completionHandler:")]
		void AddIceCandidate (LKRTCIceCandidate candidate, Action<NSError> completionHandler);

		// -(void)removeIceCandidates:(NSArray<LKRTCIceCandidate *> * _Nonnull)candidates;
		[Export ("removeIceCandidates:")]
		void RemoveIceCandidates (LKRTCIceCandidate[] candidates);

		// -(void)addStream:(LKRTCMediaStream * _Nonnull)stream;
		[Export ("addStream:")]
		void AddStream (LKRTCMediaStream stream);

		// -(void)removeStream:(LKRTCMediaStream * _Nonnull)stream;
		[Export ("removeStream:")]
		void RemoveStream (LKRTCMediaStream stream);

		// -(LKRTCRtpSender * _Nullable)addTrack:(LKRTCMediaStreamTrack * _Nonnull)track streamIds:(NSArray<NSString *> * _Nonnull)streamIds;
		[Export ("addTrack:streamIds:")]
		[return: NullAllowed]
		LKRTCRtpSender AddTrack (LKRTCMediaStreamTrack track, string[] streamIds);

		// -(BOOL)removeTrack:(LKRTCRtpSender * _Nonnull)sender;
		[Export ("removeTrack:")]
		bool RemoveTrack (LKRTCRtpSender sender);

		// -(LKRTCRtpTransceiver * _Nullable)addTransceiverWithTrack:(LKRTCMediaStreamTrack * _Nonnull)track;
		[Export ("addTransceiverWithTrack:")]
		[return: NullAllowed]
		LKRTCRtpTransceiver AddTransceiverWithTrack (LKRTCMediaStreamTrack track);

		// -(LKRTCRtpTransceiver * _Nullable)addTransceiverWithTrack:(LKRTCMediaStreamTrack * _Nonnull)track init:(LKRTCRtpTransceiverInit * _Nonnull)init;
		[Export ("addTransceiverWithTrack:init:")]
		[return: NullAllowed]
		LKRTCRtpTransceiver AddTransceiverWithTrack (LKRTCMediaStreamTrack track, LKRTCRtpTransceiverInit init);

		// -(LKRTCRtpTransceiver * _Nullable)addTransceiverOfType:(LKRTCRtpMediaType)mediaType;
		[Export ("addTransceiverOfType:")]
		[return: NullAllowed]
		LKRTCRtpTransceiver AddTransceiverOfType (LKRTCRtpMediaType mediaType);

		// -(LKRTCRtpTransceiver * _Nullable)addTransceiverOfType:(LKRTCRtpMediaType)mediaType init:(LKRTCRtpTransceiverInit * _Nonnull)init;
		[Export ("addTransceiverOfType:init:")]
		[return: NullAllowed]
		LKRTCRtpTransceiver AddTransceiverOfType (LKRTCRtpMediaType mediaType, LKRTCRtpTransceiverInit init);

		// -(void)restartIce;
		[Export ("restartIce")]
		void RestartIce ();

		// -(void)offerForConstraints:(LKRTCMediaConstraints * _Nonnull)constraints completionHandler:(LKRTCCreateSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("offerForConstraints:completionHandler:")]
		void OfferForConstraints (LKRTCMediaConstraints constraints, LKRTCCreateSessionDescriptionCompletionHandler completionHandler);

		// -(void)answerForConstraints:(LKRTCMediaConstraints * _Nonnull)constraints completionHandler:(LKRTCCreateSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("answerForConstraints:completionHandler:")]
		void AnswerForConstraints (LKRTCMediaConstraints constraints, LKRTCCreateSessionDescriptionCompletionHandler completionHandler);

		// -(void)setLocalDescription:(LKRTCSessionDescription * _Nonnull)sdp completionHandler:(LKRTCSetSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("setLocalDescription:completionHandler:")]
		void SetLocalDescription (LKRTCSessionDescription sdp, LKRTCSetSessionDescriptionCompletionHandler completionHandler);

		// -(void)setLocalDescriptionWithCompletionHandler:(LKRTCSetSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("setLocalDescriptionWithCompletionHandler:")]
		void SetLocalDescriptionWithCompletionHandler (LKRTCSetSessionDescriptionCompletionHandler completionHandler);

		// -(void)setRemoteDescription:(LKRTCSessionDescription * _Nonnull)sdp completionHandler:(LKRTCSetSessionDescriptionCompletionHandler _Nonnull)completionHandler;
		[Export ("setRemoteDescription:completionHandler:")]
		void SetRemoteDescription (LKRTCSessionDescription sdp, LKRTCSetSessionDescriptionCompletionHandler completionHandler);

		// -(BOOL)setBweMinBitrateBps:(NSNumber * _Nullable)minBitrateBps currentBitrateBps:(NSNumber * _Nullable)currentBitrateBps maxBitrateBps:(NSNumber * _Nullable)maxBitrateBps;
		[Export ("setBweMinBitrateBps:currentBitrateBps:maxBitrateBps:")]
		bool SetBweMinBitrateBps ([NullAllowed] NSNumber minBitrateBps, [NullAllowed] NSNumber currentBitrateBps, [NullAllowed] NSNumber maxBitrateBps);

		// -(BOOL)startRtcEventLogWithFilePath:(NSString * _Nonnull)filePath maxSizeInBytes:(int64_t)maxSizeInBytes;
		[Export ("startRtcEventLogWithFilePath:maxSizeInBytes:")]
		bool StartRtcEventLogWithFilePath (string filePath, long maxSizeInBytes);

		// -(void)stopRtcEventLog;
		[Export ("stopRtcEventLog")]
		void StopRtcEventLog ();
	}

	// @interface Media (LKRTCPeerConnection)
	[Category]
	[BaseType (typeof(LKRTCPeerConnection))]
	interface LKRTCPeerConnection_Media
	{
		// -(LKRTCRtpSender * _Nonnull)senderWithKind:(NSString * _Nonnull)kind streamId:(NSString * _Nonnull)streamId;
		[Export ("senderWithKind:streamId:")]
		LKRTCRtpSender SenderWithKind (string kind, string streamId);
	}

	// @interface DataChannel (LKRTCPeerConnection)
	[Category]
	[BaseType (typeof(LKRTCPeerConnection))]
	interface LKRTCPeerConnection_DataChannel
	{
		// -(LKRTCDataChannel * _Nullable)dataChannelForLabel:(NSString * _Nonnull)label configuration:(LKRTCDataChannelConfiguration * _Nonnull)configuration;
		[Export ("dataChannelForLabel:configuration:")]
		[return: NullAllowed]
		LKRTCDataChannel DataChannelForLabel (string label, LKRTCDataChannelConfiguration configuration);
	}

	// typedef void (^LKRTCStatisticsCompletionHandler)(LKRTCStatisticsReport * _Nonnull);
	delegate void LKRTCStatisticsCompletionHandler (LKRTCStatisticsReport arg0);

	// @interface Stats (LKRTCPeerConnection)
	[Category]
	[BaseType (typeof(LKRTCPeerConnection))]
	interface LKRTCPeerConnection_Stats
	{
		// -(void)statsForTrack:(LKRTCMediaStreamTrack * _Nullable)mediaStreamTrack statsOutputLevel:(LKRTCStatsOutputLevel)statsOutputLevel completionHandler:(void (^ _Nullable)(NSArray<LKRTCLegacyStatsReport *> * _Nonnull))completionHandler;
		[Export ("statsForTrack:statsOutputLevel:completionHandler:")]
		void StatsForTrack ([NullAllowed] LKRTCMediaStreamTrack mediaStreamTrack, LKRTCStatsOutputLevel statsOutputLevel, [NullAllowed] Action<NSArray<LKRTCLegacyStatsReport>> completionHandler);

		// -(void)statisticsWithCompletionHandler:(LKRTCStatisticsCompletionHandler _Nonnull)completionHandler;
		[Export ("statisticsWithCompletionHandler:")]
		void StatisticsWithCompletionHandler (LKRTCStatisticsCompletionHandler completionHandler);

		// -(void)statisticsForSender:(LKRTCRtpSender * _Nonnull)sender completionHandler:(LKRTCStatisticsCompletionHandler _Nonnull)completionHandler;
		[Export ("statisticsForSender:completionHandler:")]
		void StatisticsForSender (LKRTCRtpSender sender, LKRTCStatisticsCompletionHandler completionHandler);

		// -(void)statisticsForReceiver:(LKRTCRtpReceiver * _Nonnull)receiver completionHandler:(LKRTCStatisticsCompletionHandler _Nonnull)completionHandler;
		[Export ("statisticsForReceiver:completionHandler:")]
		void StatisticsForReceiver (LKRTCRtpReceiver receiver, LKRTCStatisticsCompletionHandler completionHandler);
	}

	// @interface LKRTCPeerConnectionFactory : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCPeerConnectionFactory
	{
		// -(instancetype _Nonnull)initWithEncoderFactory:(id<LKRTCVideoEncoderFactory> _Nullable)encoderFactory decoderFactory:(id<LKRTCVideoDecoderFactory> _Nullable)decoderFactory;
		[Export ("initWithEncoderFactory:decoderFactory:")]
		NativeHandle Constructor ([NullAllowed] LKRTCVideoEncoderFactory encoderFactory, [NullAllowed] LKRTCVideoDecoderFactory decoderFactory);

		// -(instancetype _Nonnull)initWithEncoderFactory:(id<LKRTCVideoEncoderFactory> _Nullable)encoderFactory decoderFactory:(id<LKRTCVideoDecoderFactory> _Nullable)decoderFactory audioDevice:(id<LKRTCAudioDevice> _Nullable)audioDevice;
		[Export ("initWithEncoderFactory:decoderFactory:audioDevice:")]
		NativeHandle Constructor ([NullAllowed] LKRTCVideoEncoderFactory encoderFactory, [NullAllowed] LKRTCVideoDecoderFactory decoderFactory, [NullAllowed] LKRTCAudioDevice audioDevice);

		// -(instancetype _Nonnull)initWithAudioDeviceModuleType:(LKRTCAudioDeviceModuleType)audioDeviceModuleType bypassVoiceProcessing:(BOOL)bypassVoiceProcessing encoderFactory:(id<LKRTCVideoEncoderFactory> _Nullable)encoderFactory decoderFactory:(id<LKRTCVideoDecoderFactory> _Nullable)decoderFactory audioProcessingModule:(id<LKRTCAudioProcessingModule> _Nullable)audioProcessingModule;
		[Export ("initWithAudioDeviceModuleType:bypassVoiceProcessing:encoderFactory:decoderFactory:audioProcessingModule:")]
		NativeHandle Constructor (LKRTCAudioDeviceModuleType audioDeviceModuleType, bool bypassVoiceProcessing, [NullAllowed] LKRTCVideoEncoderFactory encoderFactory, [NullAllowed] LKRTCVideoDecoderFactory decoderFactory, [NullAllowed] ILKRTCAudioProcessingModule audioProcessingModule);

		// @property (readonly, nonatomic) LKRTCAudioDeviceModule * _Nonnull audioDeviceModule;
		[Export ("audioDeviceModule")]
		LKRTCAudioDeviceModule AudioDeviceModule { get; }

		// @property (readonly, nonatomic) LKRTCAudioProcessingState * _Nonnull audioProcessingState;
		[Export ("audioProcessingState")]
		LKRTCAudioProcessingState AudioProcessingState { get; }

		// +(void)configureFieldTrials:(NSString * _Nullable)fieldTrials;
		[Static]
		[Export ("configureFieldTrials:")]
		void ConfigureFieldTrials ([NullAllowed] string fieldTrials);

		// -(LKRTCRtpCapabilities * _Nonnull)rtpSenderCapabilitiesForKind:(NSString * _Nonnull)kind;
		[Export ("rtpSenderCapabilitiesForKind:")]
		LKRTCRtpCapabilities RtpSenderCapabilitiesForKind (string kind);

		// -(LKRTCRtpCapabilities * _Nonnull)rtpReceiverCapabilitiesForKind:(NSString * _Nonnull)kind;
		[Export ("rtpReceiverCapabilitiesForKind:")]
		LKRTCRtpCapabilities RtpReceiverCapabilitiesForKind (string kind);

		// -(LKRTCAudioSource * _Nonnull)audioSourceWithConstraints:(LKRTCMediaConstraints * _Nullable)constraints;
		[Export ("audioSourceWithConstraints:")]
		LKRTCAudioSource AudioSourceWithConstraints ([NullAllowed] LKRTCMediaConstraints constraints);

		// -(LKRTCAudioTrack * _Nonnull)audioTrackWithTrackId:(NSString * _Nonnull)trackId;
		[Export ("audioTrackWithTrackId:")]
		LKRTCAudioTrack AudioTrackWithTrackId (string trackId);

		// -(LKRTCAudioTrack * _Nonnull)audioTrackWithSource:(LKRTCAudioSource * _Nonnull)source trackId:(NSString * _Nonnull)trackId;
		[Export ("audioTrackWithSource:trackId:")]
		LKRTCAudioTrack AudioTrackWithSource (LKRTCAudioSource source, string trackId);

		// -(LKRTCVideoSource * _Nonnull)videoSource;
		[Export ("videoSource")]
		////[Verify (MethodToProperty)]
		LKRTCVideoSource VideoSource { get; }

		// -(LKRTCVideoSource * _Nonnull)videoSourceForScreenCast:(BOOL)forScreenCast;
		[Export ("videoSourceForScreenCast:")]
		LKRTCVideoSource VideoSourceForScreenCast (bool forScreenCast);

		// -(LKRTCVideoTrack * _Nonnull)videoTrackWithSource:(LKRTCVideoSource * _Nonnull)source trackId:(NSString * _Nonnull)trackId;
		[Export ("videoTrackWithSource:trackId:")]
		LKRTCVideoTrack VideoTrackWithSource (LKRTCVideoSource source, string trackId);

		// -(LKRTCMediaStream * _Nonnull)mediaStreamWithStreamId:(NSString * _Nonnull)streamId;
		[Export ("mediaStreamWithStreamId:")]
		LKRTCMediaStream MediaStreamWithStreamId (string streamId);

		// -(LKRTCPeerConnection * _Nullable)peerConnectionWithConfiguration:(LKRTCConfiguration * _Nonnull)configuration constraints:(LKRTCMediaConstraints * _Nonnull)constraints delegate:(id<LKRTCPeerConnectionDelegate> _Nullable)delegate;
		[Export ("peerConnectionWithConfiguration:constraints:delegate:")]
		[return: NullAllowed]
		LKRTCPeerConnection PeerConnectionWithConfiguration (LKRTCConfiguration configuration, LKRTCMediaConstraints constraints, [NullAllowed] ILKRTCPeerConnectionDelegate @delegate);

		// -(LKRTCPeerConnection * _Nullable)peerConnectionWithConfiguration:(LKRTCConfiguration * _Nonnull)configuration constraints:(LKRTCMediaConstraints * _Nonnull)constraints certificateVerifier:(id<LKRTCSSLCertificateVerifier> _Nonnull)certificateVerifier delegate:(id<LKRTCPeerConnectionDelegate> _Nullable)delegate;
		[Export ("peerConnectionWithConfiguration:constraints:certificateVerifier:delegate:")]
		[return: NullAllowed]
		LKRTCPeerConnection PeerConnectionWithConfiguration (LKRTCConfiguration configuration, LKRTCMediaConstraints constraints, LKRTCSSLCertificateVerifier certificateVerifier, [NullAllowed] ILKRTCPeerConnectionDelegate @delegate);

		// -(void)setOptions:(LKRTCPeerConnectionFactoryOptions * _Nonnull)options;
		[Export ("setOptions:")]
		void SetOptions (LKRTCPeerConnectionFactoryOptions options);

		// -(BOOL)startAecDumpWithFilePath:(NSString * _Nonnull)filePath maxSizeInBytes:(int64_t)maxSizeInBytes;
		[Export ("startAecDumpWithFilePath:maxSizeInBytes:")]
		bool StartAecDumpWithFilePath (string filePath, long maxSizeInBytes);

		// -(void)stopAecDump;
		[Export ("stopAecDump")]
		void StopAecDump ();
	}

	// @interface LKRTCPeerConnectionFactoryOptions : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCPeerConnectionFactoryOptions
	{
		// @property (assign, nonatomic) BOOL disableEncryption;
		[Export ("disableEncryption")]
		bool DisableEncryption { get; set; }

		// @property (assign, nonatomic) BOOL disableNetworkMonitor;
		[Export ("disableNetworkMonitor")]
		bool DisableNetworkMonitor { get; set; }

		// @property (assign, nonatomic) BOOL ignoreLoopbackNetworkAdapter;
		[Export ("ignoreLoopbackNetworkAdapter")]
		bool IgnoreLoopbackNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreVPNNetworkAdapter;
		[Export ("ignoreVPNNetworkAdapter")]
		bool IgnoreVPNNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreCellularNetworkAdapter;
		[Export ("ignoreCellularNetworkAdapter")]
		bool IgnoreCellularNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreWiFiNetworkAdapter;
		[Export ("ignoreWiFiNetworkAdapter")]
		bool IgnoreWiFiNetworkAdapter { get; set; }

		// @property (assign, nonatomic) BOOL ignoreEthernetNetworkAdapter;
		[Export ("ignoreEthernetNetworkAdapter")]
		bool IgnoreEthernetNetworkAdapter { get; set; }
	}

	// @interface LKRTCRtcpParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCRtcpParameters
	{
		// @property (readonly, copy, nonatomic) NSString * _Nonnull cname;
		[Export ("cname")]
		string Cname { get; }

		// @property (assign, nonatomic) BOOL isReducedSize;
		[Export ("isReducedSize")]
		bool IsReducedSize { get; set; }
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern const NSString *const _Nonnull kLKRTCRtxCodecName;
		[Field ("kLKRTCRtxCodecName", "__Internal")]
		NSString kLKRTCRtxCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCRedCodecName;
		[Field ("kLKRTCRedCodecName", "__Internal")]
		NSString kLKRTCRedCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCUlpfecCodecName;
		[Field ("kLKRTCUlpfecCodecName", "__Internal")]
		NSString kLKRTCUlpfecCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCFlexfecCodecName;
		[Field ("kLKRTCFlexfecCodecName", "__Internal")]
		NSString kLKRTCFlexfecCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCOpusCodecName;
		[Field ("kLKRTCOpusCodecName", "__Internal")]
		NSString kLKRTCOpusCodecName { get; }
		
		/*
		// extern const NSString *const _Nonnull kLKRTCIsacCodecName;
		[Field ("kLKRTCIsacCodecName", "__Internal")]
		NSString kLKRTCIsacCodecName { get; }
		*/
		
		// extern const NSString *const _Nonnull kLKRTCL16CodecName;
		[Field ("kLKRTCL16CodecName", "__Internal")]
		NSString kLKRTCL16CodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCG722CodecName;
		[Field ("kLKRTCG722CodecName", "__Internal")]
		NSString kLKRTCG722CodecName { get; }

		/*
		// extern const NSString *const _Nonnull kLKRTCIlbcCodecName;
		[Field ("kLKRTCIlbcCodecName", "__Internal")]
		NSString kLKRTCIlbcCodecName { get; }
		*/

		// extern const NSString *const _Nonnull kLKRTCPcmuCodecName;
		[Field ("kLKRTCPcmuCodecName", "__Internal")]
		NSString kLKRTCPcmuCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCPcmaCodecName;
		[Field ("kLKRTCPcmaCodecName", "__Internal")]
		NSString kLKRTCPcmaCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCDtmfCodecName;
		[Field ("kLKRTCDtmfCodecName", "__Internal")]
		NSString kLKRTCDtmfCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCComfortNoiseCodecName;
		[Field ("kLKRTCComfortNoiseCodecName", "__Internal")]
		NSString kLKRTCComfortNoiseCodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCVp8CodecName;
		[Field ("kLKRTCVp8CodecName", "__Internal")]
		NSString kLKRTCVp8CodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCVp9CodecName;
		[Field ("kLKRTCVp9CodecName", "__Internal")]
		NSString kLKRTCVp9CodecName { get; }

		// extern const NSString *const _Nonnull kLKRTCH264CodecName;
		[Field ("kLKRTCH264CodecName", "__Internal")]
		NSString kLKRTCH264CodecName { get; }
	}

	// @interface LKRTCRtpCodecParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpCodecParameters
	{
		// @property (assign, nonatomic) int payloadType;
		[Export ("payloadType")]
		int PayloadType { get; set; }

		// @property (readonly, nonatomic) NSString * _Nonnull name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull kind;
		[Export ("kind")]
		string Kind { get; }

		// @property (readonly, nonatomic) NSNumber * _Nullable clockRate;
		[NullAllowed, Export ("clockRate")]
		NSNumber ClockRate { get; }

		// @property (readonly, nonatomic) NSNumber * _Nullable numChannels;
		[NullAllowed, Export ("numChannels")]
		NSNumber NumChannels { get; }

		// @property (readonly, nonatomic) NSDictionary * _Nonnull parameters;
		[Export ("parameters")]
		NSDictionary Parameters { get; }
	}

	// @interface LKRTCRtpEncodingParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpEncodingParameters
	{
		// @property (copy, nonatomic) NSString * _Nullable rid;
		[NullAllowed, Export ("rid")]
		string Rid { get; set; }

		// @property (assign, nonatomic) BOOL isActive;
		[Export ("isActive")]
		bool IsActive { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable maxBitrateBps;
		[NullAllowed, Export ("maxBitrateBps", ArgumentSemantic.Copy)]
		NSNumber MaxBitrateBps { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable minBitrateBps;
		[NullAllowed, Export ("minBitrateBps", ArgumentSemantic.Copy)]
		NSNumber MinBitrateBps { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable maxFramerate;
		[NullAllowed, Export ("maxFramerate", ArgumentSemantic.Copy)]
		NSNumber MaxFramerate { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable numTemporalLayers;
		[NullAllowed, Export ("numTemporalLayers", ArgumentSemantic.Copy)]
		NSNumber NumTemporalLayers { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable scaleResolutionDownBy;
		[NullAllowed, Export ("scaleResolutionDownBy", ArgumentSemantic.Copy)]
		NSNumber ScaleResolutionDownBy { get; set; }

		// @property (readonly, nonatomic) NSNumber * _Nullable ssrc;
		[NullAllowed, Export ("ssrc")]
		NSNumber Ssrc { get; }

		// @property (assign, nonatomic) double bitratePriority;
		[Export ("bitratePriority")]
		double BitratePriority { get; set; }

		// @property (assign, nonatomic) LKRTCPriority networkPriority;
		[Export ("networkPriority", ArgumentSemantic.Assign)]
		LKRTCPriority NetworkPriority { get; set; }

		// @property (assign, nonatomic) BOOL adaptiveAudioPacketTime;
		[Export ("adaptiveAudioPacketTime")]
		bool AdaptiveAudioPacketTime { get; set; }
	}

	// @interface LKRTCRtpHeaderExtension : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpHeaderExtension
	{
		// @property (readonly, copy, nonatomic) NSString * _Nonnull uri;
		[Export ("uri")]
		string Uri { get; }

		// @property (readonly, nonatomic) int id;
		[Export ("id")]
		int Id { get; }

		// @property (readonly, getter = isEncrypted, nonatomic) BOOL encrypted;
		[Export ("encrypted")]
		bool Encrypted { [Bind ("isEncrypted")] get; }
	}

	// @interface LKRTCRtpParameters : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpParameters
	{
		// @property (copy, nonatomic) NSString * _Nonnull transactionId;
		[Export ("transactionId")]
		string TransactionId { get; set; }

		// @property (readonly, copy, nonatomic) LKRTCRtcpParameters * _Nonnull rtcp;
		[Export ("rtcp", ArgumentSemantic.Copy)]
		LKRTCRtcpParameters Rtcp { get; }

		// @property (readonly, copy, nonatomic) NSArray<LKRTCRtpHeaderExtension *> * _Nonnull headerExtensions;
		[Export ("headerExtensions", ArgumentSemantic.Copy)]
		LKRTCRtpHeaderExtension[] HeaderExtensions { get; }

		// @property (copy, nonatomic) NSArray<LKRTCRtpEncodingParameters *> * _Nonnull encodings;
		[Export ("encodings", ArgumentSemantic.Copy)]
		LKRTCRtpEncodingParameters[] Encodings { get; set; }

		// @property (copy, nonatomic) NSArray<LKRTCRtpCodecParameters *> * _Nonnull codecs;
		[Export ("codecs", ArgumentSemantic.Copy)]
		LKRTCRtpCodecParameters[] Codecs { get; set; }

		// @property (copy, nonatomic) NSNumber * _Nullable degradationPreference;
		[NullAllowed, Export ("degradationPreference", ArgumentSemantic.Copy)]
		NSNumber DegradationPreference { get; set; }
	}

	// @protocol LKRTCRtpReceiverDelegate <NSObject>
	[Protocol, Model /****(AutoGeneratedName = true)****/]
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpReceiverDelegate
	{
		// @required -(void)rtpReceiver:(LKRTCRtpReceiver * _Nonnull)rtpReceiver didReceiveFirstPacketForMediaType:(LKRTCRtpMediaType)mediaType;
		[Abstract]
		[Export ("rtpReceiver:didReceiveFirstPacketForMediaType:")]
		void DidReceiveFirstPacketForMediaType (LKRTCRtpReceiver rtpReceiver, LKRTCRtpMediaType mediaType);
	}

	// @protocol LKRTCRtpReceiver <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpReceiver
    {
		// @required @property (readonly, nonatomic) NSString * _Nonnull receiverId;
		[Abstract]
		[Export ("receiverId")]
		string ReceiverId { get; }

		// @required @property (readonly, nonatomic) LKRTCRtpParameters * _Nonnull parameters;
		[Abstract]
		[Export ("parameters")]
		LKRTCRtpParameters Parameters { get; }

		// @required @property (readonly, nonatomic) LKRTCMediaStreamTrack * _Nullable track;
		[Abstract]
		[NullAllowed, Export ("track")]
		LKRTCMediaStreamTrack Track { get; }

		[Wrap ("WeakDelegate"), Abstract]
		[NullAllowed]
		ILKRTCRtpReceiverDelegate Delegate { get; set; }

		// @required @property (nonatomic, weak) id<LKRTCRtpReceiverDelegate> _Nullable delegate;
		[Abstract]
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }
    }

    // @interface LKRTCRtpReceiver : NSObject <LKRTCRtpReceiver>
    ////[BaseType (typeof(NSObject))]
    ////[DisableDefaultCtor]
    ////interface LKRTCRtpReceiver : ILKRTCRtpReceiver
    ////{
    ////}

    // @protocol LKRTCDtmfSender <NSObject>
    /*
    Check whether adding [Model] to this declaration is appropriate.
    [Model] is used to generate a C# class that implements this protocol,
    and might be useful for protocols that consumers are supposed to implement,
    since consumers can subclass the generated class instead of implementing
    the generated interface. If consumers are not supposed to implement this
    protocol, then [Model] is redundant and will generate code that will never
    be used.
    */
    [Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCDtmfSender
	{
		// @required @property (readonly, nonatomic) BOOL canInsertDtmf;
		[Abstract]
		[Export ("canInsertDtmf")]
		bool CanInsertDtmf { get; }

		// @required -(BOOL)insertDtmf:(NSString * _Nonnull)tones duration:(NSTimeInterval)duration interToneGap:(NSTimeInterval)interToneGap;
		[Abstract]
		[Export ("insertDtmf:duration:interToneGap:")]
		bool InsertDtmf(string tones, double duration, double interToneGap);

		// @required -(NSString * _Nonnull)remainingTones;
		[Abstract]
		[Export ("remainingTones")]
		////[Verify (MethodToProperty)]
		string RemainingTones { get; }

		// @required -(NSTimeInterval)duration;
		[Abstract]
		[Export ("duration")]
		////[Verify (MethodToProperty)]
		double Duration { get; }

		// @required -(NSTimeInterval)interToneGap;
		[Abstract]
		[Export ("interToneGap")]
		////[Verify (MethodToProperty)]
		double InterToneGap { get; }
	}

	// @protocol LKRTCRtpSender <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpSender
    {
		// @required @property (readonly, nonatomic) NSString * _Nonnull senderId;
		[Abstract]
		[Export ("senderId")]
		string SenderId { get; }

		// @required @property (copy, nonatomic) LKRTCRtpParameters * _Nonnull parameters;
		[Abstract]
		[Export ("parameters", ArgumentSemantic.Copy)]
		LKRTCRtpParameters Parameters { get; set; }

		// @required @property (copy, nonatomic) LKRTCMediaStreamTrack * _Nullable track;
		[Abstract]
		[NullAllowed, Export ("track", ArgumentSemantic.Copy)]
		LKRTCMediaStreamTrack Track { get; set; }

		// @required @property (copy, nonatomic) NSArray<NSString *> * _Nonnull streamIds;
		[Abstract]
		[Export ("streamIds", ArgumentSemantic.Copy)]
		string[] StreamIds { get; set; }

		// @required @property (readonly, nonatomic) id<LKRTCDtmfSender> _Nullable dtmfSender;
		[Abstract]
		[NullAllowed, Export ("dtmfSender")]
		LKRTCDtmfSender DtmfSender { get; }
	}

    // @interface LKRTCRtpSender : NSObject <LKRTCRtpSender>
    ////[BaseType (typeof(NSObject))]
    ////[DisableDefaultCtor]
    ////interface LKRTCRtpSender : ILKRTCRtpSender
    ////{
    ////}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		/*
		// extern NSString *const _Nonnull kLKRTCRtpTransceiverErrorDomain;
		[Field ("kLKRTCRtpTransceiverErrorDomain", "__Internal")]
		NSString kLKRTCRtpTransceiverErrorDomain { get; }
		*/
	}

	// @interface LKRTCRtpTransceiverInit : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpTransceiverInit
	{
		// @property (nonatomic) LKRTCRtpTransceiverDirection direction;
		[Export ("direction", ArgumentSemantic.Assign)]
		LKRTCRtpTransceiverDirection Direction { get; set; }

		// @property (nonatomic) NSArray<NSString *> * _Nonnull streamIds;
		[Export ("streamIds", ArgumentSemantic.Assign)]
		string[] StreamIds { get; set; }

		// @property (nonatomic) NSArray<LKRTCRtpEncodingParameters *> * _Nonnull sendEncodings;
		[Export ("sendEncodings", ArgumentSemantic.Assign)]
		LKRTCRtpEncodingParameters[] SendEncodings { get; set; }
	}

	// @protocol LKRTCRtpTransceiver <NSObject>
	/*
  Check whether adding [Model] to this declaration is appropriate.
  [Model] is used to generate a C# class that implements this protocol,
  and might be useful for protocols that consumers are supposed to implement,
  since consumers can subclass the generated class instead of implementing
  the generated interface. If consumers are not supposed to implement this
  protocol, then [Model] is redundant and will generate code that will never
  be used.
*/[Protocol]
	[BaseType (typeof(NSObject))]
	interface LKRTCRtpTransceiver
    {
		// @required @property (readonly, nonatomic) LKRTCRtpMediaType mediaType;
		[Abstract]
		[Export ("mediaType")]
		LKRTCRtpMediaType MediaType { get; }

		// @required @property (readonly, nonatomic) NSString * _Nonnull mid;
		[Abstract]
		[Export ("mid")]
		string Mid { get; }

		// @required @property (readonly, nonatomic) LKRTCRtpSender * _Nonnull sender;
		[Abstract]
		[Export ("sender")]
		LKRTCRtpSender Sender { get; }

		// @required @property (readonly, nonatomic) LKRTCRtpReceiver * _Nonnull receiver;
		[Abstract]
		[Export ("receiver")]
		LKRTCRtpReceiver Receiver { get; }

		// @required @property (readonly, nonatomic) BOOL isStopped;
		[Abstract]
		[Export ("isStopped")]
		bool IsStopped { get; }

		// @required @property (readonly, nonatomic) LKRTCRtpTransceiverDirection direction;
		[Abstract]
		[Export ("direction")]
		LKRTCRtpTransceiverDirection Direction { get; }

		// @required -(BOOL)currentDirection:(LKRTCRtpTransceiverDirection * _Nonnull)currentDirectionOut;
		[Abstract]
		[Export ("currentDirection:")]
		/****unsafe****/ bool CurrentDirection (/****LKRTCRtpTransceiverDirection* currentDirectionOut****/ref LKRTCRtpTransceiverDirection currentDirectionOut);

		// @required -(void)stopInternal;
		[Abstract]
		[Export ("stopInternal")]
		void StopInternal ();

		// @required -(void)setDirection:(LKRTCRtpTransceiverDirection)direction error:(NSError * _Nullable * _Nullable)error;
		[Abstract]
		[Export ("setDirection:error:")]
		void SetDirection (LKRTCRtpTransceiverDirection direction, [NullAllowed] out NSError error);
	}

	// @interface LKRTCRtpTransceiver : NSObject <LKRTCRtpTransceiver>
	////[BaseType (typeof(NSObject))]
	////[DisableDefaultCtor]
	////interface LKRTCRtpTransceiver : ILKRTCRtpTransceiver
	////{
	////}

	// @interface LKRTCSessionDescription : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCSessionDescription
	{
		// @property (readonly, nonatomic) LKRTCSdpType type;
		[Export ("type")]
		LKRTCSdpType Type { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull sdp;
		[Export ("sdp")]
		string Sdp { get; }

		// -(instancetype _Nonnull)initWithType:(LKRTCSdpType)type sdp:(NSString * _Nonnull)sdp __attribute__((objc_designated_initializer));
		[Export ("initWithType:sdp:")]
		[DesignatedInitializer]
		NativeHandle Constructor (LKRTCSdpType type, string sdp);

		// +(NSString * _Nonnull)stringForType:(LKRTCSdpType)type;
		[Static]
		[Export ("stringForType:")]
		string StringForType (LKRTCSdpType type);

		// +(LKRTCSdpType)typeForString:(NSString * _Nonnull)string;
		[Static]
		[Export ("typeForString:")]
		LKRTCSdpType TypeForString (string @string);
	}

	// @interface LKRTCStatisticsReport : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCStatisticsReport
	{
		// @property (readonly, nonatomic) CFTimeInterval timestamp_us;
		[Export ("timestamp_us")]
		double Timestamp_us { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,LKRTCStatistics *> * _Nonnull statistics;
		[Export ("statistics")]
		NSDictionary<NSString, LKRTCStatistics> Statistics { get; }
	}

	// @interface LKRTCStatistics : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface LKRTCStatistics
		: INativeObject
	{
		// @property (readonly, nonatomic) NSString * _Nonnull id;
		[Export ("id")]
		string Id { get; }

		// @property (readonly, nonatomic) CFTimeInterval timestamp_us;
		[Export ("timestamp_us")]
		double Timestamp_us { get; }

		// @property (readonly, nonatomic) NSString * _Nonnull type;
		[Export ("type")]
		string Type { get; }

		// @property (readonly, nonatomic) NSDictionary<NSString *,NSObject *> * _Nonnull values;
		[Export ("values")]
		NSDictionary<NSString, NSObject> Values { get; }
	}

    // @interface LKRTCVideoSource : LKRTCMediaSource <LKRTCVideoCapturerDelegate>
[Protocol]
    [BaseType (typeof(LKRTCMediaSource))]
	[DisableDefaultCtor]
	interface LKRTCVideoSource : /****I***/LKRTCVideoCapturerDelegate
	{
		// -(void)adaptOutputFormatToWidth:(int)width height:(int)height fps:(int)fps;
		[Export ("adaptOutputFormatToWidth:height:fps:")]
		void AdaptOutputFormatToWidth (int width, int height, int fps);
	}

	// @interface LKRTCVideoTrack : LKRTCMediaStreamTrack
	[BaseType (typeof(LKRTCMediaStreamTrack))]
	[DisableDefaultCtor]
	interface LKRTCVideoTrack
	{
		// @property (readonly, nonatomic) LKRTCVideoSource * _Nonnull source;
		[Export ("source")]
		LKRTCVideoSource Source { get; }

		// -(void)addRenderer:(id<LKRTCVideoRenderer> _Nonnull)renderer;
		[Export ("addRenderer:")]
		void AddRenderer (ILKRTCVideoRenderer renderer);

		// -(void)removeRenderer:(id<LKRTCVideoRenderer> _Nonnull)renderer;
		[Export ("removeRenderer:")]
		void RemoveRenderer (ILKRTCVideoRenderer renderer);
	}

    ////[Static]
    ////[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
	{
		// extern NSString *const kLKRTCVideoCodecVp8Name;
		[Field ("kLKRTCVideoCodecVp8Name", "__Internal")]
		NSString kLKRTCVideoCodecVp8Name { get; }

		// extern NSString *const kLKRTCVideoCodecVp9Name;
		[Field ("kLKRTCVideoCodecVp9Name", "__Internal")]
		NSString kLKRTCVideoCodecVp9Name { get; }

		// extern NSString *const kLKRTCVideoCodecAv1Name;
		[Field ("kLKRTCVideoCodecAv1Name", "__Internal")]
		NSString kLKRTCVideoCodecAv1Name { get; }
	}

	// @interface LKRTCVideoDecoderVP8 : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoderVP8
	{
		// +(id<LKRTCVideoDecoder>)vp8Decoder;
		[Static]
		[Export ("vp8Decoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoDecoder Vp8Decoder { get; }
	}

	// @interface LKRTCVideoDecoderVP9 : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoderVP9
	{
		// +(id<LKRTCVideoDecoder>)vp9Decoder;
		[Static]
		[Export ("vp9Decoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoDecoder Vp9Decoder { get; }

		// +(_Bool)isSupported;
		[Static]
		[Export ("isSupported")]
		////[Verify (MethodToProperty)]
		bool IsSupported { get; }
	}

	// @interface LKRTCVideoDecoderAV1 : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoDecoderAV1
	{
		// +(id<LKRTCVideoDecoder>)av1Decoder;
		[Static]
		[Export ("av1Decoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoDecoder Av1Decoder { get; }
	}

	// @interface LKRTCVideoEncoderVP8 : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderVP8
	{
		// +(id<LKRTCVideoEncoder>)vp8Encoder;
		[Static]
		[Export ("vp8Encoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoEncoder Vp8Encoder { get; }
	}

	// @interface LKRTCVideoEncoderVP9 : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderVP9
	{
		// +(id<LKRTCVideoEncoder>)vp9Encoder;
		[Static]
		[Export ("vp9Encoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoEncoder Vp9Encoder { get; }

		// +(_Bool)isSupported;
		[Static]
		[Export ("isSupported")]
		////[Verify (MethodToProperty)]
		bool IsSupported { get; }
	}

	// @interface LKRTCVideoEncoderAV1 : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCVideoEncoderAV1
	{
		// +(id<LKRTCVideoEncoder>)av1Encoder;
		[Static]
		[Export ("av1Encoder")]
		////[Verify (MethodToProperty)]
		LKRTCVideoEncoder Av1Encoder { get; }

		// +(_Bool)isSupported;
		[Static]
		[Export ("isSupported")]
		////[Verify (MethodToProperty)]
		bool IsSupported { get; }
    }

    // @interface LKRTCI420Buffer : NSObject <LKRTCI420Buffer>
    [BaseType (typeof(NSObject))]
    interface LKRTCI420Buffer : ILKRTCI420Buffer
    {
    }

    // @interface LKRTCMutableI420Buffer : LKRTCI420Buffer <LKRTCMutableI420Buffer>
    [BaseType (typeof(LKRTCI420Buffer))]
	interface LKRTCMutableI420Buffer : ILKRTCMutableI420Buffer
	{
	}

	// typedef void (^LKRTCCallbackLoggerMessageHandler)(NSString * _Nonnull);
	delegate void LKRTCCallbackLoggerMessageHandler (string arg0);

	// typedef void (^LKRTCCallbackLoggerMessageAndSeverityHandler)(NSString * _Nonnull, LKRTCLoggingSeverity);
	delegate void LKRTCCallbackLoggerMessageAndSeverityHandler (string arg0, LKRTCLoggingSeverity arg1);

	// @interface LKRTCCallbackLogger : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCCallbackLogger
	{
		// @property (assign, nonatomic) LKRTCLoggingSeverity severity;
		[Export ("severity", ArgumentSemantic.Assign)]
		LKRTCLoggingSeverity Severity { get; set; }

		// -(void)start:(LKRTCCallbackLoggerMessageHandler _Nullable)handler;
		[Export ("start:")]
		void Start ([NullAllowed] LKRTCCallbackLoggerMessageHandler handler);

		// -(void)startWithMessageAndSeverityHandler:(LKRTCCallbackLoggerMessageAndSeverityHandler _Nullable)handler;
		[Export ("startWithMessageAndSeverityHandler:")]
		void StartWithMessageAndSeverityHandler ([NullAllowed] LKRTCCallbackLoggerMessageAndSeverityHandler handler);

		// -(void)stop;
		[Export ("stop")]
		void Stop ();
	}

	// @interface LKRTCFileLogger : NSObject
	[BaseType (typeof(NSObject))]
	interface LKRTCFileLogger
	{
		// @property (assign, nonatomic) LKRTCFileLoggerSeverity severity;
		[Export ("severity", ArgumentSemantic.Assign)]
		LKRTCFileLoggerSeverity Severity { get; set; }

		// @property (readonly, nonatomic) LKRTCFileLoggerRotationType rotationType;
		[Export ("rotationType")]
		LKRTCFileLoggerRotationType RotationType { get; }

		// @property (assign, nonatomic) BOOL shouldDisableBuffering;
		[Export ("shouldDisableBuffering")]
		bool ShouldDisableBuffering { get; set; }

		// -(instancetype _Nonnull)initWithDirPath:(NSString * _Nonnull)dirPath maxFileSize:(NSUInteger)maxFileSize;
		[Export ("initWithDirPath:maxFileSize:")]
		NativeHandle Constructor (string dirPath, nuint maxFileSize);

		// -(instancetype _Nonnull)initWithDirPath:(NSString * _Nonnull)dirPath maxFileSize:(NSUInteger)maxFileSize rotationType:(LKRTCFileLoggerRotationType)rotationType __attribute__((objc_designated_initializer));
		[Export ("initWithDirPath:maxFileSize:rotationType:")]
		[DesignatedInitializer]
		NativeHandle Constructor (string dirPath, nuint maxFileSize, LKRTCFileLoggerRotationType rotationType);

		// -(void)start;
		[Export ("start")]
		void Start ();

		// -(void)stop;
		[Export ("stop")]
		void Stop ();

		// -(NSData * _Nullable)logData;
		[NullAllowed, Export ("logData")]
		////[Verify (MethodToProperty)]
		NSData LogData { get; }
	}
}
