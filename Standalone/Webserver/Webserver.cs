using System.Net;
using System.Text;

namespace Standalone.Webserver;

public class Webserver : IAsyncDisposable
{
	public Func<string> ApiGameState;

	private readonly HttpListener _listener;
	private bool _isRunning = false;
	private Task _listenTask;
	static string ResourcePath = "C:/Users/elfia/OneDrive/Desktop/card-wars-battle-engine/Standalone/Webserver/Resource/";

	public Webserver()
	{
		_listener = new HttpListener();
		_listener.Prefixes.Add("http://localhost:3112/");
		_listener.Start();

		_listenTask = ListenTask();
	}

	public async Task ServeHtml(HttpListenerResponse resp, string htmlPath)
	{
		using (var sr = new StreamReader(Path.Combine(ResourcePath, htmlPath)))
		{
			string htmlString = await sr.ReadToEndAsync();
			byte[] data = Encoding.UTF8.GetBytes(htmlString);

			resp.ContentType = "text/html";
			resp.ContentEncoding = Encoding.UTF8;
			resp.ContentLength64 = data.LongLength;

			await resp.OutputStream.WriteAsync(data);
		}
	}

	public async Task ServeFile(HttpListenerResponse resp, string filePath, string contentType = "")
	{
		var data = await File.ReadAllBytesAsync(Path.Combine(ResourcePath, filePath));
		resp.ContentType = contentType;
		resp.ContentLength64 = data.LongLength;

		await resp.OutputStream.WriteAsync(data);
	}

	public async Task ServeApiResponse(HttpListenerResponse resp, string jsonData)
	{
		byte[] data = Encoding.UTF8.GetBytes(jsonData);

		resp.ContentType = "application/json";
		resp.ContentEncoding = Encoding.UTF8;
		resp.ContentLength64 = data.LongLength;

		await resp.OutputStream.WriteAsync(data);
	}

	private async Task HandleConnection(HttpListenerRequest request, HttpListenerResponse response)
	{
		string[] urlParts = request?.RawUrl?.Substring(1).Split("/") ?? [""];
		var target = urlParts[0];
		List<string> parts = urlParts.ToList();
		parts.RemoveAt(0);

		switch (target)
		{
			case "":
				await ServeHtml(response, "main-page/index.html");
				break;
			case "files":
				await ServeFile(response, string.Join("/", parts));
				break;
			case "api":
				await HandleApiRequest(parts[0], response);
				break;
			case "favicon.ico":
				await ServeFile(response, "assets/favicon.png", "image/png");
				break;
			default:
				await ServeHtml(response, "error-page/index.html");
				break;
		}
	}

	private async Task HandleApiRequest(string apiRequestType, HttpListenerResponse response)
	{
		switch (apiRequestType)
		{
			case "gamestate":
				var data = ApiGameState.Invoke();
				await ServeApiResponse(response, data);
				break;
		}
	}

	private async Task ListenTask()
	{
		_isRunning = true;
		while (_isRunning)
		{
			try
			{
				HttpListenerContext ctx = await _listener.GetContextAsync();
				HttpListenerRequest req = ctx.Request;
				HttpListenerResponse resp = ctx.Response;
				await HandleConnection(req, resp);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[Webserver Error]: {exception.Message}");
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
