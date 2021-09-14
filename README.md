# Synthesizer Audio
> Synthesize text to speech audio in a web app - this is a utility and example synthesize text to an audio format WAV/MP3/OGG, then stream it to a browser client using HTML5 audio tag.

Inspired and battle tested for years from my initial tip: https://www.codeproject.com/tips/1031689/speaking-asp-net-website


<!-- ## ✨ Demo

![Demo gif](demo.gif) -->

## 🚀 Quick Start

Install [nuget package](https://www.nuget.org/packages/SynthesizerAudio/)

Web server request/response using WatsonWebserver
```csharp
static async Task GetTextToAudio(HttpContext ctx)
    {
        try
        {
            var requestUrl = new Uri(ctx.Request.Url.Full);
            var synthesizerAudioFactory = new SynthesizerWebAudioService();
            var synthResponse = await synthesizerAudioFactory.HandleGetWebRequestAsync(requestUrl);
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = synthResponse.ContentType;
            ctx.Response.ContentLength = synthResponse.ContentLength;
            ctx.Response.Headers.Add("Accept-Ranges", "bytes");
            await ctx.Response.Send(synthResponse.ToArray());
        }
        catch (Exception ex)
        {
            ctx.Response.StatusCode = 500;
            await ctx.Response.Send(ex.InnerException.Message);
        }
    }
```


If using WebApi, method in controller

```csharp
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
```


Play it from 
```html
<audio id="textToSpeech" controls>
    <source src="/api/texttoaudio?text=hello%20world&type=ogg" type="audio/ogg">
    <source src="/api/texttoaudio?text=hello%20world&type=mp3" type="audio/mpeg">
    <p>
        Your browser doesn't support HTML5 audio. Here is
        a <a href="/api/texttoaudio?text=hello%20world&type=mp3">link to the audio</a> instead.
    </p>
</audio>
```

Check out the examples folder

## 🤝 Contributing

Contributions, issues and feature requests are welcome.<br />
Feel free to check [issues page](https://github.com/skittleson/SpeakingWebApp/issues) if you want to contribute.<br />

## Author

👤 **Spencer Kittleson**

- Twitter: [@skittleson](https://twitter.com/skittleson)
- Github: [@skittleson](https://github.com/skittleson)
- LinkedIn: [@skittleson](https://www.linkedin.com/in/skittleson)
- StackOverflow: [spencer](https://stackoverflow.com/users/2414540/spencer)
- Blog: [DoCodeThatMatters](https://docodethatmatters.com)

## Show your support

⭐️ this repository if this project helped you! It motivates us a lot! 👋

Buy me a coffee ☕: <a href="https://www.buymeacoffee.com/skittles">skittles</a><br />

## Built with ♥

- [NAudio](https://github.com/naudio/NAudio)
- [NAudio Lame](https://github.com/Corey-M/NAudio.Lame)
- [Orbis Vorbis Encoder](https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder)
- [ASP.NET](https://dotnet.microsoft.com/apps/aspnet)

## 📝 License

Copyright © 2021 [Spencer Kittleson - Do Code That Matters](https://DoCodeThatMatters.com). <br />
This project is [MIT](https://github.com/skittleson/GcodeController/blob/master/LICENSE) licensed.

## References 
 - https://www.codeproject.com/tips/1031689/speaking-asp-net-website
 - https://developer.mozilla.org/en-US/docs/Web/HTML/Element/audio
 - https://www.tenforums.com/tutorials/132456-add-remove-speech-voices-windows-10-a.html#:~:text=1%20Open%20Settings%2C%20and%20click%2Ftap%20on%20the%20Time,and%20click%2Ftap%20on%20Add.%20...%20More%20items...%20