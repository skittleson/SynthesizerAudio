using Microsoft.AspNetCore.Mvc;
using SynthesizerAudio;
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
            var synthResponse = await synthesizerAudioFactory.WebResponseAsync(text);
            return new FileStreamResult(synthResponse.AudioStream, Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(synthResponse.ContentType));
        }
    }
}
