using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Audio;
using System.Diagnostics;
using Concentus;
using WinRT;

namespace Opus.Maui
{
    public class ToSpeaker : IDisposable
    {
        private AudioDeviceOutputNode? _deviceOutputNode;
        private AudioFrameInputNode? _frameInputNode;
        private IOpusDecoder? _opusDecoder;
        // PCM configuration – using mono, 16-bit at 48000 Hz (which matches the encoder).

        /// <summary>
        /// Initializes the AudioGraph and the Opus decoder.
        /// </summary>
        public async Task InitializeAsync(AudioGraph audioGraph)
        {
            // Create a device output node for the speakers.
            var deviceOutputResult = await audioGraph.CreateDeviceOutputNodeAsync();
            if (deviceOutputResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                throw new Exception("Audio Device Output creation failed: " + deviceOutputResult.Status);
            }
            _deviceOutputNode = deviceOutputResult.DeviceOutputNode;

            // Create a frame input node to send PCM audio frames to the graph.
            _frameInputNode = audioGraph.CreateFrameInputNode();
            _frameInputNode.AddOutgoingConnection(_deviceOutputNode);

            // Initialize the Concentus Opus decoder.
            _opusDecoder = OpusCodecFactory.CreateDecoder(Graph.SampleRate, Graph.Channels);
        }

        /// <summary>
        /// Decodes the provided encoded data and enqueues the resulting PCM frame for playback.
        /// </summary>
        /// <param name="encodedData">The Opus encoded audio data.</param>
        public void DecodeAndPlay(byte[] encodedData)
        {
            //openai sends tiny audio packets (3 bytes) that can't really be decoded
            //so skip them
            if (encodedData.Length < 10)
            {
                //Debug.WriteLine($"skipping decode of {encodedData.Length} bytes");
                return;
            }
            // Define a maximum frame size (in samples per channel). 
            // Opus frames typically have a maximum of 5760 samples per channel for 120 ms frames at 48000 Hz.

            int maxSamplesPerChannel = 5760;
            short[] decodedPcm = new short[maxSamplesPerChannel * Graph.Channels];

            // Decode the Opus data into PCM samples.
            int decodedSampleCount = _opusDecoder.Decode(encodedData, decodedPcm, maxSamplesPerChannel, true);
            if (decodedSampleCount < 0)
            {
                throw new Exception("Opus decode failed");
            }
            //Debug.WriteLine($"Decoded {decodedSampleCount} samples");

            // Convert the short PCM samples to a byte array (16-bit PCM -> 2 bytes per sample).
            byte[] pcmData = new byte[decodedSampleCount * Graph.Channels * 2];
            Buffer.BlockCopy(decodedPcm, 0, pcmData, 0, pcmData.Length);

            // Create an AudioFrame with a buffer sized to hold the PCM data.
                using (AudioFrame audioFrame = new AudioFrame((UInt32)pcmData.Length))
                {
                    using (AudioBuffer buffer = audioFrame.LockBuffer(AudioBufferAccessMode.Write))
                    using (var reference = buffer.CreateReference())
                    {
                        // Use COM interop to access the underlying buffer and copy the PCM data.
                        unsafe
                        {
                            byte* destBytes;
                            uint capacity;
                            var buffRef = reference.As<IMemoryBufferByteAccess>();
                            buffRef.GetBuffer(out destBytes, out capacity);
                            Marshal.Copy(pcmData, 0, (IntPtr)destBytes, pcmData.Length);
                        }

                    }
                    _frameInputNode?.AddFrame(audioFrame);
                }

            // Enqueue the decoded PCM frame to be played.

        }

        public void Dispose()
        {
            try
            {
                if (_frameInputNode != null)
                {
                    _frameInputNode.Dispose();
                    _frameInputNode = null;
                    _deviceOutputNode?.Dispose();
                    _deviceOutputNode = null;
                    _opusDecoder?.Dispose();
                    _opusDecoder = null;
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Dispose(){ex.Message}"); }
        }
    }
}
