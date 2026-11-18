using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Tests.Support;

internal static class TestCards
{
	public static readonly ResourceId By = ResourceId.Vanilla("cards/units/by");
	public static readonly ResourceId Bz = ResourceId.Vanilla("cards/units/bz");
	public static readonly ResourceId Captain = ResourceId.Vanilla("cards/units/captain");
	public static readonly ResourceId Engineer = ResourceId.Vanilla("cards/units/engineer");
	public static readonly ResourceId Faketree = ResourceId.Vanilla("cards/units/faketree");
	public static readonly ResourceId Hacker = ResourceId.Vanilla("cards/units/hacker");
	public static readonly ResourceId King = ResourceId.Vanilla("cards/units/king");
	public static readonly ResourceId Psycho = ResourceId.Vanilla("cards/units/psycho");
	public static readonly ResourceId Universal = ResourceId.Vanilla("cards/units/universal");
	public static readonly ResourceId Ilyas = ResourceId.Vanilla("cards/heroes/ilyas");

	public static void RegisterStarterDeck(
		BattleEngineRegistry registry,
		Action<ResourceId, CompoundTag>? configure = null)
	{
		Register(registry, By, Unit("BY", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Bz, Unit("BZ", hp: 10, atk: 5, pt: 3, charge: 2), configure);
		Register(registry, Captain, Unit("Captain", hp: 4, atk: 3, pt: 1, charge: 1), configure);
		Register(registry, Engineer, Unit("Engineer", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Faketree, Unit("Faketree", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Hacker, Unit("Hacker", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, King, Unit("King", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Psycho, Unit("Psycho", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Universal, Unit("Universal", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Ilyas, Hero("Ilyas", hrt: 10), configure);
	}

	public static void Register(
		BattleEngineRegistry registry,
		ResourceId id,
		CompoundTag definition,
		Action<ResourceId, CompoundTag>? configure = null)
	{
		configure?.Invoke(id, definition);
		registry.CardDefinitions.Register(id, definition);
	}

	public static CompoundTag Hero(string name, int hrt)
		=> new CompoundTag()
			.Set("name", name)
			.Set("hrt", hrt)
			.Set("card_type", "hero");

	public static CompoundTag Unit(string name, int hp, int atk, int pt, int charge)
		=> new CompoundTag()
			.Set("name", name)
			.Set("hp", hp)
			.Set("atk", atk)
			.Set("pt", pt)
			.Set("charge", charge)
			.Set("card_type", "unit")
			.Set("target_filter", DataTagMapper.ToTag(UnitTargetFilter.Create()))
			.Set("intrinsic_behaviours", new ListTag().Add(
				new CompoundTag().Set("resource", "cardwars:summon_unit_card_to_unit_slot_behaviour")));
}
