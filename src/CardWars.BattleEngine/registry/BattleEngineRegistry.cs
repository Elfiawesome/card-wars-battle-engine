using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.registry;

public class BattleEngineRegistry : ExtensibleRegistry
{
	public BattleEngineRegistry(DataTagContext dataTags, Logger logger)
	{
		DataTags = dataTags;
		Logger = logger;
		DataDriven = new DataDrivenRegistry(dataTags, logger);
	}

	public DataTagContext DataTags { get; }
	public Logger Logger { get; }
	public IStateInitializer? StateInitializer { get; set; }
	public IGameOverPolicy? GameOverPolicy { get; set; }
	public IInputPolicy? InputPolicy { get; set; }
	public Registry<Type, IEntityBehaviourProvider> EntityBehaviourProviders { get; } = new();
	public HandlerRegistry<InputContext> InputHandlers { get; } = new();
	public HandlerRegistry<GameState> BlockHandlers { get; } = new();
	public HandlerRegistry<Transaction> EventHandlers { get; } = new();
	public DataDrivenRegistry DataDriven { get; }
	public RegistryFactory<ResourceId, IBehaviour> Behaviours { get; } = new();
	public RegistryFactory<ResourceId, EntityId, IEntity> Entities { get; } = new();
	public RegistryFactory<ResourceId, IEntityCondition> EntityConditions { get; } = new();
	public Registry<ResourceId, object> CustomContent {get;} = new(); // Other modded data content
}
