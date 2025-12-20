namespace CardWars.BattleEngine.State.Entity;

public class Player(PlayerId id) : EntityState<PlayerId>(id)
{
	public HashSet<BattlefieldId> ControllingBattlefieldIds { get; set; } = [];
	public Dictionary<DeckType, HashSet<DeckId>> ControllingDecks { get; set; } = [];

	public List<ICardId> HandCards { get; set; } = [];
}

public record struct PlayerId(Guid Id) : IStateId;