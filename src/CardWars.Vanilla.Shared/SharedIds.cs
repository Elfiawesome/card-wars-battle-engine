using CardWars.Core.Registry;

namespace CardWars.Vanilla.Shared;

public static class SharedIds // TODO: BEtter name plz
{
	// Instances
	public static readonly ResourceId WorldInstanceId = ResourceId.Vanilla("world");
	public static readonly ResourceId BattleInstanceId = ResourceId.Vanilla("battle");

	// UI
	public static readonly ResourceId CardDisplay = ResourceId.Vanilla("card_display");

	
	// Battle Entity Objects
	public static readonly ResourceId Battlefield = ResourceId.Vanilla("battle/entity/battlefield");
	public static readonly ResourceId UnitSlot = ResourceId.Vanilla("battle/entity/unit_slot");
	public static readonly ResourceId Card = ResourceId.Vanilla("battle/entity/card");
	public static readonly ResourceId Player = ResourceId.Vanilla("battle/entity/player");
	public static readonly ResourceId Deck = ResourceId.Vanilla("battle/entity/deck");
}