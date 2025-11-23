namespace CardWars.BattleEngine.Entity;

public class Player(EntityService service, PlayerId id) : Entity<PlayerId>(service, id)
{
	public string Name { get; set; } = "Default Name";
	public HashSet<BattlefieldId> ControllingBattlefieldIds { get; set; } = [];
	public DeckId UnitDeckId { get; set; }
	public DeckId SpellDeckId { get; set; }


	public Dictionary<PlayerHandId, UnitCardId> HandUnitCards = [];
	public Dictionary<PlayerHandId, SpellCardId> HandSpellCards = [];
	public List<PlayerHandId> HandOrder = [];
}

public record struct PlayerHandId(Guid Id);
public record struct PlayerId(Guid Id);