using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Event.Player;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Resolver.Player;

public class DrawCardResolver(DrawCardEvent evnt) : EventResolver<DrawCardEvent>(evnt)
{
	public override void HandleStart()
	{
		if (ServiceContainer == null) { CommitResolved(); return; }

		var batch = Open();

		// Get Card
		if (ServiceContainer.State.Decks.TryGetValue(Event.DeckId, out var deck))
		{
			// Maybe raise a rng thing event here
			// Get from deck.DefinitionIds

			// Let's just create a generic card
			UnitCardId unitCardId = new(Guid.NewGuid());
			batch.Blocks.Add(new InstantiateUnitCardBlock(unitCardId));

			batch.Blocks.Add(new ModifyUnitCardHpNewBlock(unitCardId,
				new StatLayer()
				{
					Name = "Base",
					Value = 10,
					MaxValue = 10,
				}
			));
			batch.Blocks.Add(new AttachUnitCardToPlayerBlock(
				unitCardId,
				Event.PlayerId
			));
		}

		CommitResolved();
	}
}