using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
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
		var rules = GameRulesDefinition.Get(context.Registry, state.RuleId) ?? new GameRulesDefinition();
		var result = new WinConditionEvaluator(context.Registry).Evaluate(state, rules);
		if (!result.IsGameOver) return;

		GameOutcome outcome;
		if (result.IsDraw)
		{
			outcome = GameOutcome.Draw();
		}
		else if (result.WinningTeam is { } winningTeam)
		{
			outcome = GameOutcome.Win(winningTeam);
		}
		else
		{
			return;
		}

		context.ApplyBlockBatch(new BlockBatch([
			new SetGameOutcomeBlock(outcome)
		]));

		context.QueueEvent(new GameEndedEvent() { Outcome = outcome });
	}
}

[DataTagType()]
public class GameEndedEvent : IEvent
{
	[DataTag] public required GameOutcome Outcome { get; set; }
}

public class GameEndedEventHandler : IEventHandler<GameEndedEvent>
{
	public void Handle(Transaction context, GameEndedEvent request)
	{
	}
}
