using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Resolver;
using CardWars.BattleEngine.State.Component;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Feature.DrawCard;

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
			batch.AddBlock(new InstantiateUnitCardBlock(unitCardId));

			// batch.Blocks.Add( Block that edits the unit card stats );
			batch.AddBlock(new ModifyUnitCardCompositeIntStatAddBlock(
				unitCardId,
				"hp",
				new StatLayer() { Name = "Base", Value = 8, MaxValue = 8 }
			));
			batch.AddBlock(new ModifyUnitCardCompositeIntStatAddBlock(
				unitCardId,
				"atk",
				new StatLayer() { Name = "Base", Value = 5, MaxValue = 5 }
			));
			batch.AddBlock(new ModifyUnitCardCompositeIntStatAddBlock(
				unitCardId,
				"pt",
				new StatLayer() { Name = "Base", Value = 3, MaxValue = 3 }
			));

			batch.AddBlock(new AttachUnitCardToPlayerBlock(unitCardId, Event.PlayerId));
		}
		CommitResolved();
	}
}