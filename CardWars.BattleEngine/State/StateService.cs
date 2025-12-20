using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.State;

public class StateService : Service
{
	// Entity States
	public Dictionary<AbilityId, Ability> Abilities { get; set; } = [];
	public Dictionary<BattlefieldId, Battlefield> Battlefields { get; set; } = [];
	public Dictionary<DeckId, Deck> Decks { get; set; } = [];
	public Dictionary<PlayerId, Player> Players { get; set; } = [];
	public Dictionary<SpellCardId, SpellCard> SpellCards { get; set; } = [];
	public Dictionary<UnitCardId, UnitCard> UnitCards { get; set; } = [];
	public Dictionary<UnitSlotId, UnitSlot> UnitSlots { get; set; } = [];

	// Global States
	// Turns
	public List<PlayerId> TurnOrder { get; set; } = [];
	public HashSet<PlayerId> AllowedPlayerInputs { get; set; } = [];
	public int TurnIndex { get; set; } = 0;
	public TurnPhase TurnPhase { get; set; } = TurnPhase.None;
	public PlayerId? CurrentPlayerId => GetPlayerIdByTurnIndex(TurnIndex);

	public StateService(IServiceContainer container) : base(container)
	{
		container.Mapping.Register<Ability>();
		container.Mapping.Register<Battlefield>();
		container.Mapping.Register<Deck>();
		container.Mapping.Register<Player>();
		container.Mapping.Register<SpellCard>();
		container.Mapping.Register<UnitCard>();
		container.Mapping.Register<UnitSlot>();
	}

	public PlayerId? GetPlayerIdByTurnIndex(int turnIndex)
	{
		if (TurnIndex < TurnOrder.Count && TurnIndex >= 0 && TurnOrder.Count > 0) { return TurnOrder[turnIndex]; }
		return null;
	}
}

public enum TurnPhase { None = 0, Setup, Attacking }