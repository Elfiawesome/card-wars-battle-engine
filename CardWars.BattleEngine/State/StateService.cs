namespace CardWars.BattleEngine.State;

public class StateService(IServiceContainer container) : Service(container)
{
	// Entity States
	public Dictionary<AbilityId, Ability> Abilities = [];
	public Dictionary<BattlefieldId, Battlefield> Battlefields = [];
	public Dictionary<DeckId, Deck> Decks = [];
	public Dictionary<PlayerId, Player> Players = [];
	public Dictionary<SpellCardId, SpellCard> SpellCards = [];
	public Dictionary<UnitCardId, UnitCard> UnitCards = [];
	public Dictionary<UnitSlotId, UnitSlot> UnitSlots = [];

	// Global States
	// Turns
	public List<PlayerId> TurnOrder = [];
	public HashSet<PlayerId> AllowedPlayerInputs = [];
	public int TurnIndex = 0;
	public TurnPhase TurnPhase = TurnPhase.None;
	public PlayerId? CurrentPlayerId => GetPlayerIdByTurnIndex(TurnIndex);

	public PlayerId? GetPlayerIdByTurnIndex(int turnIndex)
	{
		if (TurnIndex < TurnOrder.Count && TurnIndex >= 0 && TurnOrder.Count > 0) { return TurnOrder[turnIndex]; }
		return null;
	}

	public void PrintSnapshot()
	{
		Console.WriteLine("         [Snapshot]");
		Console.WriteLine("  --- State Informaton --- ");
		Console.WriteLine("> Players:");
		ServiceContainer.State.Players.ToList().ForEach((s) =>
		{
			Console.WriteLine("  - " + s.Key.Id);
		});

		Console.WriteLine("> Battlefields:");
		ServiceContainer.State.Battlefields.ToList().ForEach((s) =>
		{
			Console.WriteLine("  - " + s.Key.Id);
		});

		Console.WriteLine("> Unit Slots:");
		ServiceContainer.State.UnitSlots.ToList().ForEach((s) =>
		{
			Console.WriteLine("  - " + s.Key.Id);
		});

		Console.WriteLine("> Decks:");
		ServiceContainer.State.Decks.ToList().ForEach((s) =>
		{
			Console.WriteLine("  - " + s.Key.Id);
		});

		Console.WriteLine("");
		Console.WriteLine("  --- Turn Information --- ");
		Console.WriteLine("> Order: " + string.Join(", ", TurnOrder.Select((id) => id.Id)));
		Console.WriteLine("> Allowed Player Inputs: " + string.Join(", ", AllowedPlayerInputs.Select((id) => id.Id)));
		Console.WriteLine("> Index: " + TurnIndex);
		Console.WriteLine("> Phase: " + TurnPhase);
	}
}

public enum TurnPhase
{
	None = 0,
	Setup,
	Attacking
}