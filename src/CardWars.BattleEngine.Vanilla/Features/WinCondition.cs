using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class WinConditionCheckEvent : IEvent
{
	[DataTag] public required TurnState TurnState { get; set; }
}

public class WinConditionCheckEventHandler : IEventHandler<WinConditionCheckEvent>
{
	public void Handle(Transaction context, WinConditionCheckEvent request)
	{
		var state = context.State;
		var rules = context.Registry.GameRules.Get(state.RuleId) ?? new GameRulesDefinition();
		var result = new WinConditionEvaluator(context.Registry).Evaluate(state, rules);

		if (result.IsGameOver)
		{
			var turnState = request.TurnState.Clone();
			turnState.AllowedPlayerInputs.Clear();

			context.ApplyBlockBatch(new BlockBatch([
				new UpdateTurnStateBlock(turnState),
				new SetGameOutcomeBlock(true, result.WinningTeam, result.IsDraw)
			]));

			context.QueueEvent(new GameEndedEvent()
			{
				WinningTeam = result.WinningTeam,
				IsDraw = result.IsDraw
			});
			return;
		}

		foreach (var card in state.OfType<GenericCard>())
		{
			if (!card.IsUnit || card.OwnerUnitSlotId is null) continue;

			context.QueueEvent(new ChargeRefillEvent()
			{
				CardId = card.Id,
				Amount = card.ChargeMax
			});
		}

		var nextTurnState = request.TurnState.Clone();
		nextTurnState.TurnOrder.RemoveAll(playerId => state.Get<Player>(playerId) is { IsDefeated: true });
		nextTurnState.TurnIndex = 0;
		nextTurnState.AllowedPlayerInputs.Clear();
		if (nextTurnState.TurnOrder.Count > 0)
			nextTurnState.AllowedPlayerInputs.Add(nextTurnState.TurnOrder[0]);

		context.QueueEvent(new EndTurnEvent() { TurnState = nextTurnState, PhaseChanged = true });

		var targetPt = rules.GetPtForRound(nextTurnState.RoundNumber);
		foreach (var player in state.OfType<Player>())
		{
			if (player.IsDefeated) continue;

			context.QueueEvent(new PtReplenishEvent()
			{
				PlayerId = player.Id,
				TargetPt = targetPt
			});
		}
	}
}

[DataTagType()]
public class GameEndedEvent : IEvent
{
	[DataTag] public int? WinningTeam { get; set; }
	[DataTag] public bool IsDraw { get; set; }
}

public class GameEndedEventHandler : IEventHandler<GameEndedEvent>
{
	public void Handle(Transaction context, GameEndedEvent request)
	{
	}
}
