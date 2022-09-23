using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SynthesizerAudio
{
    public class AmazonPollySpeechSynthesizer : ISpeechSynthesizer
    {
        private AmazonPollyClient _client;

        /// <summary>
        ///  Use `new BasicAWSCredentials(accessKey, secretKey)`
        /// </summary>
        /// <param name="credentials"></param>
        public AmazonPollySpeechSynthesizer(AWSCredentials credentials) {
            _client = new AmazonPollyClient(credentials);
        }
        public AmazonPollySpeechSynthesizer(){
            _client = new AmazonPollyClient();
        }
        public async Task<MemoryStream> GetStreamAsync(string text, TextToSpeechAudioOptions options = null) {
            if (options.SampleRate > 16000) {
                options.SampleRate = 16000;
            }
            if (text.Length > 6000) {
                throw new ArgumentException($"{nameof(text)} can not be longer than 6,000 characters");
            }
            var voiceName = string.IsNullOrEmpty(options.VoiceName) ? "Joanna" : options.VoiceName;
            var describeVoiceResponse = await _client.DescribeVoicesAsync(new DescribeVoicesRequest());
            var voice = describeVoiceResponse.Voices.FirstOrDefault(x => x.Name == voiceName);

            //https://docs.aws.amazon.com/polly/latest/dg/API_SynthesizeSpeech.html#polly-SynthesizeSpeech-request-SampleRate
            var req = await _client.SynthesizeSpeechAsync(new SynthesizeSpeechRequest {
                OutputFormat = "pcm",
                SampleRate = options.SampleRate.ToString(),
                LanguageCode = voice.LanguageCode,
                VoiceId = voice.Id, 
                Text = text, 
            });
            if (req.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception("Fail to get audio from Amazon Polly");
            }

            // Only do this if on Windows for pcm data
            var ms = new MemoryStream();
            var rawPcmData = new RawSourceWaveStream(req.AudioStream, new WaveFormat(options.SampleRate, 1));
            WaveFileWriter.WriteWavFileToStream(ms, rawPcmData);
            ms.Position = 0;
            return ms;
        }
        public async Task<string[]> VoicesAsync() => (await _client
            .DescribeVoicesAsync(new DescribeVoicesRequest()))
            .Voices.Select(x => x.Name).ToArray();


        /// <summary>
        /// Use <see cref="VoicesAsync"/>
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use VoicesAsync", false)]
        public string[] Voices() => VoicesAsync().GetAwaiter().GetResult();
    }
}
