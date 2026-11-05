using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using Xunit;

namespace CardWars.Tests;

public sealed class CardTargetFilterTests
{
	[Fact]
	public void UnitDefaultFilter_AllowsOnlyOwnEmptySlots()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var player = AddPlayer(state, out _, out var firstSlot, out var secondSlot);
		AddPlayer(state, out _, out var opponentSlot, out _);
		var card = AddFilteredCard(state, player, UnitTargetFilter.Create());

		var evaluator = new CardTargetFilterEvaluator(registry.DataDriven);
		var targets = evaluator.ResolveTargets(state, card, player.Id);

		Assert.Contains(firstSlot.Id, targets);
		Assert.Contains(secondSlot.Id, targets);
		Assert.DoesNotContain(opponentSlot.Id, targets);
		Assert.DoesNotContain(card.Id, targets);

		Assert.True(evaluator.CanTarget(state, card, player.Id, firstSlot.Id));
		Assert.False(evaluator.CanTarget(state, card, player.Id, opponentSlot.Id));
		Assert.False(evaluator.CanTarget(state, card, player.Id, card.Id));
		Assert.False(evaluator.CanTarget(state, card, player.Id, null));

		var occupant = new GenericCard(EntityId.New());
		state.Add(occupant);
		firstSlot.HoldingCardId = occupant.Id;

		targets = evaluator.ResolveTargets(state, card, player.Id);
		Assert.DoesNotContain(firstSlot.Id, targets);
		Assert.Contains(secondSlot.Id, targets);
		Assert.False(evaluator.CanTarget(state, card, player.Id, firstSlot.Id));
	}

	[Fact]
	public void CustomFilter_CanUseCandidatePlayerAndType()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var player = AddPlayer(state, out _, out var playerSlot, out _);
		AddPlayer(state, out _, out var opponentSlot, out _);
		var card = AddFilteredCard(state, player, OwnUnitSlotFilter());

		var evaluator = new CardTargetFilterEvaluator(registry.DataDriven);
		var targets = evaluator.ResolveTargets(state, card, player.Id);

		Assert.Contains(playerSlot.Id, targets);
		Assert.DoesNotContain(opponentSlot.Id, targets);
		Assert.True(evaluator.CanTarget(state, card, player.Id, playerSlot.Id));
		Assert.False(evaluator.CanTarget(state, card, player.Id, opponentSlot.Id));
		Assert.False(evaluator.CanTarget(state, card, player.Id, card.Id));
	}

	[Fact]
	public void MissingFilter_MeansNoEntityTarget()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var player = AddPlayer(state, out _, out var slot, out _);
		var card = new GenericCard(EntityId.New()) { OwnerPlayerId = player.Id };
		state.Add(card);

		var evaluator = new CardTargetFilterEvaluator(registry.DataDriven);

		Assert.Empty(evaluator.ResolveTargets(state, card, player.Id));
		Assert.True(evaluator.CanTarget(state, card, player.Id, null));
		Assert.False(evaluator.CanTarget(state, card, player.Id, slot.Id));
	}

	[Fact]
	public void MalformedFilter_IsTreatedAsMissing()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var player = AddPlayer(state, out _, out var slot, out _);
		var card = new GenericCard(EntityId.New()) { OwnerPlayerId = player.Id };
		card.Data.Set("target_filter", new ListTag());
		state.Add(card);

		var evaluator = new CardTargetFilterEvaluator(registry.DataDriven);

		Assert.Empty(evaluator.ResolveTargets(state, card, player.Id));
		Assert.True(evaluator.CanTarget(state, card, player.Id, null));
		Assert.False(evaluator.CanTarget(state, card, player.Id, slot.Id));
	}

	[Fact]
	public void CanTarget_ReturnsFalseForMissingEntity()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var player = AddPlayer(state, out _, out _, out _);
		var card = AddFilteredCard(state, player, UnitTargetFilter.Create());

		var evaluator = new CardTargetFilterEvaluator(registry.DataDriven);

		Assert.False(evaluator.CanTarget(state, card, player.Id, EntityId.New()));
	}

	[Fact]
	public void UnitCardRegistration_InjectsDefaultTargetFilter()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("cards/units/test_unit.json", """{"name":"Test Unit","hp":4,"atk":2}""");

		var registry = new BattleEngineRegistry();
		var content = new ModContentResult
		{
			Id = ResourceId.Vanilla("cards/units/test_unit"),
			FilePath = new StoragePath("cards/units/test_unit.json", provider),
			Category = ["cards", "units"]
		};

		new VanillaMod().OnLoad(registry, [content]);

		var definition = registry.CardDefinitions.Get(content.Id) ?? throw new InvalidOperationException("Unit definition was not registered");
		Assert.True(definition.Has("target_filter"));

		var state = new GameState();
		var player = AddPlayer(state, out _, out _, out _);
		var card = new GenericCard(EntityId.New()) { OwnerPlayerId = player.Id, Data = (CompoundTag)definition.Clone() };
		state.Add(card);

		var evaluator = new CardTargetFilterEvaluator(registry.DataDriven);
		Assert.NotEmpty(evaluator.ResolveTargets(state, card, player.Id));
	}

	[Fact]
	public void ScriptContext_EvaluatesExpressionsWithoutABehaviour()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var context = new ScriptContext(state, registry.DataDriven);

		var result = context.Evaluate(new AddExpression(
			new LiteralExpression(new IntTag(2)),
			new LiteralExpression(new IntTag(3))));

		Assert.Equal(5, Assert.IsType<IntTag>(result).Value);
	}

	private static IExpression OwnUnitSlotFilter()
	{
		var candidate = new VariableExpression(TargetBindings.Candidate);
		var player = new VariableExpression(TargetBindings.Player);
		var state = new VariableExpression(BattleEngine.Behaviour.DataDriven.ScriptBindings.State);

		var isUnitSlot = new IsEntityTypeExpression(
			candidate,
			ResourceId.Vanilla("battle/entity/unit_slot"));

		var ownBattlefield = new EqualsExpression(
			new MemberExpression(candidate, "owner_battlefield_id"),
			new IndexExpression(
				new MemberExpression(player, "battlefield_ids"),
				new LiteralExpression(new IntTag(0))));

		return new AndExpression(isUnitSlot, ownBattlefield);
	}

	private static Player AddPlayer(
		GameState state,
		out Battlefield battlefield,
		out UnitSlot firstSlot,
		out UnitSlot secondSlot)
	{
		var player = new Player(EntityId.New());
		battlefield = new Battlefield(EntityId.New());
		firstSlot = new UnitSlot(EntityId.New()) { OwnerBattlefieldId = battlefield.Id };
		secondSlot = new UnitSlot(EntityId.New()) { OwnerBattlefieldId = battlefield.Id };

		player.BattlefieldIds.Add(battlefield.Id);
		battlefield.UnitSlotIds.Add(firstSlot.Id);
		battlefield.UnitSlotIds.Add(secondSlot.Id);

		state.Add(player);
		state.Add(battlefield);
		state.Add(firstSlot);
		state.Add(secondSlot);
		return player;
	}

	private static GenericCard AddFilteredCard(GameState state, Player owner, IExpression filter)
	{
		var card = new GenericCard(EntityId.New()) { OwnerPlayerId = owner.Id };
		card.Data.Set("target_filter", DataTagMapper.ToTag(filter));
		state.Add(card);
		return card;
	}
}
