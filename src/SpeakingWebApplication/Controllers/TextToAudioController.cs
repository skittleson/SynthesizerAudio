using Microsoft.AspNetCore.Mvc;
using Speech.Logic;
using System.Threading.Tasks;

namespace SpeakingWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextToAudioController : ControllerBase
    {

        [HttpGet]
        public async Task<FileStreamResult> Get([FromQuery] string text)
        {
            var synthesizerAudioFactory = new SynthesizerAudioFactory();
            var audio = await synthesizerAudioFactory.TextToSpeechAudioAsync(text, SynthesizerAudioFactory.AUDIO_FORMAT.MP3);
            return new FileStreamResult(audio, Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("audio/mpeg"));
        }
    }
}
