using System.Globalization;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public interface ISpeechSynthesizerFactory
    {
        public Task<MemoryStream> GetStreamAsync(string text, SpeechAudioFormatInfo speechAudioFormatConfi);
    }
    public class SpeechSynthesizerFactory : ISpeechSynthesizerFactory
    {
        private SpeechSynthesizer SpeechSynthesizer { get; }

        public SpeechSynthesizerFactory()
        {

        }

        public async Task<MemoryStream> GetStreamAsync(string text, SpeechAudioFormatInfo speechAudioFormatConfig)
        {
            // Slow down the voice: https://docs.microsoft.com/en-us/dotnet/api/system.speech.synthesis.promptbuilder?view=netframework-4.8
            var prompt = new PromptBuilder { Culture = CultureInfo.CreateSpecificCulture("en-US") };
            prompt.StartVoice(prompt.Culture);
            prompt.StartSentence();
            prompt.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Reduced, Rate = PromptRate.Slow });
            prompt.AppendText(text);
            prompt.EndStyle();
            prompt.EndSentence();
            prompt.EndVoice();

            // Setup speech synthesizer to stream
            using var speechSynthesizerStream = new MemoryStream();
            SpeechSynthesizer.SetOutputToAudioStream(
                audioDestination: speechSynthesizerStream,
                formatInfo: speechAudioFormatConfig);

            // Synthesize text to a wav stream
            await Task.Run(() => SpeechSynthesizer.Speak(prompt));
            speechSynthesizerStream.Position = 0;
            return speechSynthesizerStream;

        }
    }
}
