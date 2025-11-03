using CardWars.BattleEngine.Block.Turn;
using CardWars.BattleEngine.Entity;

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
		}
		PlayerId? forecastedPlayerId = engine.TurnService.GetPlayerByTurnNumber(forecastedTurn);

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