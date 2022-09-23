using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SynthesizerAudio.Tests
{
    public class IntegrationTests
    {
        private readonly SynthesizerWebAudioService _service;

        public IntegrationTests() {
            _service = new SynthesizerWebAudioService(null);
        }

        [Fact]
        public async Task Can_play_text_to_wav()
        {
            // Act
            var streamAudio = await _service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() {
                Format = AUDIO_FORMAT.WAV,
                VoiceName = "David"
            });

            // Assert
            using System.Media.SoundPlayer sound = new System.Media.SoundPlayer(streamAudio);
            sound.Play();
        }

        [Fact]
        public async Task Can_convert_text_to_wav()
        {
            // Act
            var result = await _service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() { Format = AUDIO_FORMAT.WAV });

            // Assert
            var saveFileLocation = Path.Combine(Environment.CurrentDirectory, "test.wav");
            await File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
        }

        [Fact]
        public async Task Can_convert_text_to_mp3()
        {
            // Act
            var result = await _service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() { Format = AUDIO_FORMAT.MP3 });

            // Assert
            var saveFileLocation = Path.Combine(Environment.CurrentDirectory, "test.mp3");
            await File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
            var reader = new Mp3FileReader(saveFileLocation);
            Assert.Equal(1, reader.WaveFormat.Channels);
            Assert.Equal(22050, reader.WaveFormat.SampleRate);
            Assert.Equal(3, Math.Round(reader.TotalTime.TotalSeconds, 0));
        }

        [Fact]
        public async Task Can_convert_text_to_ogg()
        {
            // Act
            var result = await _service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() { Format = AUDIO_FORMAT.OGG });

            // Assert
            var saveFileLocation = Path.Combine(Environment.CurrentDirectory, "test.ogg");
            await File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
        }

        [Fact]
        public async Task Can_get_voices()
        {
            var voices = await _service.GetVoiceNamesAsync();
            Assert.True(voices.Length >= 2);
        }

        [Fact(Skip = "Azure")]
        public async Task Azure_test()
        {
            // Arrange
            var text = @"The color of animals is by no means a matter of chance; it depends on many considerations, but in the majority of cases tends to protect the animal from danger by rendering it less conspicuous. Perhaps it may be said that if coloring is mainly protective, there ought to be but few brightly colored animals. There are, however, not a few cases in which vivid colors are themselves protective. The kingfisher itself, though so brightly colored, is by no means easy to see. The blue harmonizes with the water, and the bird as it darts along the stream looks almost like a flash of sunlight.";

            // Act
            // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/get-started-speech-to-text?tabs=windowsinstall&pivots=programming-language-csharp
            var azureSpeech = new SynthesizerWebAudioService(new AzureSpeechSynthesizer("replace", "replace"));
            var streamAudio = await azureSpeech.TextToSpeechAudioAsync(text, new TextToSpeechAudioOptions() {
                Format = AUDIO_FORMAT.WAV
            });

            // Assert
            using System.Media.SoundPlayer sound = new System.Media.SoundPlayer(streamAudio);
            sound.Play();
        }

        [Fact]
        public async Task Can_use_polly() {
            // Arrange
            var text = @"The color of animals is by no means a matter of chance; it depends on many considerations, but in the majority of cases tends to protect the animal from danger by rendering it less conspicuous. Perhaps it may be said that if coloring is mainly protective, there ought to be but few brightly colored animals. There are, however, not a few cases in which vivid colors are themselves protective. The kingfisher itself, though so brightly colored, is by no means easy to see. The blue harmonizes with the water, and the bird as it darts along the stream looks almost like a flash of sunlight.";
            //new BasicAWSCredentials("AKIAYNK6RNQGFKTTL6E3", "NT/o+Vr98SXoVE64rKY8Mb4yPgcv5G3HdxMeA8l+")
            // Act
            var synth = new SynthesizerWebAudioService(new AmazonPollySpeechSynthesizer());
            var streamAudio = await synth.TextToSpeechAudioAsync(text, new TextToSpeechAudioOptions() {
                Format = AUDIO_FORMAT.WAV
            });

            // Assert
            using System.Media.SoundPlayer sound = new System.Media.SoundPlayer(streamAudio);
            sound.Play();
        }
    }
}

