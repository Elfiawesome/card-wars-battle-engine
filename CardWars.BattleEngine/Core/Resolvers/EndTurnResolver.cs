
using CardWars.BattleEngine.Core.Actions.ActionHandlers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public class EndTurnResolver() : Resolver
{
	public override void Resolve(GameState state)
	{
		AddActionBatch(new([
			new AdvanceTurnOrderData()
		]));
		CommitResolve();
	}
}