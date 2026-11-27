using CardWars.Vanilla.Shared;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.State;
using CardWars.Core.Data.Tags;
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
		state.Ext.GetOrCreate<CardWarsBattleState>().Turn.TurnOrder.Add(player);
		state.Ext.GetOrCreate<CardWarsBattleState>().Turn.AllowedPlayerInputs.Add(player);

		var snapshot = state.ToSnapshot(TestBootstrap.DataTags);
		var restored = new GameState();
		restored.FromSnapshot(snapshot, TestBootstrap.DataTags);
		restored.Ext.GetOrCreate<CardWarsBattleState>().Turn.TurnOrder.Clear();
		restored.Ext.GetOrCreate<CardWarsBattleState>().Turn.AllowedPlayerInputs.Clear();

		Assert.Single(state.Ext.GetOrCreate<CardWarsBattleState>().Turn.TurnOrder);
		Assert.Single(state.Ext.GetOrCreate<CardWarsBattleState>().Turn.AllowedPlayerInputs);
	}

	[Fact]
	public void Clear_ResetsTurnState()
	{
		var state = new GameState();
		state.Ext.GetOrCreate<CardWarsBattleState>().Turn.TurnOrder.Add(EntityId.New());
		state.Ext.GetOrCreate<CardWarsBattleState>().Turn.AllowedPlayerInputs.Add(EntityId.New());

		state.Clear();

		Assert.Empty(state.Ext.GetOrCreate<CardWarsBattleState>().Turn.TurnOrder);
		Assert.Empty(state.Ext.GetOrCreate<CardWarsBattleState>().Turn.AllowedPlayerInputs);
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
		state.Ext.GetOrCreate<CardWarsBattleState>().RuleId = CardWarsIds.Vanilla("rules/custom");
		state.Ext.GetOrCreate<CardWarsBattleState>().Outcome = GameOutcome.Win(2);
		state.Ext.GetOrCreate<CardWarsBattleState>().Turn.RoundNumber = 4;

		var tag = TestBootstrap.DataTags.ToTag(state.ToSnapshot(TestBootstrap.DataTags));
		var snapshot = TestBootstrap.DataTags.FromTag<GameStateSnapshot>(tag);
		var restored = new GameState();
		restored.FromSnapshot(snapshot, TestBootstrap.DataTags);

		Assert.Equal(state.Ext.GetOrCreate<CardWarsBattleState>().RuleId, restored.Ext.GetOrCreate<CardWarsBattleState>().RuleId);
		Assert.True(restored.Ext.GetOrCreate<CardWarsBattleState>().Outcome.IsGameOver);
		Assert.Equal(2, restored.Ext.GetOrCreate<CardWarsBattleState>().Outcome.WinningTeam);
		Assert.False(restored.Ext.GetOrCreate<CardWarsBattleState>().Outcome.IsDraw);
		Assert.Equal(4, restored.Ext.GetOrCreate<CardWarsBattleState>().Turn.RoundNumber);
	}
}
