using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.core;

public class ClientRegistry : ExtensibleRegistry
{
	public HandlerRegistry<PacketContextClient> PacketHandlers { get; } = new();
	public SceneRegistry<ResourceId> Instances { get; } = new();
	public SceneRegistry<ResourceId> UserInterface { get; } = new();
	public SceneRegistry<ResourceId> GameObjects { get; } = new();
	public ResourceRegistry<ResourceId> Resources { get; } = new();
}
