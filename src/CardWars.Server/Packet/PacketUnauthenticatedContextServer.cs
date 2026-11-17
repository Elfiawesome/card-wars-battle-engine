using CardWars.Core.Logging;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Packet;

public record struct PacketUnauthenticatedContextServer(
	Server Server,
	IConnection Connection
)
{
	/// <summary>The owning server's logger, so packet handlers log through the session logger.</summary>
	public readonly ILogger Logger => Server.Logger;
}