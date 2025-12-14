using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardWars.BattleEngine.State;

public class StateService(IServiceContainer container) : Service(container)
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