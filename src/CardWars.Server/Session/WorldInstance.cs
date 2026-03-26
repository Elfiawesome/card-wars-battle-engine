using CardWars.Core.Network.Packet;

namespace CardWars.Server.Session;

public class WorldInstance(Guid id) : IServerInstance
{
	public Guid InstanceId => id;
	private List<PlayerSession> _players = [];

	public void AddPlayer(PlayerSession player)
		=> _players.Add(player);

	public void RemovePlayer(PlayerSession player)
		=> _players.Remove(player);

	public void HandlePacket(PlayerSession session, IPacket packet)
	{
	}


	public void Tick(float deltaTime)
	{
	}
}