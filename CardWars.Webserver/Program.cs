using CardWars.BattleEngine;
using CardWars.BattleEngine.Input;

Console.WriteLine("Hello, World!");

var e = new BattleEngine();
var p1 = e.AddPlayer();
// var p2 = e.AddPlayer();
// var p3 = e.AddPlayer();

if (e.EntityService.Players.TryGetValue(p1, out var player))
{
	if (e.EntityService.Decks.TryGetValue(player.UnitDeckId, out var deck))
	{
		e.HandleInput(p1, new DrawCardInput(player.UnitDeckId));
		e.HandleInput(p1, new DrawCardInput(player.UnitDeckId));
		e.HandleInput(p1, new DrawCardInput(player.UnitDeckId));
		e.HandleInput(p1, new DrawCardInput(player.UnitDeckId));
	}
}
e.HandleInput(p1, new EndTurnInput());
// e.HandleInput(p2, new EndTurnInput());
// e.HandleInput(p3, new EndTurnInput());