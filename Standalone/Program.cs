using Elfiawesome.CardWarsBattleEngine;
using Standalone.Webserver;

CardWarsBattleEngine Engine = new();
var player1 = Engine.AddPlayer();
var player2 = Engine.AddPlayer();
var player3 = Engine.AddPlayer();

Console.WriteLine(player1.Id);
Engine.StartGame();


await using (var ws = new Webserver())
{
	ws.ApiGameState += () =>
	{
		var data = "{}"; //JsonSerializer.Serialize(Engine.GetGameState());
		return data;
	};
	Console.WriteLine("Webserver up!");
	Console.ReadKey(true);
}

Console.WriteLine("Game End!");