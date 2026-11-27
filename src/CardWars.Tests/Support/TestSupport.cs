using CardWars.Vanilla.Shared;
using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.registry;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Tests.Support;

internal static class TestSupport
{
	public static BattleEngineRegistry CreateRegistry()
	{
		var registry = new BattleEngineRegistry(TestBootstrap.DataTags, TestBootstrap.Logger);
		new VanillaMod().OnLoad(registry, []);
		registry.EntityBehaviourProviders.Register(typeof(TestEntity), new TestBehaviourProvider());
		var definitions = Definitions(registry);
		definitions.DefaultUnitTargetFilter = TestTargetFilters.CreateUnitTargetFilter();
		definitions.RuleDefinitions.Register(GameRulesDefinition.DefaultId, TestBootstrap.DataTags.ToTag(new GameRulesDefinition
		{
			PtSchedule = [5, 6, 8, 11, 15, 20, 26, 33, 41, 50],
			PtCap = 50,
			UnitSlots =
			[
				new UnitSlotPos(-1, 0),
				new UnitSlotPos(0, 0),
				new UnitSlotPos(1, 0),
				new UnitSlotPos(0, -1)
			],
			IntroductionAnimation = CardWarsIds.Vanilla("battle/animation/player_introduction_animation")
		}));
		return registry;
	}

	public static CardWarsDefinitions Definitions(BattleEngineRegistry registry)
		=> registry.GetExtension<CardWarsDefinitions>()
			?? throw new InvalidOperationException("CardWars definitions are not registered.");

	public static PlayerBattleSetup DefaultBattleSetup() => new()
	{
		Deck =
		[
			TestCards.CobCannon,
			TestCards.ApocalypseTank,
			TestCards.AssaultDestroyer,
			TestCards.Fastpass,
			TestCards.Auditor
		],
		Heroes = [TestCards.CaptainHero]
	};

	public static BehaviourContext CreateBehaviourContext(BattleEngineRegistry registry, GameState state, EntityId owner)
	{
		var transaction = new Transaction(registry, state);
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


internal sealed class TestBehaviourProvider : IEntityBehaviourProvider
{
	public IReadOnlyList<ContentPointer> GetBehaviours(IEntity entity)
		=> entity is TestEntity testEntity ? testEntity.Behaviours : [];
}
