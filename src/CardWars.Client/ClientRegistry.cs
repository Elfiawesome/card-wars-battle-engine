using CardWars.Core.Registry;

namespace CardWars.Client;

public class ClientRegistry
{
	public HandlerRegistry<PacketContextClient> PacketHandlers = new();
}
