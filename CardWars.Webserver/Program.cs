using CardWars.BattleEngine;
using CardWars.BattleEngine.Input.Player;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.State.Entity;

var be = new BattleEngine();
be.BlockDispatcher.BlockBatchProcessedAction += (playerId, blockBatchRecord) =>
{
};

var p1 = be.AddPlayer();
var p2 = be.AddPlayer();
var p3 = be.AddPlayer();
var p4 = be.AddPlayer();

var deckId = be.State.Players[p1].ControllingDecks[DeckType.Unit].First();
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));
be.HandleInput(p1, new DrawCardInput(deckId));

Console.WriteLine(StateSerializer.ToJson(be.State));

be.Mapping.Print();