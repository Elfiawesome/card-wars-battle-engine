using CardWars.Vanilla.Shared;
using CardWars.Core.Registry;

namespace CardWars.Vanilla.Shared;

public static class CardWarsIds
{
	public static ResourceId Vanilla(string path) => new("cardwars", path);
}

public static class SharedIds
{
	// Instances
	public static class Instances
	{
		public static readonly ResourceId World = CardWarsIds.Vanilla("world");
		public static readonly ResourceId Battle = CardWarsIds.Vanilla("battle");
	}

	// UI
	public static class UI
	{
		public static readonly ResourceId StandardCardDisplay = CardWarsIds.Vanilla("standard_card_display");
		public static readonly ResourceId StandardCardInspectionPanel = CardWarsIds.Vanilla("standard_card_inspection_panel");
		public static readonly ResourceId LineArrow = CardWarsIds.Vanilla("line_arrow");
		public static readonly ResourceId TargetHighlight = CardWarsIds.Vanilla("target_highlight");
	}

	// Battle Entity Objects
	public static class Entities
	{
		public static readonly ResourceId Battlefield = CardWarsIds.Vanilla("battle/entity/battlefield");
		public static readonly ResourceId UnitSlot = CardWarsIds.Vanilla("battle/entity/unit_slot");
		public static readonly ResourceId Card = CardWarsIds.Vanilla("battle/entity/card");
		public static readonly ResourceId Player = CardWarsIds.Vanilla("battle/entity/player");
		public static readonly ResourceId Deck = CardWarsIds.Vanilla("battle/entity/deck");
	}

	public static class PlayerData
	{
		public static readonly ResourceId Profile = CardWarsIds.Vanilla("player_data/profile");
	}

	public static class Animations
	{
		public static readonly ResourceId PlayerIntroduction = CardWarsIds.Vanilla("battle/animation/player_introduction_animation");
		public static readonly ResourceId PlayCard = CardWarsIds.Vanilla("battle/animation/play_card_animation");
		public static readonly ResourceId Attack = CardWarsIds.Vanilla("battle/animation/attack_animation");
		public static readonly ResourceId TakeDamage = CardWarsIds.Vanilla("battle/animation/take_damage_animation");
		public static readonly ResourceId Death = CardWarsIds.Vanilla("battle/animation/death_animation");

		public static class Args
		{
			public const string Attacker = "attacker";
			public const string Target = "target";
			public const string CardId = "card_id";
			public const string SlotId = "slot_id";
			public const string Units = "units";
		}
	}
}
