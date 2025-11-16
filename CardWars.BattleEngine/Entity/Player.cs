namespace CardWars.BattleEngine.Entity;

public class Player(EntityService service, PlayerId id) : Entity<PlayerId>(service, id)
{
	public string Name { get; set; } = "Default Name";
	public HashSet<BattlefieldId> ControllingBattlefieldIds { get; set; } = [];
	public DeckId UnitDeckId { get; set; }
	public DeckId SpellDeckId { get; set; }
}

public record struct PlayerId(Guid Id);