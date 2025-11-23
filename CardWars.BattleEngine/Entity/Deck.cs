namespace CardWars.BattleEngine.Entity;

public class Deck(EntityService service, DeckId id) : Entity<DeckId>(service, id)
{
	public Dictionary<DeckPosDefinitionId, string> DefinitionIds { get; set; } = [];
}

public record struct DeckId(Guid Id);
// TODO: Probably need a better name than `DeckPosDefinitionId`
public record struct DeckPosDefinitionId(Guid Id);