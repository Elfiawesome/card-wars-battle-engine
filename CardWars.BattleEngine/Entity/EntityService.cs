using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Entity;

public sealed class EntityService
{
	public Dictionary<BattlefieldId, Battlefield> Battlefields = [];
	public Dictionary<DeckId, Deck> Decks = [];
	public Dictionary<PlayerId, Player> Players = [];
	public Dictionary<UnitCardId, UnitCard> UnitCards = [];
	public Dictionary<SpellCardId, SpellCard> SpellCards = [];
	public Dictionary<UnitSlotId, UnitSlot> UnitSlots = [];
	public Dictionary<AbilityId, Ability> Abilities = [];
}