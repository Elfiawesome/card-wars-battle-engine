using CardWars.Core.Data;
using CardWars.Core.Network.Transport;
using CardWars.Core.Registry;

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
	public ResourceId CurrentWorldId { get => ResourceId.Parse(PersistentData.GetString("current_world_id")); set => PersistentData.Set("current_world_id", value.ToString()); }
}

public enum PlayState
{
	AwaitingPlayerDataRequest,
	Play
}