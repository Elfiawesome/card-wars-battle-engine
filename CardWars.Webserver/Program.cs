
using System.Net;
using System.Text;
using CardWars.BattleEngine;
using CardWars.Webserver;

var engine = new BattleEngine();
var playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();

// Setup webapp server
var listener = new HttpListener();
string url = "http://localhost:8080/";
listener.Prefixes.Add(url);
listener.Start();
Console.WriteLine($"Web Server is listening on {url}");


while (true)
{
	HttpListenerContext context = await listener.GetContextAsync();
	HttpListenerRequest request = context.Request;
	HttpListenerResponse response = context.Response;

	Console.WriteLine($"Received request: {request.HttpMethod} {request?.Url?.AbsolutePath}");
	string? requestPath = request?.Url?.AbsolutePath;

	if (requestPath == null) { continue; }
	
	switch (requestPath)
	{
		case "/":
			var builder = new HTMLBuilder();
			builder.state = engine.GameState;
			var s = builder.Build();

			response.ContentType = "text/html";
			response.StatusCode = (int)HttpStatusCode.OK;


			byte[] buffer = Encoding.UTF8.GetBytes(s);
			response.ContentLength64 = buffer.Length;
			response.OutputStream.Write(buffer, 0, buffer.Length);

			response.OutputStream.Close();
			break;
	}
}