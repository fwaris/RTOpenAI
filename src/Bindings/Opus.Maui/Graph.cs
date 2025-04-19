using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

namespace Opus.Maui
{
    public class Graph : IDisposable
    {
        AudioGraph? _audioGraph;
        FromMic? _fromMic;
        ToSpeaker? _toSpeaker;
        // PCM configuration – using mono, 16-bit at 48000 Hz (which matches the encoder).
        public const int SampleRate = 48000;
        public const int Channels = 1;

        public async Task InitializeAsync()
        {
            // Create an AudioGraph with default settings.
            var settings = new AudioGraphSettings(AudioRenderCategory.Communications);
            var pcmEncoding = AudioEncodingProperties.CreatePcm((uint)SampleRate, (uint)Channels, 16u);           
            settings.EncodingProperties = pcmEncoding;
            settings.DesiredRenderDeviceAudioProcessing = Windows.Media.AudioProcessing.Raw;
            settings.DesiredSamplesPerQuantum = 48000 / 1000 * 20;
            var createGraphResult = await AudioGraph.CreateAsync(settings);
            if (createGraphResult.Status != AudioGraphCreationStatus.Success)
            {
                throw new Exception("AudioGraph creation failed: " + createGraphResult.Status);
            }
            _fromMic = new FromMic();
            _toSpeaker = new ToSpeaker();
            _audioGraph = createGraphResult.Graph;
            await _fromMic.InitializeAsync(_audioGraph);
            await _toSpeaker.InitializeAsync(_audioGraph);
            _audioGraph.UnrecoverableErrorOccurred += Graph_UnrecoverableErrorOccurred;
        }

        public void DecodeAndPlay(byte[] bytes)
        {
            _toSpeaker?.DecodeAndPlay(bytes);
        }

        public void Start(Action<Tuple<uint, byte[]>> callback)
        {
            _audioGraph?.Start();
            _toSpeaker?.Start();
            _fromMic?.Start(_audioGraph, callback);
        }

        void Graph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args)
        {
            Dispose();

            throw new Exception($"UnrecoverableErrorOccurred error: {args.Error}");
        }


        public void Dispose()
        {
            if (_audioGraph != null) {
                _audioGraph.Stop();                
                _audioGraph.Dispose();
                _audioGraph = null;
                _fromMic?.Dispose();
                _fromMic = null;
                _toSpeaker?.Dispose();
                _toSpeaker = null;
            }
        }
    }
}
