using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Webrtc
{
    public abstract partial class WrappedNativeVideoEncoder : global::Java.Lang.Object, global::Org.Webrtc.IVideoEncoder
    {
        public IVideoEncoder.ScalingSettings GetScalingSettings()
        {
            throw new NotImplementedException();
        }
    }

    public partial class TextureBufferImpl : global::Java.Lang.Object, global::Org.Webrtc.VideoFrame.ITextureBuffer
    {
        VideoFrame.ITextureBuffer.Type VideoFrame.ITextureBuffer.GetType()
        {
            throw new NotImplementedException();
        }
    }

    public static class RTCJUtils
    {
        public static Java.Nio.ByteBuffer Wrap(byte[] data) => Java.Nio.ByteBuffer.Wrap(data);
    }
}
