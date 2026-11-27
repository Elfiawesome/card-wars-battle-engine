using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;

namespace CardWars.Server;

public class ServerRegistry : ExtensibleRegistry
{
	public HandlerRegistry<PacketContextServer> PacketHandlers { get; } = new();
	public HandlerRegistry<PacketUnauthenticatedContextServer> UnauthenticatedPacketHandlers { get; } = new();
	public Registry<ResourceId, IServerInstanceProvider> ServerInstanceProviders { get; } = new();
}
