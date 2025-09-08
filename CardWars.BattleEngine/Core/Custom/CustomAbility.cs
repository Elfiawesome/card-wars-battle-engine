using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.Events.EventContexts;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Custom;

public class CustomAbility(GameState gameState, AbilityId id) : AbilityState(gameState, id)
{
	private class CustomResolver : Resolver
	{
		public override void Resolve(GameState state)
		{
			AddActionBatch(new([
			]));
			CommitResolve();
		}
	}
	private class CustomSubscriber : EventSubscriber<OnUnitSummonedEventContext, EventOutcome>
	{
		public override EventOutcome Raise(OnUnitSummonedEventContext context)
		{
			var outcome = new EventOutcome();
			Console.WriteLine("This Ability has been activated!");
			outcome.RaisedResolvers = [];
			return outcome;
		}
	}

	public override void Ready(GameState state, AbilityId id)
	{
		state.EventManager.OnUnitSummoned.Subscribe(new CustomSubscriber());
	}
}

