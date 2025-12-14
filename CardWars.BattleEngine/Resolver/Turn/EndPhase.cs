using CardWars.BattleEngine.Event.Turn;

namespace CardWars.BattleEngine.Resolver.Turn;

public class EndPhaseResolver(EndPhaseEvent evnt) : EventResolver<EndPhaseEvent>(evnt)
{
	public override void HandleStart()
	{
		CommitResolved();
	}
}