using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour;

public class TestAbility : SimpleBehaviour
{
	public override Type ListeningEventType => typeof(TurnEndedEvent);

	public override BehaviourResult Start(IEvent evnt, BehaviourContext context)
	{
		if (evnt is TurnEndedEvent turnEndedEvent)
		{
			if (context.Owner is GenericCard card)
			{
				context.CommitBlocks(
					new BlockBatch(
						[
							new SetCardDataBlock(context.OwnerEntityId, "hp", new IntTag(1000)),
							new SetCardDataBlock(context.OwnerEntityId, "atk", new IntTag(2000))
						]
					));
				context.CommitStagedBlocks();
			}
		}
		return BehaviourResult.Complete;
	}
}