﻿using SynthesizerAudio;
using System;
using System.Threading.Tasks;
using WatsonWebserver;

namespace SimpleSpeakingWebApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new Server("127.0.0.1", 5001, false, DefaultRouteAsync);
            s.Routes.Static.Add(HttpMethod.GET, "/api/texttoaudio", GetTextToAudioDefaultAsync);
            s.Start();
            Console.ReadLine();
        }

        static async Task DefaultRouteAsync(HttpContext ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/html";
            var indexPage = System.IO.Path.Combine(Environment.CurrentDirectory, "index.html");
            var content = await System.IO.File.ReadAllTextAsync(indexPage);
            await ctx.Response.Send(content);
        }

        static async Task GetTextToAudioDefaultAsync(HttpContext ctx)
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
            } catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send(ex.InnerException.Message);
            }
        }
    }
}
