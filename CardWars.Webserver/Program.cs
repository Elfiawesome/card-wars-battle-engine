using CardWars.BattleEngine;
using CardWars.BattleEngine.Input.Player;
using CardWars.BattleEngine.State;

var be = new BattleEngine();
var p1 = be.AddPlayer();
var p2 = be.AddPlayer();
var p3 = be.AddPlayer();
var p4 = be.AddPlayer();

var deckId = be.State.Players[p1].ControllingDecks[DeckType.Unit].First();
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));


be.State.PrintSnapshot();

Console.WriteLine(StateSerializer.ToJson(be.State));