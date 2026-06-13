using CardWars.BattleEngine;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Server.Session;
using BattleEngineVanillaMod = CardWars.BattleEngine.Vanilla.VanillaMod;

namespace CardWars.Server.Vanilla;

public class BattleInstance(Guid id) : IServerInstance
{
	public Guid InstanceId => id;

	private readonly List<PlayerSession> _players = [];
	private BattleEngine.BattleEngine? _engine;

	public void Load(DataTag data)
	{
		_engine = new BattleEngine.BattleEngine();
		_engine.LoadMod(new BattleEngineVanillaMod(), []);
	}

	public DataTag Save()
	{
		var tag = new CompoundTag();
		return tag;
	}

	public void AddPlayer(PlayerSession player)
	{
		_players.Add(player);
		_engine?.HandleInput(EntityId.None, new PlayerJoinedRequestInput(player.PlayerId));
	}

	public void RemovePlayer(PlayerSession player)
		=> _players.Remove(player);

	public void HandlePacket(PlayerSession session, IPacket packet)
	{
		// TODO: route game inputs (attacks, card plays, end turn) to _engine.HandleInput
	}

	public void Tick(float deltaTime)
	{
		// TODO: tick engine
	}
}
