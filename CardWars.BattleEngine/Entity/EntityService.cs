namespace CardWars.BattleEngine.Entity;

public sealed class EntityService
{
	public Dictionary<BattlefieldId, Battlefield> Battlefields = [];
	public Dictionary<DeckId, Deck> Decks = [];
	public Dictionary<PlayerId, Player> Players = [];
	public Dictionary<UnitId, UnitCard> UnitCards = [];
	public Dictionary<UnitSlotId, UnitSlot> UnitSlots = [];
}