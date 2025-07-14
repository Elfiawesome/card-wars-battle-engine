using System.Data.SqlTypes;
using System.Net;
using System.Text;

namespace Standalone.Webserver;

public class Webserver : IAsyncDisposable
{
	private readonly HttpListener _listener;
	private bool _isRunning = false;
	private Task _listenTask;
	static string ResourcePath = "C:/Users/elfia/OneDrive/Desktop/card-wars-battle-engine/Standalone/Webserver/Resource/";

	public Webserver()
	{
		_listener = new HttpListener();
		_listener.Prefixes.Add("http://localhost:3112/");

		_listenTask = ListenTask();
	}

	private async Task<byte[]> LoadFromResource(string relativePath)
	{
		var fullPath = Path.Join(ResourcePath, relativePath);
		using (var memoryStream = new MemoryStream())
		{
			using (FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
			{
				await fileStream.CopyToAsync(memoryStream);
			}
			return memoryStream.ToArray();
		}
	}

	private async Task<SiteBytes> HandleConnection(HttpListenerRequest request)
	{
		List<string> parts = request?.RawUrl?.Substring(1).Split("/").ToList() ?? [""];
		var target = parts[0];
		parts.RemoveAt(0);

		// Console.WriteLine($"Raw URL: {request?.RawUrl}");

		switch (target)
		{
			case "":
				return new SiteBytes(await LoadFromResource("sites/main/main.html"));
			case "styles":
				return new SiteBytes(await LoadFromResource("sites/" + string.Join("/", parts) + "/style.css"));
			case "scripts":
				return new SiteBytes(await LoadFromResource("sites/" + string.Join("/", parts) + "/script.js"));
			case "img":
				return new SiteBytes(await LoadFromResource("assets/img/" + string.Join("/", parts)));
			case "favicon.ico":
				return new SiteBytes(await LoadFromResource("assets/favicon.png"), "image/png");
			default:
				return new SiteBytes(await LoadFromResource("sites/error/page-not-found.html"));
		}
	}

	private async Task ListenTask()
	{
		_listener.Start();
		_isRunning = true;
		while (_isRunning)
		{
			try
			{
				HttpListenerContext ctx = await _listener.GetContextAsync();
				HttpListenerRequest request = ctx.Request;
				HttpListenerResponse response = ctx.Response;
				try
				{
					var data = await HandleConnection(request);
					response.ContentLength64 = data.Data.LongLength;
					response.ContentType = data.ContentType;
					await response.OutputStream.WriteAsync(data.Data);
				}
				catch (Exception handlingException)
				{
					var data = Encoding.UTF8.GetBytes($"<h1>Internal Server Error</h1><p>{handlingException.Message}</p>");
					response.ContentLength64 = data.LongLength;
					await response.OutputStream.WriteAsync(data);
					Console.WriteLine($"[Webserver Handling Error]: {handlingException.Message}");
				}
			}
			catch (Exception listenerException)
			{
				Console.WriteLine($"[Webserver Listener Error]: {listenerException.Message}");
			}
		}
	}

	public async ValueTask DisposeAsync()
	{
		_isRunning = false;
		_listener.Close();
		await _listenTask;
	}
}

public record struct SiteBytes
{
	public byte[] Data = [];
	public string? ContentType;

	public SiteBytes(byte[] data)
	{
		Data = data;
	}

	public SiteBytes(byte[] data, string contentType)
	{
		Data = data;
		ContentType = contentType;
	}
}
