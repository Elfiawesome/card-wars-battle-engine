using CardWars.BattleEngine;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.BattleEngine.Vanilla.Targeting;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

/// <summary>
/// Phase 7 behavior-preservation proof: the card-type defaults that used to be produced inline in
/// C# (UnitTargetFilter.Create, the intrinsic_behaviours ListTag, the HeroHrtLoseCondition content
/// pointer) must come out byte-identical now that they live in content/server/card_defaults/*.json
/// and are merged by the data-driven loader. These tests load the REAL gamedata JSON files through
/// the live mod-content load path, so a wrong template byte would fail them.
/// </summary>
public sealed class CardDefaultsEquivalenceTests
{
	[Fact]
	public void UnitDefaults_FromData_MatchFormerCSharpProducers()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("cards/units/test_unit.json", """{"name":"Test Unit","hp":4,"atk":2}""");

		var registry = new BattleEngineRegistry();
		var card = new ModContentResult
		{
			Id = ResourceId.Vanilla("cards/units/test_unit"),
			FilePath = new StoragePath("cards/units/test_unit.json", provider),
			Category = ["cards", "units"]
		};
		var unitDefaults = TestGameData.CardDefaultContent("unit", provider);

		new VanillaMod().OnLoad(registry, DataContext.Default, [card, unitDefaults]);

		var definition = registry.CardDefinitions.Get(card.Id)
			?? throw new InvalidOperationException("Unit definition was not registered");

		Assert.Equal("unit", definition.GetString("card_type"));

		var expectedFilter = DataContext.Default.ToTag(UnitTargetFilter.Create());
		var actualFilter = definition.GetCompound("target_filter")
			?? throw new InvalidOperationException("target_filter was not injected");
		AssertTagEqual(expectedFilter, actualFilter);

		var expectedBehaviours = new ListTag().Add(
			new CompoundTag().Set("resource", "cardwars:summon_unit_card_to_unit_slot_behaviour"));
		var actualBehaviours = definition.GetList("intrinsic_behaviours")
			?? throw new InvalidOperationException("intrinsic_behaviours was not injected");
		AssertTagEqual(expectedBehaviours, actualBehaviours);
	}

	[Fact]
	public void HeroDefaults_FromData_MatchFormerCSharpProducers()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("cards/heroes/test_hero.json", """{"name":"Test Hero","hrt":5}""");

		var registry = new BattleEngineRegistry();
		var card = new ModContentResult
		{
			Id = ResourceId.Vanilla("cards/heroes/test_hero"),
			FilePath = new StoragePath("cards/heroes/test_hero.json", provider),
			Category = ["cards", "heroes"]
		};
		var heroDefaults = TestGameData.CardDefaultContent("hero", provider);

		new VanillaMod().OnLoad(registry, DataContext.Default, [card, heroDefaults]);

		var definition = registry.CardDefinitions.Get(card.Id)
			?? throw new InvalidOperationException("Hero definition was not registered");

		Assert.Equal("hero", definition.GetString("card_type"));

		var expectedPointer = DataContext.Default.ToTag(new ContentPointer(Resource: HeroHrtLoseCondition.Id));
		var actualPointer = definition.GetCompound("lose_condition")
			?? throw new InvalidOperationException("lose_condition was not injected");
		AssertTagEqual(expectedPointer, actualPointer);
	}

	// Deep structural equality for DataTags, order-insensitive for compound keys (JSON object key
	// order is irrelevant; the parsed tree is what must match).
	private static void AssertTagEqual(DataTag expected, DataTag actual)
		=> Assert.True(TagEquals(expected, actual),
			$"DataTag trees differ.\nExpected: {DataContext.Default.Serialize(expected)}\nActual:   {DataContext.Default.Serialize(actual)}");

	private static bool TagEquals(DataTag? a, DataTag? b)
	{
		if (a is null || b is null) return a is null && b is null;
		if (a.Type != b.Type) return false;

		return (a, b) switch
		{
			(CompoundTag ca, CompoundTag cb) =>
				ca.Count == cb.Count
				&& ca.Entries.All(kv => cb.Entries.TryGetValue(kv.Key, out var bv) && TagEquals(kv.Value, bv)),
			(ListTag la, ListTag lb) =>
				la.Count == lb.Count
				&& la.Items.Zip(lb.Items, TagEquals).All(x => x),
			(IntTag ia, IntTag ib) => ia.Value == ib.Value,
			(FloatTag fa, FloatTag fb) => fa.Value.Equals(fb.Value),
			(StringTag sa, StringTag sb) => sa.Value == sb.Value,
			(BoolTag ba, BoolTag bb) => ba.Value == bb.Value,
			(GuidTag ga, GuidTag gb) => ga.Value == gb.Value,
			(NullTag, NullTag) => true,
			_ => false
		};
	}
}
