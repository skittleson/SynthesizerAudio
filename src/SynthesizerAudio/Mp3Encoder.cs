using NAudio.Lame;
using NAudio.Wave;
using System.IO;
using System.Speech.AudioFormat;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public interface IMp3Encoder
    {
        Task EncodeAsync(WaveStream source, MemoryStream destination);
    }
    public class Mp3Encoder : IMp3Encoder
    {
        public async Task EncodeAsync(WaveStream source, MemoryStream destination)
        {
            using (WaveStream wav = WaveFormatConversionStream.CreatePcmStream(source))
            using (var mp3 = new LameMP3FileWriter(destination, source.WaveFormat, LAMEPreset.STANDARD))
                await wav.CopyToAsync(mp3);
        }
    }
}
