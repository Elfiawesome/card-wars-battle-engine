using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Packet;

namespace CardWars.Server.Session;

public interface IServerInstance
{
	Guid InstanceId { get; }
	ResourceId InstanceProviderId { get; }

	IReadOnlyCollection<PlayerSession> Players { get; }
	bool RemoveWhenEmpty { get; }

	void Ready();
	bool CanAddPlayer(PlayerSession player, out string? reason);
	void AddPlayer(PlayerSession player);
	void RemovePlayer(PlayerSession player);

	bool HandlePacket(PacketContextServer context, IPacket packet);
	void Tick(float deltaTime);
}

public abstract class ServerInstance : IServerInstance
{
	private readonly List<PlayerSession> _players = [];

	public abstract Guid InstanceId { get; set; }
	public abstract ResourceId InstanceProviderId { get; set; }

	public IReadOnlyCollection<PlayerSession> Players => _players;
	public virtual bool RemoveWhenEmpty => false;

	public virtual void Ready() { }

	public virtual bool CanAddPlayer(PlayerSession player, out string? reason)
	{
		reason = null;
		return true;
	}

	public virtual void AddPlayer(PlayerSession player)
	{
		if (!_players.Contains(player))
			_players.Add(player);
	}

	public virtual void RemovePlayer(PlayerSession player)
		=> _players.Remove(player);

	public abstract bool HandlePacket(PacketContextServer context, IPacket packet);
	public abstract void Tick(float deltaTime);
}