using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.core;

public class ClientRegistry
{
	public HandlerRegistry<PacketContextClient> PacketHandlers = new();
	public SceneRegistry<ResourceId> Instances = new();
}