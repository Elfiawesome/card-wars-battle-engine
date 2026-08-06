using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;

namespace CardWars.Server;

public class ServerRegistry
{
	public HandlerRegistry<PacketContextServer> PacketHandlers = new();
	public HandlerRegistry<PacketUnauthenticatedContextServer> UnauthenticatedPacketHandlers = new();
	public Registry<ResourceId, IServerInstanceProvider> ServerInstanceProviders = new();
}
