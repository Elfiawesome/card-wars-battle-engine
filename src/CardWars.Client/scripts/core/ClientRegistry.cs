using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.core;

public class ClientRegistry : ExtensibleRegistry
{
	public HandlerRegistry<PacketContextClient> PacketHandlers = new();
	public SceneRegistry<ResourceId> Instances = new();
	public SceneRegistry<ResourceId> UserInterface = new();
	public SceneRegistry<ResourceId> GameObjects = new();
	public ResourceRegistry<ResourceId> Resources = new();
}
