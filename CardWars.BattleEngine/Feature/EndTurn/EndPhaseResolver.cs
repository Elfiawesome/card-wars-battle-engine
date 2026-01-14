using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine.Feature.EndTurn;

public class EndPhaseResolver(EndPhaseEvent evnt) : EventResolver<EndPhaseEvent>(evnt)
{
	public override void HandleStart()
	{
		CommitResolved();
	}
}