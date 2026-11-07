using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Tests.Support;

internal static class TestSupport
{
	public static BattleEngineRegistry CreateRegistry()
	{
		var registry = new BattleEngineRegistry();
		new VanillaMod().OnLoad(registry, []);
		registry.RuleDefinitions.Register(GameRulesDefinition.DefaultId, DataTagMapper.ToTag(new GameRulesDefinition
		{
			PtSchedule = [5, 6, 8, 11, 15, 20, 26, 33, 41, 50],
			PtCap = 50
		}));
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

	public static CompoundTag AttackPointer(CompoundTag definition)
		=> new CompoundTag().Set("definition", definition);

	public static CompoundTag AttackPointer(ResourceId id)
		=> new CompoundTag().Set("resource", id.ToString());

	public static TestEntity AddEntity(GameState state, int hp = 10)
	{
		var entity = new TestEntity { Id = EntityId.New(), Hp = hp };
		state.Add(entity);
		return entity;
	}
}
