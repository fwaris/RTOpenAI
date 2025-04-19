using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Audio;
using System.Diagnostics;
using Concentus;
using WinRT;
using System.Text;
using Windows.Devices.AllJoyn;
using System.Buffers;
using Windows.Foundation;
using System.Collections;

namespace Opus.Maui
{

    public class DebugTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            Debug.Write(value);
        }

        public override void Write(string? value)
        {
            Debug.Write(value);
        }

        public override void WriteLine(string? value)
        {
            Debug.WriteLine(value);
        }
    }

    public class SamplesBuffer
    {
        private readonly List<Tuple<int, float[]>> samples = [];
        private readonly object _lock = new();

        // Writes PCM data to the buffer
        public void Write(ReadOnlySpan<float> data)
        {
            lock (_lock)
            {
                var sampleSet = ArrayPool<float>.Shared.Rent(data.Length);
                data.CopyTo(sampleSet);
                samples.Add(Tuple.Create(data.Length,sampleSet));
            }
        }

        // Reads available data from the buffer
        public Tuple<int,float[]>? Read()
        {
            lock (_lock)
            {
                if(samples.Count > 0)
                {
                    var (len,data) = samples[0];
                    samples.RemoveAt(0);
                    return Tuple.Create(len,data);
                }
                {
                    return null;
                }
            }
        }
    }


    public class ToSpeaker : IDisposable
    {
        private AudioDeviceOutputNode? _deviceOutputNode;
        private AudioFrameInputNode? _frameInputNode;
        private IOpusDecoder? _opusDecoder;
        private SamplesBuffer? _buffer;
        
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
            _frameInputNode.Stop();

            // Initialize the Concentus Opus decoder.
            _opusDecoder = OpusCodecFactory.CreateDecoder(Graph.SampleRate, Graph.Channels, new DebugTextWriter());
            _buffer = new SamplesBuffer();
            _frameInputNode.QuantumStarted += _frameInputNode_QuantumStarted;
        }

        public void Start() {
            _frameInputNode?.Start();
        }

        unsafe private AudioFrame GenerateAudioData(Tuple<int, float[]> data)
        {        
            int byteLen = data.Item1 * sizeof(float);
            AudioFrame frame = new Windows.Media.AudioFrame((uint)byteLen);

            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                var buffRef = reference.As<IMemoryBufferByteAccess>();
                buffRef.GetBuffer(out dataInBytes, out capacityInBytes);
                dataInFloat = (float*)dataInBytes;
                for(int i=0; i < data.Item1; i++) {
                    dataInFloat[i] = data.Item2[i];
                }
                var nonZeroFloats = data.Item2.Take(data.Item1).Where(x => x != 0.0f).Count();
                ArrayPool<float>.Shared.Return(data.Item2);
                Debug.WriteLine($"written to speaker {data.Item1}; non zero count {nonZeroFloats}");
            }

            return frame;
        }

        private void _frameInputNode_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            try
            {
                Tuple<int,float[]>? data = _buffer.Read();
                if (data != null )
                {
                    sender.AddFrame(GenerateAudioData(data));
                }

            }
            catch (Exception ex) {
                Debug.WriteLine($"unable to write frame {ex.Message}");
            }
        }

        /// <summary>
        /// Decodes the provided encoded data and enqueues the resulting PCM frame for playback.
        /// </summary>
        /// <param name="encodedData">The Opus encoded audio data.</param>
        public void DecodeAndPlay(byte[] encodedData)
        {
            //openai sends tiny audio packets (3 bytes) that can't really be decoded
            //so skip them
            //if (encodedData.Length < 10)
            //{
            //    Debug.WriteLine($"skipping decode of {encodedData.Length} bytes");
            //    return;
            //}
            Debug.WriteLine($"received rtp {encodedData.Length} bytes");

            // Define a maximum frame size (in samples per channel). 
            // Opus frames typically have a maximum of 5760 samples per channel for 120 ms frames at 48000 Hz.
            unsafe
            {
                int maxSamplesPerChannel = 5760;
                Span<float> decodedPcm = stackalloc float[maxSamplesPerChannel * Graph.Channels];
                int decodedSampleCount = _opusDecoder.Decode(encodedData, decodedPcm, maxSamplesPerChannel * Graph.Channels,false);

                if (decodedSampleCount <= 0 || decodedSampleCount == maxSamplesPerChannel)
                {
                    Debug.WriteLine($"skipping as decoded bytes length {decodedSampleCount}, does not look right");
                }
                else
                {
                    Debug.WriteLine($"samples to speaker {decodedSampleCount}");
                     ReadOnlySpan<float> samples = decodedPcm.Slice(0, decodedSampleCount);
                    _buffer.Write(samples);
                }
            }
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
                    _buffer = null;
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Dispose(){ex.Message}"); }
        }
    }
}
