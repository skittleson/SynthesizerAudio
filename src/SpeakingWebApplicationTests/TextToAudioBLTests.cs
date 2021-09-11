using Speech.Logic;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SpeakingWebApplicationTests
{
    public class TextToAudioBLTests
    {
        private readonly SynthesizerAudioFactory service;

        public TextToAudioBLTests()
        {
            service = new SynthesizerAudioFactory();
        }


        [Fact]
        public async Task Can_convert_text_to_wav()
        {
            // Act
            var result = await service.TextToSpeechAudioAsync("this is a test", SynthesizerAudioFactory.AUDIO_FORMAT.WAV);

            // Assert
            var saveFileLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "test.wav");
            await System.IO.File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
        }

        [Fact]
        public async Task Can_convert_text_to_mp3()
        {
            // Act
            var result = await service.TextToSpeechAudioAsync("this is a test", SynthesizerAudioFactory.AUDIO_FORMAT.MP3);

            // Assert
            var saveFileLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "test.mp3");
            await System.IO.File.WriteAllBytesAsync(saveFileLocation, result.ToArray());


        }

        [Fact]
        public async Task Can_convert_text_to_ogg()
        {
            // Act
            var result = await service.TextToSpeechAudioAsync("this is a test", SynthesizerAudioFactory.AUDIO_FORMAT.OGG);

            // Assert
            var saveFileLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "test.ogg");
            await System.IO.File.WriteAllBytesAsync(saveFileLocation, result.ToArray());
        }

        [Fact]
        public void Can_get_voices()
        {

            var voices = service.GetVoiceNames();
            Assert.True(voices.Length > 2);

        }

    }
}
