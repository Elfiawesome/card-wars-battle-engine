using Elfiawesome.CardWarsBattleEngine;
using Standalone.Webserver;

CardWarsBattleEngine Engine = new();
var player = Engine.AddPlayer();
var player2 = Engine.AddPlayer();
var player3 = Engine.AddPlayer();
var battlefield = Engine.AddBattlefield();

Console.WriteLine(player.Id);
Console.WriteLine(battlefield.Id);



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