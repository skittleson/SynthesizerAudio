using Microsoft.CognitiveServices.Speech;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public class AzureSpeechSynthesizer : ISpeechSynthesizer
    {
        private readonly SpeechConfig _config;
        public AzureSpeechSynthesizer(string key, string region) {
            _config = SpeechConfig.FromSubscription(key, region);
            _config.SpeechSynthesisVoiceName = "en-US-JennyNeural";
        }
        public async Task<MemoryStream> GetStreamAsync(string text, TextToSpeechAudioOptions options = null) {
            _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff48Khz16BitMonoPcm);
            
            // TODO: Convert options to Speech output formats
            var _synthesizer = new SpeechSynthesizer(_config, null);
            var result = await _synthesizer.SpeakTextAsync(text);
            if (result.Reason == ResultReason.Canceled) {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                throw new Exception("Unable to connect to Azure services:" + cancellation.ErrorDetails);
            }
            var source = AudioDataStream.FromResult(result);
            source.SetPosition(0);
            var buffer = new byte[4096];
            var wav = new MemoryStream();
            uint reader;
            while ((reader = source.ReadData(buffer)) != 0) {
                await wav.WriteAsync(buffer.AsMemory(0, (int)reader));
            }
            wav.Position = 0;
            return wav;
        }
        public async Task<string[]> VoicesAsync() => (await new SpeechSynthesizer(_config, null)
                .GetVoicesAsync())
                .Voices.Select(x => x.Name)
                .ToArray();

        /// <summary>
        /// Use <see cref="VoicesAsync"/>
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use VoicesAsync", false)]
        public string[] Voices() => VoicesAsync().GetAwaiter().GetResult();
    }
}
