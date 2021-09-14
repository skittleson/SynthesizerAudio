using Microsoft.AspNetCore.Mvc;
using SynthesizerAudio;
using System;
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
            var synthesizerAudioFactory = new SynthesizerWebAudioService();
            var synthResponse = await synthesizerAudioFactory.HandleRequest(text, SynthesizerWebAudioService.AUDIO_FORMAT.MP3);
            return new FileStreamResult(synthResponse.AudioStream, Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(synthResponse.ContentType));
        }
    }
}
