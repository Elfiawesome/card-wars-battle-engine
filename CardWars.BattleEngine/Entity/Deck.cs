namespace CardWars.BattleEngine.Entity;

public class Deck(EntityService service, BattlefieldId id) : Entity<BattlefieldId>(service, id)
{
	// Holds the id (probably something else rather than string) of the prefab definition
	public List<string> DefinitionIds { get; set; } = [];
}

public record struct DeckId(Guid Id);