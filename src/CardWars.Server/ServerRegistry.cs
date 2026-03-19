using CardWars.Core.Registry;
using CardWars.Server.Packet;

namespace CardWars.Server;

public class ServerRegistry
{
	public HandlerRegistry<PacketContextServer> PacketHandlers = new();
}