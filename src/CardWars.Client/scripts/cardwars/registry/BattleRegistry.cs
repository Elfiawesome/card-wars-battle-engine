using CardWars.Client.scripts.cardwars.layout;
using CardWars.Client.scripts.cardwars.systems;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.registry;

public class BattleRegistry : IClientRegistryExtension
{
	public SceneRegistry<ResourceId> EntityScene = new();
	public EntityViewHandlerRegistry EntityViewHandlers = new();
	
	public ResourceId DefaultLayoutId;
	public Registry<ResourceId, IBattleLayoutHandler> LayoutHandler = new();
	public RegistryFactory<ResourceId, BattleAnimation> Animations = new();
}