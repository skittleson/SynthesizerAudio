using NAudio.Lame;
using NAudio.Wave;
using System.IO;
using System.Speech.AudioFormat;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public interface IMp3Encoder
    {
        Task EncodeAsync(MemoryStream source, MemoryStream destination, SpeechAudioFormatInfo speechAudioFormatInfo);
    }
    public class Mp3Encoder : IMp3Encoder
    {
        public async Task EncodeAsync(MemoryStream source, MemoryStream destination, SpeechAudioFormatInfo speechAudioFormatInfo)
        {
            // NAudio wave format has to be the same as speech audio format
            var waveFormat = new WaveFormat(
                rate: speechAudioFormatInfo.SamplesPerSecond,
                bits: speechAudioFormatInfo.BitsPerSample,
                channels: speechAudioFormatInfo.ChannelCount);
            var bitRate = (speechAudioFormatInfo.AverageBytesPerSecond * 8);

            // Encode to mp3
            using var mp3StreamWriter = new LameMP3FileWriter(outStream: destination, format: waveFormat, bitRate: bitRate);
            await source.CopyToAsync(mp3StreamWriter);
        }
    }
}
