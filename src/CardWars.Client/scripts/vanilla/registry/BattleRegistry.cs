using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.vanilla.registry;

public class BattleRegistry : IClientRegistryExtension
{
	public SceneRegistry<ResourceId> EntityScene = new();
}