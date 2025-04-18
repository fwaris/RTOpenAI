using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Audio;
using Concentus.Enums;
using Windows.Media.Capture;
using Concentus;
using WinRT;
using System.Diagnostics;
namespace Opus.Maui
{
    public class FromMic: IDisposable
    {        
        private AudioDeviceInputNode? _deviceInputNode;
        private AudioFrameOutputNode? _frameOutputNode;
        private IOpusEncoder? _opusEncoder;
        // Choose your PCM configuration – here we use mono, 16-bit at 48000 Hz (which Opus expects).

        /// <summary>
        /// Initializes the AudioGraph and the Opus encoder.
        /// </summary>
        public async Task InitializeAsync(AudioGraph audioGraph)
        {
            // Create a device input node for the microphone.
            var deviceInputResult = await audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Communications);
            if (deviceInputResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                throw new Exception("Audio Device Input creation failed: " + deviceInputResult.Status);
            }
            _deviceInputNode = deviceInputResult.DeviceInputNode;

            // Create a frame output node to get audio frames.
            _frameOutputNode = audioGraph.CreateFrameOutputNode();
            // Connect the microphone input to the frame output.
            _deviceInputNode.AddOutgoingConnection(_frameOutputNode);

            // Initialize the Concentus Opus encoder.
            // The encoder expects PCM at 48000 Hz. You can adjust channels as needed.
            _opusEncoder = OpusCodecFactory.CreateEncoder(Graph.SampleRate, Graph.Channels, OpusApplication.OPUS_APPLICATION_VOIP);
            _frameOutputNode.Stop();
        }

        public void Start(AudioGraph audioGraph, Action<Tuple<UInt32,byte[]>> callback)
        {
            audioGraph.QuantumStarted += (sender, args) =>
            {
                // Call the GetEncoded() method to retrieve the encoded data.
                Tuple<UInt32,byte[]> ret = GetEncoded();
                if (ret.Item1 > 0u)
                {
                    callback(ret);
                }
                // Pass the encoded// data to the callback.
            };
            _frameOutputNode.Start();
        }

        private Tuple<UInt32,byte[]> GetEncoded()
        {
            // Retrieve an audio frame from the frame output node.
            using (AudioFrame frame = _frameOutputNode.GetFrame())
            // Lock the frame buffer for reading.
            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
            using (var reference = buffer.CreateReference())
            {
                // Use COM interop to get a pointer to the raw buffer.
                unsafe
                {
                    byte* dataInBytes;                   
                    uint capacity;
                    var buffRef = reference.As<IMemoryBufferByteAccess>();
                    buffRef.GetBuffer(out dataInBytes, out capacity);
                    if (capacity > 0)
                    {
                        float * pcmFloats = (float*)dataInBytes;
                        var sampleCount =(int) capacity / sizeof(float);
                        Span<float> span = MemoryMarshal.CreateSpan<float>(ref pcmFloats[0], sampleCount);                  
                        // Copy the raw data to a managed byte array.
                        // Allocate an output buffer for the encoded Opus data.
                        byte[] encodedBuffer = new byte[4000];  // adjust size as needed

                        // Encode the PCM samples into an Opus frame.
                        int encodedLength = _opusEncoder.Encode(span, sampleCount, encodedBuffer, encodedBuffer.Length);
                        if (encodedLength < 0)
                        {
                            throw new Exception("unable to encode");
                        }
                        else
                        {
                           // Debug.WriteLine($"Encode: {encodedLength}");
                        }

                        UInt32 duration = (uint)(Graph.SampleRate / sampleCount);
                        return Tuple.Create(duration, encodedBuffer.AsSpan().Slice(0,sampleCount).ToArray());
                    }
                    else
                    {
                        return Tuple.Create(0U, new byte[] { });
                    }
                }
            }
        }

        static short FloatToInt16(float value)
        {
            float f = value * short.MaxValue;
            if (f > short.MaxValue)
            {
                f = short.MaxValue;
            }

            if (f < short.MinValue)
            {
                f = short.MinValue;
            }

            return (short)f;
        }

        public void Dispose()
        {
            try
            {   if (_frameOutputNode != null) { 
                    _frameOutputNode?.Dispose();
                    _frameOutputNode = null;
                    _opusEncoder?.Dispose();                   
                    _opusEncoder = null;
                }

            }
            catch { }
        }
    }

    // COM interface to access the underlying byte buffer.
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

}
