using Moq;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SynthesizerAudio.Tests
{
    public class HandlerTests
    {
        private readonly SynthesizerWebAudioService service;
        public Mock<IVorbisEncoder> VorbisEncoderMock { get; }
        public Mock<IMp3Encoder> Mp3EncoderMock { get; }
        public Mock<ISpeechSynthesizerFactory> SynthesizerMock { get; }


        public HandlerTests()
        {
            VorbisEncoderMock = new Mock<IVorbisEncoder>(MockBehavior.Strict);
            Mp3EncoderMock = new Mock<IMp3Encoder>(MockBehavior.Strict);
            SynthesizerMock = new Mock<ISpeechSynthesizerFactory>(MockBehavior.Strict);
            service = new SynthesizerWebAudioService(VorbisEncoderMock.Object, Mp3EncoderMock.Object, SynthesizerMock.Object);
        }

        [Fact]
        public async Task Can_handle_web_request()
        {

            // Arrange
            var requestUrl = new Uri("http://foo.bar?text=hello%20world&type=mp3");
            SynthesizerMock.Setup(x => x.GetStreamAsync("hello world", It.IsAny<TextToSpeechAudioOptions>())).ReturnsAsync(await MockWaveStreamAsync());
            Mp3EncoderMock.Setup(x => x.EncodeAsync(It.IsAny<WaveStream>(), It.IsAny<MemoryStream>())).Returns(Task.CompletedTask);


            // Act
            var response = await service.HandleGetWebRequestAsync(requestUrl);

            // Assert

            Assert.Equal("audio/mp3", response.ContentType);
        }

        private async Task<WaveStream> MockWaveStreamAsync()
        {
            var outStream = new MemoryStream();
            var writer = new WaveFileWriter(outStream, new WaveFormat());
            await writer.FlushAsync();
            outStream.Position = 0;
            return new WaveFileReader(outStream);
        }

    }
}
