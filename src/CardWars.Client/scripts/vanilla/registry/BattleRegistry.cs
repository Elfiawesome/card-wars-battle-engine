using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.registry;
using CardWars.Client.scripts.vanilla.layout;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.vanilla.registry;

public class BattleRegistry : IClientRegistryExtension
{
	public SceneRegistry<ResourceId> EntityScene = new();
	public EntityViewHandlerRegistry EntityViewHandlers = new();
	
	public ResourceId DefaultLayoutId;
	public Registry<ResourceId, IBattleLayoutHandler> LayoutHandler = new();
}