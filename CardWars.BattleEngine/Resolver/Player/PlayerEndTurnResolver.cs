using CardWars.BattleEngine.Block.Turn;
using CardWars.BattleEngine.Event.Turn;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Resolver.Player;

public class PlayerEndTurnResolver : Resolver
{
	public override void HandleStart()
	{
		if (ServiceContainer == null) { CommitResolved(); return; }
		int forecastedTurn = ServiceContainer.State.TurnIndex + 1;
		TurnPhase forecastedPhase = ServiceContainer.State.TurnPhase;
		bool isPhasedChanged = false;

		// If we finish the last player of the phase
		if (forecastedTurn >= ServiceContainer.State.TurnOrder.Count)
		{
			forecastedTurn = 0;
			forecastedPhase = (forecastedPhase == TurnPhase.Setup)
							? TurnPhase.Attacking
							: TurnPhase.Setup;
			isPhasedChanged = true;

			// RAISE EVENT HERE
			ServiceContainer.EventService.Raise(new EndPhaseEvent() { Phase = forecastedPhase });
		}


		// Get the id of the forecasted player to put it in the allowed player
		PlayerId? forecastedPlayerId = ServiceContainer.State.GetPlayerIdByTurnIndex(forecastedTurn);

		if (forecastedPlayerId == null)
		{
			// Probably some error happened if the player id is null. For now, we just continously end turn
			ServiceContainer.Resolver.QueueResolver(new EndTurnResolver());
		}
		else
		{
			ServiceContainer.EventService.Raise(new EndTurnEvent()
			{
				Turn = forecastedTurn,
				Phase = forecastedPhase,
				PlayerId = (PlayerId)forecastedPlayerId
			});
			Open().Blocks.AddRange([
				new SetTurnIndexBlock(forecastedTurn, forecastedPhase, isPhasedChanged),
				new AddAllowedPlayerInputsBlock((PlayerId)forecastedPlayerId, true)
			]);
		}
		CommitResolved();
	}
}