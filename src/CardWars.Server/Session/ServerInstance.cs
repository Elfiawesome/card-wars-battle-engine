using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;

namespace CardWars.Server.Session;

public interface IServerInstance
{
	Guid InstanceId { get; }
	ResourceId InstanceProviderId { get; }

	IReadOnlyCollection<PlayerSession> Players { get; }

	void AddPlayer(PlayerSession player);
	void RemovePlayer(PlayerSession player);

	void HandlePacket(PlayerSession session, IPacket packet);
	void Tick(float deltaTime);
}

public abstract class ServerInstance : IServerInstance
{
	private readonly List<PlayerSession> _players = [];

	public abstract Guid InstanceId { get; set; }
	public abstract ResourceId InstanceProviderId { get; set; }

	public IReadOnlyCollection<PlayerSession> Players => _players;

	public virtual void AddPlayer(PlayerSession player)
	{
		if (!_players.Contains(player))
			_players.Add(player);
	}

	public virtual void RemovePlayer(PlayerSession player)
		=> _players.Remove(player);

	public abstract void HandlePacket(PlayerSession session, IPacket packet);
	public abstract void Tick(float deltaTime);
}