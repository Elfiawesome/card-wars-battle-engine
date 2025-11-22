namespace CardWars.BattleEngine.Event.Turn;

public class EndPhaseTurnEvent : IEvent
{
	public TurnService.PhaseType PhaseType = TurnService.PhaseType.Setup;
}