using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Vanilla.Features;

namespace CardWars.BattleEngine.Vanilla.Behaviour;

public class SummonUnitCardToUnitSlotBehaviour : Behaviour<UseCardRequestEvent>
{
	public override int Priority => 0;

	protected override BehaviourResult Start(UseCardRequestEvent evnt, BehaviourContext context)
	{
		// Only care if its my behaviour that I'm listening to
		if (evnt.CardId == context.OwnerEntityId)
		{
			Console.WriteLine("The unit card that is being summoned to is " + context.OwnerEntityId);

		}

		return BehaviourResult.Complete;
	}
}