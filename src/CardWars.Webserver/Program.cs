using CardWars.BattleEngine;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Server;

Server server = new();
server.BattleEngine.LoadMod(new VanillaMod());

// --- Game Test Simulation ---
// New Player Joins
var playerId1 = new EntityId(Guid.NewGuid());
server.BattleEngine.HandleInput(Guid.Empty, new PlayerJoinedRequestInput(playerId1));

// Player Draw
var deck = server.BattleEngine.State.OfType<Deck>().First();
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });
server.BattleEngine.HandleInput(playerId1, new DrawCardRequestInput() { DeckId = deck.Id, ReceivedPlayerId = playerId1 });

// Player use card
var card = server.BattleEngine.State.OfType<GenericCard>().First();
var unitSlot = server.BattleEngine.State.OfType<UnitSlot>().First();
server.BattleEngine.HandleInput(playerId1, new UseCardRequestInput() { CardId = card.Id, TargetEntityId = unitSlot.Id });







// --- Debug Dump State ---
Console.WriteLine(" --- Turn State --- ");
Console.WriteLine("Turn Index: " + server.BattleEngine.State.Turn.TurnIndex);
Console.WriteLine("Phase: " + server.BattleEngine.State.Turn.Phase);
Console.WriteLine("Turn Order Count: " + server.BattleEngine.State.Turn.TurnOrder.Count);
Console.WriteLine("Allowed Input Count: " + server.BattleEngine.State.Turn.AllowedPlayerInputs.Count);

Console.WriteLine(" --- All Entities --- ");
foreach (var id in server.BattleEngine.State.All)
{
	Console.WriteLine(id.Id + ": " + id.GetType().Name);
}

Console.WriteLine(Helper.GameStateDump(server.BattleEngine.State));