using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public interface ISpeechSynthesizer {
        Task<MemoryStream> GetStreamAsync(string text, TextToSpeechAudioOptions options = null);
        
        [Obsolete("Use VoicesAsync")]
        string[] Voices();
        Task<string[]> VoicesAsync();
    }
    public class WindowsSpeechSynthesizer : ISpeechSynthesizer {
        private SpeechSynthesizer SpeechSynthesizer { get; }

        public WindowsSpeechSynthesizer() {
            if (!OperatingSystem.IsWindows()) {
                throw new NotSupportedException("Only works with Windows");
            }
            SpeechSynthesizer = new SpeechSynthesizer();
        }
        public async Task<MemoryStream> GetStreamAsync(string text, TextToSpeechAudioOptions options) {
            var voiceSpeed = options.Speed switch {
                SPEECH_SPEED.SLOW => PromptRate.Slow,
                SPEECH_SPEED.MEDIUM => PromptRate.Medium,
                SPEECH_SPEED.FAST => PromptRate.Fast,
                _ => PromptRate.NotSet,
            };
            var prompt = new PromptBuilder { Culture = CultureInfo.CreateSpecificCulture("en-US") };
            prompt.StartVoice(prompt.Culture);
            prompt.StartSentence();
            prompt.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Strong, Rate = voiceSpeed });
            prompt.AppendText(text);
            prompt.EndStyle();
            prompt.EndSentence();
            prompt.EndVoice();
            if (!string.IsNullOrEmpty(options?.VoiceName)) {
                var voice = Voices().FirstOrDefault(x => x.Contains(options?.VoiceName, System.StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(voice)) {
                    SpeechSynthesizer.SelectVoice(voice);
                }
            }

            // Setup speech synthesizer to stream
            var speechSynthesizerStream = new MemoryStream();
            //var bitsPerSample = options.BitsPerSample == 16 ? AudioBitsPerSample.Sixteen : AudioBitsPerSample.Eight;
            //var channels = options.Channels == 1 ? AudioChannel.Mono : AudioChannel.Stereo;
            //SpeechSynthesizer.SetOutputToAudioStream(speechSynthesizerStream, new SpeechAudioFormatInfo(options.SampleRate, bitsPerSample, channels));
            //var synthFormat = new SpeechAudioFormatInfo(EncodingFormat.Pcm, options.SampleRate, options.BitsPerSample, options.Channels, options.BitsPerSample * 1000, 2, null);
            //SpeechSynthesizer.SetOutputToAudioStream(speechSynthesizerStream,synthFormat);
            SpeechSynthesizer.SetOutputToWaveStream(speechSynthesizerStream);

            // Synthesize text to a wav stream
            await Task.Run(() => SpeechSynthesizer.Speak(prompt)).ConfigureAwait(false);
            speechSynthesizerStream.Position = 0;
            SpeechSynthesizer.SetOutputToNull();
            return speechSynthesizerStream;
        }

        public string[] Voices() => SpeechSynthesizer
                  .GetInstalledVoices()
                  .Where(x => x.Enabled)
                  .Select(x => x.VoiceInfo.Name)
                  .ToArray();
        public Task<string[]> VoicesAsync() => Task.FromResult(Voices());
    }
}
