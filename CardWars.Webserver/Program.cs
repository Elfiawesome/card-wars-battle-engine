
using System.Net;
using System.Text;
using CardWars.BattleEngine;
using CardWars.BattleEngine.Core.Inputs;
using CardWars.Webserver;

var engine = new BattleEngine();
var playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();
playerId = engine.NewPlayerJoined();
engine.StartGame();

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


	// Super **Crude** Input Handling
	Dictionary<string, Func<string, IInputData>> InputMapping = new()
	{
		{"End Turn", (o) => new EndTurnInputData()},
		{"Player Joined?", (o) => new PlayerJoinedInputData()},
		{"Draw Card", (o) => {
				_ = long.TryParse(o, out var id);
				return new DrawCardFromDeckInputData(new(id));
		}}
	};
	var query = request?.QueryString;
	if (query != null)
	{
		if (query.AllKeys.Contains("action") && query.AllKeys.Contains("player"))
		{
			var playerInput = query["player"] ?? "";
			var actionInput = query["action"] ?? "";
			var argsString = query["args"] ?? "";
			if (InputMapping.TryGetValue(actionInput, out var func))
			{
				var inputData = func(argsString);
				engine.HandleInput(new(int.Parse(playerInput)), inputData);
			}
		}
	}

	if (requestPath == null) { continue; }

	switch (requestPath)
	{
		case "/":
			var builder = new HTMLBuilder();
			builder.state = engine.GameState;
			builder.options = [.. InputMapping.Keys];
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