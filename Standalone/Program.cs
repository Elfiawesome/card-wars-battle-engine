using Elfiawesome.CardWarsBattleEngine;
using Standalone.Webserver;

CardWarsBattleEngine Engine = new();
var player1 = Engine.AddPlayer();
var player2 = Engine.AddPlayer();

Engine.StartGame();

// Engine.Input(player1.Id, new NextTurnInput());

await using (var ws = new Webserver())
{
	Console.ReadKey(true);
}

Console.WriteLine("Game End!");