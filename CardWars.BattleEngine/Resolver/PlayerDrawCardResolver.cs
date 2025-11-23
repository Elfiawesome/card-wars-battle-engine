using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Resolver;

public class PlayerDrawCardResolver(PlayerId playerId, DeckId deckId) : ResolverBase
{
	public PlayerId PlayerId = playerId;
	public DeckId DeckId = deckId;

	public override void HandleStart(BattleEngine engine)
	{
		if (!engine.EntityService.Decks.TryGetValue(DeckId, out var deck)) { Resolved(); return; }
		if (deck == null) { Resolved(); return; }

		// Get Random Card
		if (deck.DefinitionIds.Count < 1) { Resolved(); return; }
		var rand = new Random();
		var item = deck.DefinitionIds.ElementAt(rand.Next(0, deck.DefinitionIds.Count));

		// Retrieve definition
		var deckPosDefinitionId = item.Key;
		if (!engine.DefinitionService.UnitDefinitions.TryGetValue(item.Value, out var deckDefinition)) { Resolved(); return; }

		// Create Batch
		var unitCardId = new UnitCardId(Guid.NewGuid());
		AddBlockBatch(new([
			new ModifyDeckRemoveBlock(DeckId, deckPosDefinitionId),
			new InstantiateUnitCardBlock(unitCardId),
			new ModifyUnitCardSetBlock(unitCardId){
				Name = deckDefinition.Name,
				FlavourText = deckDefinition.FlavorText,
				PointCost = deckDefinition.BasePt,
				Hp = deckDefinition.BaseHp,
				Atk = deckDefinition.BaseAtk
			}
		]));
		CommitResolved();
	}
}