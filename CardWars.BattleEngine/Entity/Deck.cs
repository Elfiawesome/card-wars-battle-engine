namespace CardWars.BattleEngine.Entity;

public class Deck(EntityService service, DeckId id) : Entity<DeckId>(service, id)
{
	// Holds the id (probably something else rather than string) of the prefab definition
	public Dictionary<Guid, string> DefinitionIds { get; set; } = [];
}

public record struct DeckId(Guid Id);