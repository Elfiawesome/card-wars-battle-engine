using CardWars.BattleEngine.Core.Registry;
using CardWars.Server.Packet;

namespace CardWars.Server;

public class ServerRegistry
{
	public HandlerRegistry<PacketContext> PacketHandlers = new();
}