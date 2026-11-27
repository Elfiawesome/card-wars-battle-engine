using CardWars.Client.scripts.cardwars.layout;
using CardWars.Client.scripts.cardwars.systems;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.cardwars.registry;

public class BattleViewRegistry
{
	public SceneRegistry<ResourceId> EntityScene { get; } = new();
	public EntityViewHandlerRegistry EntityViewHandlers { get; } = new();
	
	public ResourceId DefaultLayoutId;
	public Registry<ResourceId, IBattleLayoutHandler> LayoutHandler { get; } = new();
	public RegistryFactory<ResourceId, BattleAnimation> Animations { get; } = new();
	
	public ResourceId DefaultCardDisplay;
	public ResourceId DefaultCardInspectionPanel;
}