using CardWars.Core.Data;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Session;

public class PlayerSession
{
	public PlayerSession(Guid id, IConnection connection)
	{
		PlayerId = id;
		Connection = connection;
	}

	public PlayState PlayState { set; get; } = PlayState.AwaitingPlayerDataRequest;
	public IConnection Connection { get; }
	public IServerInstance? CurrentInstance { get; set; }
	public CompoundTag PersistentData { get; set; } = new();

	public Guid PlayerId { get => PersistentData.GetGuid("player_id"); set => PersistentData.Set("player_id", value); }
	public string Username { get => PersistentData.GetString("username"); set => PersistentData.Set("username", value); }
}

public enum PlayState
{
	AwaitingPlayerDataRequest,
	Play
}