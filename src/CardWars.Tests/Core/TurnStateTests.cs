using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.State;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class TurnStateTests
{
	[Fact]
	public void NewTurnState_HasEmptyCollections()
	{
		var turn = new TurnState();

		Assert.Empty(turn.TurnOrder);
		Assert.Empty(turn.AllowedPlayerInputs);
		Assert.Equal(TurnPhase.Setup, turn.Phase);
		Assert.Equal(1, turn.RoundNumber);
	}

	[Fact]
	public void Clone_ProducesIndependentCollections()
	{
		var player = EntityId.New();
		var source = new TurnState([player], [player], 1, 2, TurnPhase.Attacking);

		var clone = source.Clone();
		clone.TurnOrder.Add(EntityId.New());
		clone.AllowedPlayerInputs.Clear();

		Assert.Single(source.TurnOrder);
		Assert.Single(source.AllowedPlayerInputs);
		Assert.Equal(TurnPhase.Attacking, source.Phase);
	}

	[Fact]
	public void Snapshot_ClonesTurnCollections()
	{
		var state = new GameState();
		var player = EntityId.New();
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.TurnOrder.Add(player);
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.AllowedPlayerInputs.Add(player);

		var snapshot = state.ToSnapshot(TestBootstrap.DataTags);
		var restored = new GameState();
		restored.FromSnapshot(snapshot, TestBootstrap.DataTags);
		restored.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.TurnOrder.Clear();
		restored.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.AllowedPlayerInputs.Clear();

		Assert.Single(state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.TurnOrder);
		Assert.Single(state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.AllowedPlayerInputs);
	}

	[Fact]
	public void Clear_ResetsTurnState()
	{
		var state = new GameState();
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.TurnOrder.Add(EntityId.New());
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.AllowedPlayerInputs.Add(EntityId.New());

		state.Clear();

		Assert.Empty(state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.TurnOrder);
		Assert.Empty(state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.AllowedPlayerInputs);
	}

	[Fact]
	public void TurnState_RoundTripsThroughDataTags()
	{
		var player = EntityId.New();
		var source = new TurnState([player], [player], 1, 3, TurnPhase.Attacking, 4);

		var tag = TestBootstrap.DataTags.ToTag(source);
		var json = TestBootstrap.DataTags.Serialize(tag);
		var restoredTag = TestBootstrap.DataTags.Deserialize<CompoundTag>(json) ?? new CompoundTag();
		var restored = TestBootstrap.DataTags.FromTag<TurnState>(restoredTag);

		Assert.Equal(source.TurnOrder, restored.TurnOrder);
		Assert.Equal(source.AllowedPlayerInputs, restored.AllowedPlayerInputs);
		Assert.Equal(source.TurnIndex, restored.TurnIndex);
		Assert.Equal(source.TurnNumber, restored.TurnNumber);
		Assert.Equal(source.RoundNumber, restored.RoundNumber);
		Assert.Equal(source.Phase, restored.Phase);
	}

	[Fact]
	public void Snapshot_PreservesOutcomeAndRuleId()
	{
		var state = new GameState();
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().RuleId = ResourceId.Vanilla("rules/custom");
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().Outcome = GameOutcome.Win(2);
		state.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.RoundNumber = 4;

		var tag = TestBootstrap.DataTags.ToTag(state.ToSnapshot(TestBootstrap.DataTags));
		var snapshot = TestBootstrap.DataTags.FromTag<GameStateSnapshot>(tag);
		var restored = new GameState();
		restored.FromSnapshot(snapshot, TestBootstrap.DataTags);

		Assert.Equal(state.Ext.GetOrCreateExtension<CardWarsBattleState>().RuleId, restored.Ext.GetOrCreateExtension<CardWarsBattleState>().RuleId);
		Assert.True(restored.Ext.GetOrCreateExtension<CardWarsBattleState>().Outcome.IsGameOver);
		Assert.Equal(2, restored.Ext.GetOrCreateExtension<CardWarsBattleState>().Outcome.WinningTeam);
		Assert.False(restored.Ext.GetOrCreateExtension<CardWarsBattleState>().Outcome.IsDraw);
		Assert.Equal(4, restored.Ext.GetOrCreateExtension<CardWarsBattleState>().Turn.RoundNumber);
	}
}
