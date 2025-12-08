namespace CardWars.BattleEngine.Entity;

public class Deck(DeckId id) : Entity<DeckId>(id)
{
	public Dictionary<DeckPosDefinitionId, string> DefinitionIds { get; set; } = [];
}

public record struct DeckId(Guid Id);
// TODO: Probably need a better name than `DeckPosDefinitionId`
public record struct DeckPosDefinitionId(Guid Id);