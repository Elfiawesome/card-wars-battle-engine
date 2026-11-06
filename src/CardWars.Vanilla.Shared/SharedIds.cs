using CardWars.Core.Registry;

namespace CardWars.Vanilla.Shared;

public static class SharedIds
{
	// Instances
	public static class Instances
	{
		public static readonly ResourceId World = ResourceId.Vanilla("world");
		public static readonly ResourceId Battle = ResourceId.Vanilla("battle");
	}

	// UI
	public static class UI
	{
		public static readonly ResourceId CardDisplay = ResourceId.Vanilla("card_display");
		public static readonly ResourceId LineArrow = ResourceId.Vanilla("line_arrow");
		public static readonly ResourceId TargetHighlight = ResourceId.Vanilla("target_highlight");
	}

	// Battle Entity Objects
	public static class Entities
	{
		public static readonly ResourceId Battlefield = ResourceId.Vanilla("battle/entity/battlefield");
		public static readonly ResourceId UnitSlot = ResourceId.Vanilla("battle/entity/unit_slot");
		public static readonly ResourceId Card = ResourceId.Vanilla("battle/entity/card");
		public static readonly ResourceId Player = ResourceId.Vanilla("battle/entity/player");
		public static readonly ResourceId Deck = ResourceId.Vanilla("battle/entity/deck");
	}

	public static class Animations
	{
		public static readonly ResourceId PlayerIntroduction = ResourceId.Vanilla("battle/animation/player_introduction_animation");
		public static readonly ResourceId PlayCard = ResourceId.Vanilla("battle/animation/play_card_animation");
		public static readonly ResourceId Attack = ResourceId.Vanilla("battle/animation/attack_animation");
		public static readonly ResourceId TakeDamage = ResourceId.Vanilla("battle/animation/take_damage_animation");
		public static readonly ResourceId Death = ResourceId.Vanilla("battle/animation/death_animation");
	}
}
