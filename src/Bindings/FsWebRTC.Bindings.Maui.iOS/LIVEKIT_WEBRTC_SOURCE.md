# LiveKit WebRTC Source

- Release tag: `144.7559.09`
- Release URL: <https://github.com/livekit/webrtc-xcframework/releases/tag/144.7559.09>
- Asset URL: <https://github.com/livekit/webrtc-xcframework/releases/download/144.7559.09/LiveKitWebRTC.xcframework.zip>
- SHA256 / SwiftPM checksum: `64da5637fbb171fb0bce7889e9e025ceb3521a1c9010bb5df47df9b54a659c30`
- Framework: `LiveKitWebRTC.xcframework`
- Binding package: `FsWebRTC.Bindings.Maui.iOS`
- Binding namespace: `FsWebRTC.Bindings`

## Supported Slices

Only these slices are intentionally copied into the binding package:

- `ios-arm64`
- `ios-arm64_x86_64-simulator`
- `ios-arm64_x86_64-maccatalyst`

The upstream asset also contains other Apple platform slices. They are intentionally excluded from this first package so the supported surface is limited to iOS device, iOS simulator, and Mac Catalyst.

## Symbol Prefix

LiveKit builds WebRTC with `RTC_OBJC_TYPE_PREFIX` set to `LK` and `RTC_CONSTANT_TYPE_PREFIX` set to `kLK`.

That means Objective-C classes and protocols are exported as `LKRTC*`, such as `LKRTCPeerConnection`, `LKRTCAudioSession`, and `LKRTCConfiguration`. This package keeps that distinction in both Objective-C registration names and C# binding type names, rather than reusing the existing `RTC*` ABI from stasel/Google WebRTC.
