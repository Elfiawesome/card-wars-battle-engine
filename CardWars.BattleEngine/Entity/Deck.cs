namespace CardWars.BattleEngine.Entity;

public class Deck(EntityService service, BattlefieldId id) : Entity<BattlefieldId>(service, id)
{
}

public record struct DeckId(Guid Id);