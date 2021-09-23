using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SynthesizerAudio.Tests
{
    public class IntegrationTests
    {
        private readonly SynthesizerWebAudioService service;

        public IntegrationTests()
        {
            service = new SynthesizerWebAudioService();
        }

        [Fact(Skip = "Only play when audio confirmation is needed")]
        public async Task Can_play_text_to_wav()
        {
            // Act
            var streamAudio = await service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions()
            {
                Format = SynthesizerWebAudioService.AUDIO_FORMAT.WAV,
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
            var result = await service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() { Format = SynthesizerWebAudioService.AUDIO_FORMAT.WAV });

            // Assert
            var saveFileLocation = Path.Combine(Environment.CurrentDirectory, "test.wav");
            await File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
        }

        [Fact]
        public async Task Can_convert_text_to_mp3()
        {
            // Act
            var result = await service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() { Format = SynthesizerWebAudioService.AUDIO_FORMAT.MP3 });

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
            var result = await service.TextToSpeechAudioAsync("this is a test", new TextToSpeechAudioOptions() { Format = SynthesizerWebAudioService.AUDIO_FORMAT.OGG });

            // Assert
            var saveFileLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "test.ogg");
            await System.IO.File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
        }

        [Fact]
        public void Can_get_voices()
        {
            var voices = service.GetVoiceNames();
            Assert.True(voices.Length >= 2);
        }

    }
}
