using CardWars.Core.Registry;

namespace CardWars.Vanilla.Shared;

public static class SharedIds // TODO: BEtter name plz
{
	// Instances
	public static readonly ResourceId WorldInstanceId = ResourceId.Vanilla("world");
	public static readonly ResourceId BattleInstanceId = ResourceId.Vanilla("battle");

	// UI
	public static readonly ResourceId CardDisplay = ResourceId.Vanilla("card_display");

	// Game Objects
	public static readonly ResourceId Battlefield = ResourceId.Vanilla("battlefield");
	public static readonly ResourceId UnitSlot = ResourceId.Vanilla("unit_slot");
}