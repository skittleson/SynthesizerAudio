// See https://aka.ms/new-console-template for more information
using System.Net;

Console.WriteLine("Hello, World!");
SimpleListenerExample(new[] { "http://127.0.0.1:9090/" });
Console.ReadLine();

//https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-5.0
static void SimpleListenerExample(string[] prefixes)
{
    if (!HttpListener.IsSupported)
    {
        Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
        return;
    }
    // URI prefixes are required,
    // for example "http://contoso.com:8080/index/".
    if (prefixes == null || prefixes.Length == 0)
        throw new ArgumentException("prefixes");

    // Create a listener.
    var listener = new HttpListener();
    listener.IgnoreWriteExceptions = true;

    // Add the prefixes.
    foreach (string s in prefixes)
    {
        listener.Prefixes.Add(s);
    }
    listener.Start();
    Console.WriteLine("Listening...");
    while (true)
    {
        var context = listener.GetContext();
        context.Response.ContentType = "audio/mp3";
        context.Response.SendChunked = true;
        context.Response.KeepAlive = true;
        try
        {
            using var fs = new FileStream("rainy.mp3", FileMode.Open);
            fs.CopyToAsync(context.Response.OutputStream).GetAwaiter().GetResult();
        } catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        } finally
        {
            context.Response.OutputStream.Close();
            context.Response.OutputStream.Dispose();
            context.Response.Close();
        }
    }
    //listener.Stop();
}