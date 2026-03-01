using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.Data;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class BattleEngineRegistry
{
	public HandlerRegistry<InputContext> InputHandlers = new();
	public HandlerRegistry<GameState> BlockHandlers = new();
	public HandlerRegistry<Transaction> EventHandlers = new();
	public RegistryFactory<ResourceId, IBehaviour> Behaviours { get; } = new();
	public RegistryFactory<ResourceId, EntityId, IEntity> Entities { get; } = new();

	public Registry<ResourceId, CardDefinition> CardContent { get; } = new();
	public Registry<ResourceId, object> CustomContent {get;} = new(); // Other modded data content
}
