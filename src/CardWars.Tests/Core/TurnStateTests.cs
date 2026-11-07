using CardWars.BattleEngine.State;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
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
		state.Turn.TurnOrder.Add(player);
		state.Turn.AllowedPlayerInputs.Add(player);

		var snapshot = state.ToSnapshot();
		snapshot.Turn.TurnOrder.Clear();
		snapshot.Turn.AllowedPlayerInputs.Clear();

		Assert.Single(state.Turn.TurnOrder);
		Assert.Single(state.Turn.AllowedPlayerInputs);
	}

	[Fact]
	public void Clear_ResetsTurnState()
	{
		var state = new GameState();
		state.Turn.TurnOrder.Add(EntityId.New());
		state.Turn.AllowedPlayerInputs.Add(EntityId.New());

		state.Clear();

		Assert.Empty(state.Turn.TurnOrder);
		Assert.Empty(state.Turn.AllowedPlayerInputs);
	}

	[Fact]
	public void TurnState_RoundTripsThroughDataTags()
	{
		var player = EntityId.New();
		var source = new TurnState([player], [player], 1, 3, TurnPhase.Attacking, 4);

		var tag = DataTagMapper.ToTag(source);
		var json = DataTagSerializer.Serialize(tag);
		var restoredTag = DataTagSerializer.Deserialize<CompoundTag>(json) ?? new CompoundTag();
		var restored = DataTagMapper.FromTag<TurnState>(restoredTag);

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
		var state = new GameState
		{
			RuleId = ResourceId.Vanilla("rules/custom"),
			Outcome = GameOutcome.Win(2)
		};
		state.Turn.RoundNumber = 4;

		var tag = DataTagMapper.ToTag(state.ToSnapshot());
		var restored = DataTagMapper.FromTag<GameStateSnapshot>(tag);

		Assert.Equal(state.RuleId, restored.RuleId);
		Assert.True(restored.Outcome.IsGameOver);
		Assert.Equal(2, restored.Outcome.WinningTeam);
		Assert.False(restored.Outcome.IsDraw);
		Assert.Equal(4, restored.Turn.RoundNumber);
	}
}
