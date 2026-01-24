using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Input;
using CardWars.Server;

Server server = new();
server.BattleEngine.LoadMod(new VanillaMod());


var playerId = new EntityId(Guid.NewGuid());
server.BattleEngine.HandleInput(new PlayerJoinedInput(playerId));

// with floating purple crystal shards,

foreach (var id in server.BattleEngine.State.All)
{
	Console.WriteLine(id);
}