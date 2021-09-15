using NAudio.Wave;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public interface ISpeechSynthesizerFactory
    {
        Task<WaveStream> GetStreamAsync(string text, TextToSpeechAudioOptions options = null);
        Task CopyToAsync(WaveStream source, MemoryStream destination);
        string[] Voices();
    }
    public class SpeechSynthesizerFactory : ISpeechSynthesizerFactory
    {
        private SpeechSynthesizer SpeechSynthesizer { get; }

        public SpeechSynthesizerFactory()
        {
            SpeechSynthesizer = new SpeechSynthesizer();
        }

        public async Task<WaveStream> GetStreamAsync(string text, TextToSpeechAudioOptions options)
        {
            // Slow down the voice: https://docs.microsoft.com/en-us/dotnet/api/system.speech.synthesis.promptbuilder?view=netframework-4.8
            var prompt = new PromptBuilder { Culture = CultureInfo.CreateSpecificCulture("en-US") };
            prompt.StartVoice(prompt.Culture);
            prompt.StartSentence();
            prompt.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Strong, Rate = PromptRate.Slow });
            prompt.AppendText(text);
            prompt.EndStyle();
            prompt.EndSentence();
            prompt.EndVoice();

            if (!string.IsNullOrEmpty(options?.VoiceName))
            {
                var voice = Voices().FirstOrDefault(x => x.Contains(options?.VoiceName, System.StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(voice))
                {
                    SpeechSynthesizer.SelectVoice(voice);
                }
            }
            // Setup speech synthesizer to stream
            var speechSynthesizerStream = new MemoryStream();
            SpeechSynthesizer.SetOutputToWaveStream(speechSynthesizerStream);

            // Synthesize text to a wav stream
            await Task.Run(() => SpeechSynthesizer.Speak(prompt)).ConfigureAwait(false);
            speechSynthesizerStream.Position = 0;
            SpeechSynthesizer.SetOutputToNull();
            return new WaveFileReader(speechSynthesizerStream);
        }

        public async Task CopyToAsync(WaveStream source, MemoryStream destination)
        {
            source.Position = 0;
            var waveFileWriter = new WaveFileWriter(destination, source.WaveFormat);
            var bytes = new byte[source.Length];
            await source.ReadAsync(bytes, 0, bytes.Length);
            await waveFileWriter.WriteAsync(bytes, 0, bytes.Length);
            await waveFileWriter.FlushAsync();
        }

        public string[] Voices()
        {
            return SpeechSynthesizer
                  .GetInstalledVoices()
                  .Where(x => x.Enabled)
                  .Select(x => x.VoiceInfo.Name)
                  .ToArray();
        }
    }
}
