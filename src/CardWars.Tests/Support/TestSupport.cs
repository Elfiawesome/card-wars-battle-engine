using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;

namespace CardWars.Tests.Support;

internal static class TestSupport
{
	public static BattleEngineRegistry CreateRegistry()
	{
		var registry = new BattleEngineRegistry();
		new VanillaMod().OnLoad(registry, []);
		return registry;
	}

	public static BehaviourContext CreateBehaviourContext(BattleEngineRegistry registry, GameState state, EntityId owner)
	{
		var transaction = new Transaction { Registry = registry, State = state };
		return new BehaviourContext(transaction, state, owner);
	}

	public static ScriptRuntimeContext CreateRuntime(BattleEngineRegistry registry, GameState state, EntityId owner)
	{
		var context = CreateBehaviourContext(registry, state, owner);
		return new ScriptRuntimeContext(context, registry.DataDriven);
	}

	public static TestEntity AddEntity(GameState state, int hp = 10)
	{
		var entity = new TestEntity { Id = EntityId.New(), Hp = hp };
		state.Add(entity);
		return entity;
	}
}
