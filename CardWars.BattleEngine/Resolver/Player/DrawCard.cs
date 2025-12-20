using System.Dynamic;
using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Event.Player;
using CardWars.BattleEngine.State.Entity;

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

			// batch.Blocks.Add( Block that edits the unit card stats );
		}

		CommitResolved();
	}
}