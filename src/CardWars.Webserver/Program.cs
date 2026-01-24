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


// Print all entities
foreach (var id in server.BattleEngine.State.All)
{
	Console.WriteLine(id.Id + ": " + id.GetType().Name);
}