using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Tests;

internal static class TestCards
{
	public static readonly ResourceId John = ResourceId.Vanilla("cards/units/john");
	public static readonly ResourceId Elbert = ResourceId.Vanilla("cards/units/elbert");
	public static readonly ResourceId Nicholas = ResourceId.Vanilla("cards/units/nicholas");

	public static void RegisterStarterDeck(
		BattleEngineRegistry registry,
		Action<ResourceId, CompoundTag>? configure = null)
	{
		Register(registry, John, Unit("John", hp: 6, atk: 4, pt: 2, charge: 1), configure);
		Register(registry, Elbert, Unit("Elbert", hp: 10, atk: 5, pt: 3, charge: 2), configure);
		Register(registry, Nicholas, Unit("Nicholas", hp: 4, atk: 3, pt: 1, charge: 1), configure);
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
