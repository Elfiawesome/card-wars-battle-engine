namespace CardWars.BattleEngine.State;

public class Player(PlayerId id) : EntityState<PlayerId>(id)
{
	public HashSet<BattlefieldId> ControllingBattlefieldIds { get; set; } = [];
	public Dictionary<DeckType, HashSet<DeckId>> ControllingDecks { get; set; } = [];

	public Dictionary<PlayerHandId, UnitCardId> HandUnitCards { get; set; } = [];
	public Dictionary<PlayerHandId, SpellCardId> HandSpellCards { get; set; } = [];
	public List<PlayerHandId> HandOrder { get; set; } = [];
}

public record struct PlayerId(Guid Id) : EntityId;
public record struct PlayerHandId(Guid Id) : EntityId;