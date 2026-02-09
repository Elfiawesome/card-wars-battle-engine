using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Server;

Server server = new();
server.BattleEngine.LoadMod(new VanillaMod());


var playerId1 = new EntityId(Guid.NewGuid());
var playerId2 = new EntityId(Guid.NewGuid());
var playerId3 = new EntityId(Guid.NewGuid());
server.BattleEngine.HandleInput(Guid.Empty, new PlayerJoinedInput(playerId1));
server.BattleEngine.HandleInput(Guid.Empty, new PlayerJoinedInput(playerId2));
server.BattleEngine.HandleInput(Guid.Empty, new PlayerJoinedInput(playerId3));
server.BattleEngine.HandleInput(playerId1, new PlayerEndTurnInput());


// Print all entities
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