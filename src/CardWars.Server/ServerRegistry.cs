using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;

namespace CardWars.Server;

// The owning Server supplies its real ILogger so packet-dispatch Debug/Error diagnostics reach the
// server console; absent an owner the immutable no-op NullLogger.Instance is used (R1).
public class ServerRegistry(ILogger? logger = null)
{
	public HandlerRegistry<PacketContextServer> PacketHandlers = new(logger);
	public HandlerRegistry<PacketUnauthenticatedContextServer> UnauthenticatedPacketHandlers = new(logger);
	public Registry<ResourceId, IServerInstanceProvider> ServerInstanceProviders = new();
}
