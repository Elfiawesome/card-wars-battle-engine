using CardWars.BattleEngine.Block.Turn;
using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Event.Turn;

namespace CardWars.BattleEngine.Resolver;

public class EndTurnResolver() : ResolverBase
{
	public override void HandleStart(BattleEngine engine)
	{
		int forecastedTurn = engine.TurnService.TurnNumber += 1;
		TurnService.PhaseType forecastedPhase = engine.TurnService.Phase;
		bool isPhasedChanged = false;
		if (engine.TurnService.TurnNumber >= engine.TurnService.TurnOrder.Count)
		{
			forecastedTurn = 0;
			forecastedPhase = (forecastedPhase == TurnService.PhaseType.Setup)
								? TurnService.PhaseType.Attacking
								: TurnService.PhaseType.Setup;
			isPhasedChanged = true;
			RaiseEvent(new EndPhaseTurnEvent() { PhaseType = forecastedPhase });
		}
		PlayerId? forecastedPlayerId = engine.TurnService.GetPlayerByTurnNumber(forecastedTurn);

		RaiseEvent(new EndTurnEvent());
		if (forecastedPlayerId == null)
		{
			QueueResolver(new EndTurnResolver());
		}
		else
		{
			AddBlockBatch(new([
				new SetTurnIndexBlock(forecastedTurn, forecastedPhase, isPhasedChanged),
				new AddAllowedPlayerInputsBlock((PlayerId)forecastedPlayerId, true)
			]));
		}
		CommitResolved();
	}
}