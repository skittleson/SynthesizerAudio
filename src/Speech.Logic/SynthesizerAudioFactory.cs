using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace Speech.Logic
{
    public class SynthesizerAudioFactory
    {
        public enum AUDIO_FORMAT
        {
            WAV = 0,
            MP3 = 2,
            OGG = 4
        }

        public string[] GetVoiceNames()
        {
            return new SpeechSynthesizer()
                .GetInstalledVoices()
                .Select(x => x.VoiceInfo.Name)
                .ToArray();
        }

        public async Task<MemoryStream> TextToSpeechAudioAsync(string text, AUDIO_FORMAT format)
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Setup audio stream to return on response
            var audioStream = new MemoryStream();

            // Speech format: https://docs.microsoft.com/en-us/dotnet/api/system.speech.audioformat?view=netframework-4.8
            var speechAudioFormatConfig = new SpeechAudioFormatInfo(
                samplesPerSecond: 8000,
                bitsPerSample: AudioBitsPerSample.Sixteen,
                channel: AudioChannel.Stereo);

            // NAudio wave format has to be the same as speech audio format
            var waveFormat = new WaveFormat(
                rate: speechAudioFormatConfig.SamplesPerSecond,
                bits: speechAudioFormatConfig.BitsPerSample,
                channels: speechAudioFormatConfig.ChannelCount);

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
            using var speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToAudioStream(
                audioDestination: speechSynthesizerStream,
                formatInfo: speechAudioFormatConfig);

            // Synthesize text to a wav stream
            await Task.Run(() => speechSynthesizer.Speak(prompt), cancellationToken.Token);
            speechSynthesizerStream.Position = 0;
            var bitRate = (speechAudioFormatConfig.AverageBytesPerSecond * 8);

            // Convert wav stream to mp3 or ogg
            switch (format)
            {
                case AUDIO_FORMAT.MP3:
                    using (var mp3StreamWriter = new LameMP3FileWriter(outStream: audioStream, format: waveFormat, bitRate: bitRate))
                        await speechSynthesizerStream.CopyToAsync(mp3StreamWriter, cancellationToken.Token);
                    break;
                case AUDIO_FORMAT.OGG:
                    VorbisEncoder.ConvertPCMFile(
                        speechSynthesizerStream,
                        audioStream,
                        speechAudioFormatConfig.SamplesPerSecond,
                        speechAudioFormatConfig.ChannelCount);
                    break;
                default:
                    await speechSynthesizerStream.CopyToAsync(audioStream, cancellationToken.Token);
                    break;
            }

            // Set the position back to zero to start from the begining
            audioStream.Position = 0;
            return audioStream;
        }
    }
}
