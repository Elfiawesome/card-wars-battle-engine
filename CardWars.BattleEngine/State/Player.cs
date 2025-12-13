namespace CardWars.BattleEngine.State;

public class Player(PlayerId id) : EntityState<PlayerId>(id)
{
	public HashSet<BattlefieldId> ControllingBattlefieldIds { get; set; } = [];
	public Dictionary<DeckType, HashSet<DeckId>> ControllingDecks { get; set; } = [];

	public Dictionary<PlayerHandId, UnitCardId> HandUnitCards  = [];
	public Dictionary<PlayerHandId, SpellCardId> HandSpellCards  = [];
	public List<PlayerHandId> HandOrder = [];
}

public record struct PlayerId(Guid Id);
public record struct PlayerHandId(Guid Id);