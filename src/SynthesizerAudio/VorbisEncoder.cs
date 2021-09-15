using NAudio.Wave;
using OggVorbisEncoder;
using System;
using System.IO;
using System.Threading.Tasks;

//All Credits - https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder/blob/2ee366a2c6a92f095e0cfdaf0ae5d047fdb1cc35/OggVorbisEncoder.Example/Encoder.cs#L48
namespace SynthesizerAudio
{

    public interface IVorbisEncoder
    {
        Task EncodeAsync(WaveStream source, MemoryStream destination);
    }

    public class VorbisEncoder : IVorbisEncoder
    {
        private static readonly int WriteBufferSize = 512;

        public VorbisEncoder() { }
        public static IVorbisEncoder New() => new VorbisEncoder();

        public async Task EncodeAsync(WaveStream source, MemoryStream destination)
            => await CreatePCMAsync(source, destination);

        public static async Task CreatePCMAsync(WaveStream source, MemoryStream destination)
        {
            source.Position = 0;
            byte[] buffer = new byte[4096];
            var wav = new MemoryStream();
            int reader;
            while ((reader = await source.ReadAsync(buffer, 0, buffer.Length)) != 0)
                await wav.WriteAsync(buffer, 0, reader);

            var oggBytes = ConvertRawPCMFile(
                source.WaveFormat.SampleRate,
                source.WaveFormat.Channels,
                wav.ToArray(),
                source.WaveFormat.BitsPerSample == 16 ? PcmSample.SixteenBit : PcmSample.EightBit,
                source.WaveFormat.AverageBytesPerSecond,
                source.WaveFormat.Channels);
            await destination.WriteAsync(oggBytes, 0, oggBytes.Length);
        }

        private static byte[] ConvertRawPCMFile(int outputSampleRate, int outputChannels, byte[] pcmSamples, PcmSample pcmSampleSize, int pcmSampleRate, int pcmChannels)
        {
            int numPcmSamples = (pcmSamples.Length / (int)pcmSampleSize / pcmChannels);
            float pcmDuraton = numPcmSamples / (float)pcmSampleRate;

            int numOutputSamples = (int)(pcmDuraton * outputSampleRate);

            //Ensure that samble buffer is aligned to write chunk size
            numOutputSamples = (numOutputSamples / WriteBufferSize) * WriteBufferSize;

            float[][] outSamples = new float[outputChannels][];

            for (int ch = 0; ch < outputChannels; ch++)
            {
                outSamples[ch] = new float[numOutputSamples];
            }

            for (int sampleNumber = 0; sampleNumber < numOutputSamples; sampleNumber++)
            {
                float rawSample = 0.0f;

                for (int ch = 0; ch < outputChannels; ch++)
                {
                    int sampleIndex = (sampleNumber * pcmChannels) * (int)pcmSampleSize;

                    if (ch < pcmChannels) sampleIndex += (ch * (int)pcmSampleSize);

                    switch (pcmSampleSize)
                    {
                        case PcmSample.EightBit:
                            rawSample = ByteToSample(pcmSamples[sampleIndex]);
                            break;
                        case PcmSample.SixteenBit:
                            rawSample = ShortToSample((short)(pcmSamples[sampleIndex + 1] << 8 | pcmSamples[sampleIndex]));
                            break;
                    }

                    outSamples[ch][sampleNumber] = rawSample;
                }
            }

            return GenerateFile(outSamples, outputSampleRate, outputChannels);
        }

        private static byte[] GenerateFile(float[][] floatSamples, int sampleRate, int channels)
        {
            var outputData = new MemoryStream();

            // Stores all the static vorbis bitstream settings
            var info = VorbisInfo.InitVariableBitRate(channels, sampleRate, 0.5f);

            // set up our packet->stream encoder
            var serial = new Random().Next();
            var oggStream = new OggStream(serial);

            // =========================================================
            // HEADER
            // =========================================================
            // Vorbis streams begin with three headers; the initial header (with
            // most of the codec setup parameters) which is mandated by the Ogg
            // bitstream spec.  The second header holds any comment fields.  The
            // third header holds the bitstream codebook.

            var comments = new Comments();
            //comments.AddTag("ARTIST", "TEST");

            var infoPacket = HeaderPacketBuilder.BuildInfoPacket(info);
            var commentsPacket = HeaderPacketBuilder.BuildCommentsPacket(comments);
            var booksPacket = HeaderPacketBuilder.BuildBooksPacket(info);

            oggStream.PacketIn(infoPacket);
            oggStream.PacketIn(commentsPacket);
            oggStream.PacketIn(booksPacket);

            // Flush to force audio data onto its own page per the spec
            FlushPages(oggStream, outputData, true);

            // =========================================================
            // BODY (Audio Data)
            // =========================================================
            var processingState = ProcessingState.Create(info);

            for (int readIndex = 0; readIndex <= floatSamples[0].Length; readIndex += WriteBufferSize)
            {
                if (readIndex == floatSamples[0].Length)
                {
                    processingState.WriteEndOfStream();
                }
                else
                {
                    processingState.WriteData(floatSamples, WriteBufferSize, readIndex);
                }

                while (!oggStream.Finished && processingState.PacketOut(out OggPacket packet))
                {
                    oggStream.PacketIn(packet);

                    FlushPages(oggStream, outputData, false);
                }
            }

            FlushPages(oggStream, outputData, true);

            return outputData.ToArray();
        }

        private static void FlushPages(OggStream oggStream, Stream output, bool force)
        {
            while (oggStream.PageOut(out OggPage page, force))
            {
                output.Write(page.Header, 0, page.Header.Length);
                output.Write(page.Body, 0, page.Body.Length);
            }
        }

        private static float ByteToSample(short pcmValue) => pcmValue / 128f;

        private static float ShortToSample(short pcmValue) => pcmValue / 32768f;
        enum PcmSample : int
        {
            EightBit = 1,
            SixteenBit = 2
        }
    }
}
