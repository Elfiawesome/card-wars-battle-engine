using Elfiawesome.CardWarsBattleEngine;
using Standalone.Webserver;

CardWarsBattleEngine Engine = new();
var player1 = Engine.AddPlayer();
var player2 = Engine.AddPlayer();

Console.WriteLine(player1.Id);

Engine.StartGame();

Console.WriteLine("Engine's Battlefields");
foreach (var b in Engine.Battlefields)
{
	Console.WriteLine($"Battlefield -> {b.Value.Id}");
	foreach (var us in b.Value.Grid)
	{
		Console.WriteLine($"  --Unit Slot [{us.Value.Id}]");
	}
}



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