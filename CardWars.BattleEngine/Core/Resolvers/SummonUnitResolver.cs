using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public class SummonUnitResolver : Resolver
{
	public override void Resolve(GameState state, EventManager eventManager)
	{
		
		CommitResolve();
	}
}