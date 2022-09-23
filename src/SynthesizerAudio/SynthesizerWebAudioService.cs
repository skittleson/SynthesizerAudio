using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using static SynthesizerAudio.SynthesizerWebAudioService;

namespace SynthesizerAudio
{
    public interface ISynthesizerWebAudioService
    {
        Task<MemoryStream> TextToSpeechAudioAsync(string text, TextToSpeechAudioOptions options = null);
        Task<SynthesizerWebAudioResponse> HandleGetWebRequestAsync(Uri requestedUrl);
        Task<SynthesizerWebAudioResponse> WebResponseAsync(string text, TextToSpeechAudioOptions options = null);
    }

    public enum AUDIO_FORMAT
    {
        WAV = 0,
        MP3 = 2,
        OGG = 4
    }

    public enum SPEECH_SPEED
    {
        DEFAULT = 0,
        SLOW = 2,
        MEDIUM = 4,
        FAST = 8
    }

    public class TextToSpeechAudioOptions
    {
        public AUDIO_FORMAT Format { get; set; }
        public string VoiceName { get; set; }
        public SPEECH_SPEED Speed { get; set; }
        public int SampleRate { get; set; } = 44100;
        public int BitsPerSample { get; set; } = 16;
        public int Channels { get; set; } = 1;
    }

    public class SynthesizerWebAudioService : ISynthesizerWebAudioService
    {
        //--- Private Properties ---
        private IVorbisEncoder VorbisEncoder { get; }
        private IMp3Encoder Mp3Encoder { get; }
        private ISpeechSynthesizer SpeechSynthesizerFactory { get; }

        public SynthesizerWebAudioService() {
            VorbisEncoder = new VorbisEncoder();
            Mp3Encoder = new Mp3Encoder();
            SpeechSynthesizerFactory = new WindowsSpeechSynthesizer();
        }

        public SynthesizerWebAudioService(IVorbisEncoder vorbisEncoder, IMp3Encoder mp3Encoder, ISpeechSynthesizer speechSynthesizerFactory)
        {
            VorbisEncoder = vorbisEncoder
                ?? throw new ArgumentNullException("Vorbis Encoder Required");
            Mp3Encoder = mp3Encoder
                ?? throw new ArgumentNullException("Mp3 Encoder Required");
            SpeechSynthesizerFactory = speechSynthesizerFactory
                ?? throw new ArgumentNullException("Speech Synthesizer Required");
        }
        
        public SynthesizerWebAudioService(ISpeechSynthesizer speechSynthesizerFactory)
        {
            SpeechSynthesizerFactory = speechSynthesizerFactory ?? new WindowsSpeechSynthesizer();
            VorbisEncoder ??= new VorbisEncoder();
            Mp3Encoder ??= new Mp3Encoder();
        }

        public Task<string[]> GetVoiceNamesAsync() => SpeechSynthesizerFactory.VoicesAsync();

        public async Task<MemoryStream> TextToSpeechAudioAsync(string text, TextToSpeechAudioOptions options = null)
        {
            // Setup audio stream to return on response
            var audioStream = new MemoryStream();
            var speechSynthesizerStream = await SpeechSynthesizerFactory.GetStreamAsync(text, options);

            // Convert wav stream to mp3 or ogg
            var format = options?.Format ?? AUDIO_FORMAT.MP3;
            switch (format)
            {
                case AUDIO_FORMAT.MP3:
                    await Mp3Encoder.EncodeAsync(speechSynthesizerStream, audioStream);
                    break;
                case AUDIO_FORMAT.OGG:
                    await VorbisEncoder.EncodeAsync(speechSynthesizerStream, audioStream, options);
                    break;
                default:

                    // Already in a wav format so just return it
                    speechSynthesizerStream.Position = 0;
                    return speechSynthesizerStream;
            }

            // Set the position back to zero to start from the begining
            audioStream.Position = 0;
            return audioStream;
        }

        public async Task<SynthesizerWebAudioResponse> HandleGetWebRequestAsync(Uri requestedUrl)
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
            return await WebResponseAsync(text, new TextToSpeechAudioOptions { Format = format });
        }

        public async Task<SynthesizerWebAudioResponse> WebResponseAsync(string text, TextToSpeechAudioOptions options = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException(nameof(text));
            }
            var contentType = "audio/mp3";
            switch (options?.Format)
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
            var audio = await TextToSpeechAudioAsync(text, options);
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
            public byte[] ToArray() => AudioStream?.ToArray() ?? Array.Empty<byte>();
        }
    }
}