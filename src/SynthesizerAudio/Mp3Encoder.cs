using NAudio.Lame;
using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.IO;
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

            // Use media foundation api if greater than windows XP
            if (Environment.OSVersion.Version.Major > 6.2)
            {
                var tempFile = Path.GetTempFileName();
                var mp3TempFile = tempFile.Replace(".tmp", ".mp3");
                File.Move(tempFile, mp3TempFile);
                try
                {
                    MediaFoundationApi.Startup();
                    MediaFoundationEncoder.EncodeToMp3(source, mp3TempFile);
                } catch (InvalidOperationException ex)
                {
                    // TODO: should use logger class here
                    Console.WriteLine(ex.Message);
                }
                using var mp3FileStream = new FileStream(mp3TempFile, FileMode.Open);
                await mp3FileStream.CopyToAsync(destination);
            } else
            {
                using var wav = WaveFormatConversionStream.CreatePcmStream(source);
                using var mp3 = new LameMP3FileWriter(destination, source.WaveFormat, LAMEPreset.STANDARD);
                await wav.CopyToAsync(mp3);
            }
        }
    }
}
