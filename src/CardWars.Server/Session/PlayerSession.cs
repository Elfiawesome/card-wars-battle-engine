using CardWars.Core.Data;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Session;

[DataTagType()]
public class PlayerSession()
{
	[DataTag] public Guid PlayerId { get; set; } = Guid.Empty;
	[DataTag] public string Username { get; set; } = "";

	public required IConnection Connection { get; set; }
	public IServerInstance? CurrentInstance { get; set; }

	[DataTag] public Guid CurrentInstanceId => CurrentInstance?.InstanceId ?? Guid.Empty;
	[DataTag] public CompoundTag CustomData { get; set; } = new();
}