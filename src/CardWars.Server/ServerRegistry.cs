using CardWars.Core.Data;
using CardWars.Core.Registry;
using CardWars.Server.Packet;

namespace CardWars.Server;

public class ServerRegistry
{
	public HandlerRegistry<PacketContextServer> PacketHandlers = new();
	public Registry<ResourceId, CompoundTag> WorldDefinitions { get; } = new();
	public ResourceId DefaultWorld;
}