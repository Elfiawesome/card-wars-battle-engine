using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Event.Turn;

public record EndPhaseEvent : IEvent
{
	public TurnPhase Phase = TurnPhase.Setup;
}