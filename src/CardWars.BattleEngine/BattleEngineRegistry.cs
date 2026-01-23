using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class BattleEngineRegistry
{
	public HandlerRegistry<Transaction> InputHandlers = new();
	public HandlerRegistry<GameState> BlockHandlers = new();
	public HandlerRegistry<Transaction> EventHandlers = new();
	public RegistryFactory<ResourceId, IBehaviour> Behaviours { get; } = new();
}
