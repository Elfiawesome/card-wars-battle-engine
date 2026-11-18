using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Tests.Support;

internal static class TestCards
{
	public static readonly ResourceId CobCannon = ResourceId.Vanilla("cards/units/plantvszombies/cob_cannon");
	public static readonly ResourceId ApocalypseTank = ResourceId.Vanilla("cards/units/redalert3/apocalypse_tank");
	public static readonly ResourceId AssaultDestroyer = ResourceId.Vanilla("cards/units/redalert3/assault_destroyer");
	public static readonly ResourceId Fastpass = ResourceId.Vanilla("cards/units/southpark/fastpass");
	public static readonly ResourceId Ilyas = ResourceId.Vanilla("cards/heroes/ilyas");

	public static void RegisterStarterDeck(
		BattleEngineRegistry registry,
		Action<ResourceId, CompoundTag>? configure = null)
	{
		Register(registry, CobCannon, Unit("Cob Cannon", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, ApocalypseTank, Unit("Apocalypse Tank", hp: 10, atk: 5, pt: 3, charge: 2), configure);
		Register(registry, AssaultDestroyer, Unit("Assault Destroyer", hp: 4, atk: 3, pt: 1, charge: 1), configure);
		Register(registry, Fastpass, Unit("Fastpass", hp: 6, atk: 4, pt: 2, charge: 1), configure);
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
