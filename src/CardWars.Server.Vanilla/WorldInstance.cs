using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla;

public class WorldInstance(Guid id) : IServerInstance
{
	public Guid InstanceId => id;

	private readonly List<PlayerSession> _players = [];
	public CompoundTag? WorldConfig { get; private set; }

	public void Load(DataTag data)
	{
		if (data is CompoundTag compoundTag) { WorldConfig = compoundTag; }
	}

	public DataTag Save()
	{
		var tag = new CompoundTag();
		return tag;
	}

	public void AddPlayer(PlayerSession player)
		=> _players.Add(player);

	public void RemovePlayer(PlayerSession player)
		=> _players.Remove(player);

	public void HandlePacket(PlayerSession session, IPacket packet)
	{
		// TODO: world-specific packets (chat, movement, interactions)
	}

	public void Tick(float deltaTime)
	{
		// TODO: world-specific tick (NPCs, terrain, time)
	}
}
