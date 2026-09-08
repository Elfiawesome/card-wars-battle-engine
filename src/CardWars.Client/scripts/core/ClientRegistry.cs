using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.registry;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.core;

public class ClientRegistry
{
	public HandlerRegistry<PacketContextClient> PacketHandlers = new();
	public InstanceSceneRegistry<ResourceId> Instances;
	public SceneRegistry<ResourceId> UserInterface = new();
	public SceneRegistry<ResourceId> GameObjects = new();

	public ClientRegistry() { Instances = new(this); }
}