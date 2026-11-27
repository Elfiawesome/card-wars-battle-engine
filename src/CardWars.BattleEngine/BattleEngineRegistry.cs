using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine;

public class BattleEngineRegistry : ExtensibleRegistry
{
	public HandlerRegistry<InputContext> InputHandlers { get; } = new();
	public HandlerRegistry<GameState> BlockHandlers { get; } = new();
	public HandlerRegistry<Transaction> EventHandlers { get; } = new();
	public DataDrivenRegistry DataDriven { get; } = new();
	public RegistryFactory<ResourceId, IBehaviour> Behaviours { get; } = new();
	public RegistryFactory<ResourceId, EntityId, IEntity> Entities { get; } = new();
	public RegistryFactory<ResourceId, IEntityCondition> EntityConditions { get; } = new();
	public Registry<ResourceId, object> CustomContent {get;} = new(); // Other modded data content
}
