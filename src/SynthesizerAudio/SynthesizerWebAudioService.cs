using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SynthesizerAudio
{
    public class SynthesizerWebAudioService
    {
        //--- Private Properties ---
        private IVorbisEncoder VorbisEncoder { get; }
        private IMp3Encoder Mp3Encoder { get; }
        private ISpeechSynthesizerFactory SpeechSynthesizerFactory { get; }


        /// <summary>
        /// Text to speech synthesizer for web audio
        /// </summary>
        /// <param name="vorbisEncoder">Vorbis encoder</param>
        public SynthesizerWebAudioService(IVorbisEncoder vorbisEncoder, IMp3Encoder mp3Encoder, ISpeechSynthesizerFactory speechSynthesizerFactory)
        {

            VorbisEncoder = vorbisEncoder ?? throw new ArgumentNullException("Vorbis Encoder Required");
            Mp3Encoder = mp3Encoder ?? throw new ArgumentNullException("Mp3 Encoder Required");
            SpeechSynthesizerFactory = speechSynthesizerFactory ?? throw new ArgumentNullException("Speech Synthesizer Required");
        }

        public SynthesizerWebAudioService()
        {
            SpeechSynthesizerFactory ??= new SpeechSynthesizerFactory();
            VorbisEncoder ??= new VorbisEncoder();
            Mp3Encoder ??= new Mp3Encoder();
        }

        public enum AUDIO_FORMAT
        {
            WAV = 0,
            MP3 = 2,
            OGG = 4
        }

        //public string[] GetVoiceNames()
        //{
        //    return SpeechSynthesizer
        //        .GetInstalledVoices()
        //        .Select(x => x.VoiceInfo.Name)
        //        .ToArray();
        //}

        public async Task<MemoryStream> TextToSpeechAudioAsync(string text, AUDIO_FORMAT format)
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Setup audio stream to return on response
            var audioStream = new MemoryStream();

            // Speech format: https://docs.microsoft.com/en-us/dotnet/api/system.speech.audioformat?view=netframework-4.8
            var speechAudioFormatConfig = new SpeechAudioFormatInfo(
                samplesPerSecond: 16000,
                bitsPerSample: AudioBitsPerSample.Sixteen,
                channel: AudioChannel.Stereo);

            var speechSynthesizerStream = await SpeechSynthesizerFactory.GetStreamAsync(text, speechAudioFormatConfig);

            // Convert wav stream to mp3 or ogg
            switch (format)
            {
                case AUDIO_FORMAT.MP3:
                    await Mp3Encoder.EncodeAsync(
                        speechSynthesizerStream,
                        audioStream,
                        speechAudioFormatConfig);
                    break;
                case AUDIO_FORMAT.OGG:
                    VorbisEncoder.Encode(
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

        public async Task<SynthesizerWebAudioResponse> HandleRequestAsync(Uri requestedUrl)
        {
            var queryParams = HttpUtility.ParseQueryString(requestedUrl.Query);
            var text = queryParams["text"];
            var type = (queryParams["type"] ?? "").Trim().ToLower();
            var format = AUDIO_FORMAT.MP3;
            switch (type)
            {
                case "ogg":
                    format = AUDIO_FORMAT.OGG;
                    break;
                case "mp3":
                    format = AUDIO_FORMAT.MP3;
                    break;
                case "wav":
                    format = AUDIO_FORMAT.WAV;
                    break;
            }
            return await HandleRequest(text, format);
        }
        public async Task<SynthesizerWebAudioResponse> HandleRequest(string text, AUDIO_FORMAT format)
        {

            if (string.IsNullOrEmpty(text))
            {
                throw new MissingParameters("text");
            }
            var contentType = "audio/mp3";
            switch (format)
            {
                case AUDIO_FORMAT.OGG:
                    contentType = "audio/ogg";
                    break;
                case AUDIO_FORMAT.MP3:
                    contentType = "audio/mp3";
                    break;
                case AUDIO_FORMAT.WAV:
                    contentType = "audio/wav";
                    break;
            }
            var audio = await TextToSpeechAudioAsync(text, format);
            return new SynthesizerWebAudioResponse(audio, contentType, audio.Length);
        }

        public class SynthesizerWebAudioResponse
        {
            public SynthesizerWebAudioResponse(MemoryStream audioStream, string contentType, long contentLength)
            {
                AudioStream = audioStream;
                ContentType = contentType;
                ContentLength = contentLength;
            }

            public int StatusCode { get; }
            public MemoryStream AudioStream { get; }
            public string ContentType { get; }
            public long ContentLength { get; }

            public byte[] ToArray() => AudioStream?.ToArray() ?? new byte[0];
        }

        public class MissingParameters : Exception
        {
            public MissingParameters(string parameter)
            {
                Parameter = parameter;
            }
            public override string Message => $"Missing parameter {Parameter} or is empty";

            public string Parameter { get; }
        }
    }
}
