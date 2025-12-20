namespace CardWars.BattleEngine.State.Entity;

public class Deck(DeckId id) : EntityState<DeckId>(id)
{
	// We just store what type of cards wether it is spell or units in this deck. we will figure out what type
	// of card it actually is when we draw it :)
	public DeckType DeckType { get; set; } = DeckType.Unit;
	public Dictionary<DeckDefinitionIdKey, string> DefinitionIds { get; set; } = [];
	public PlayerId OwnerPlayerId { get; set; }
}

public enum DeckType { Unit = 0, Spell }

public record struct DeckDefinitionIdKey(Guid Id) : IStateId;
public record struct DeckId(Guid Id) : IStateId;