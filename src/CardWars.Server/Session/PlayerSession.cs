using CardWars.Core.Data;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Session;

public class PlayerSession(Guid id, IConnection connection)
{
	public Guid PlayerId { get; } = id;
	public PlayState PlayState {set; get;} = PlayState.AwaitingPlayerDataRequest;
	public IConnection Connection { get; } = connection;
	public IServerInstance? CurrentInstance { get; set; }
	public CompoundTag PersistentData { get; set; } = new();
}

public enum PlayState
{
	AwaitingPlayerDataRequest,
	Play
}