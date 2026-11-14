using CardWars.BattleEngine;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class AttackDefinitionTests
{
	[Fact]
	public void Parser_ParsesFlatPercentAndCellPatterns()
	{
		var tag = new CompoundTag()
			.Set("name", "Cross")
			.Set("charge_cost", 2)
			.Set("bypass", true)
			.Set("patterns", new ListTag()
				.Add(new CompoundTag().Set("type", "cell").Set("x", -1).Set("y", 1).Set("value", 3))
				.Add(new CompoundTag().Set("type", "row").Set("value", "50%")));

		Assert.True(AttackDefinitionParser.TryParse(tag, "Fallback", out var definition));
		Assert.Equal("Cross", definition.Name);
		Assert.Equal(2, definition.ChargeCost);
		Assert.True(definition.Bypass);
		Assert.Equal(2, definition.Patterns.Count);
		Assert.Equal(-1, definition.Patterns[0].X);
		Assert.Equal(1, definition.Patterns[0].Y);
		Assert.Equal(3, definition.Patterns[0].Damage.Resolve(5));
		Assert.True(definition.Patterns[1].Damage.IsPercent);
		Assert.Equal(2, definition.Patterns[1].Damage.Resolve(5));
	}

	[Fact]
	public void Resolver_CombinesAllSpAtksAndUsesAnyBypass()
	{
		var registry = TestSupport.CreateRegistry();
		registry.AttackDefinitions.Register(
			ResourceId.Vanilla("attacks/cross"),
			new CompoundTag()
				.Set("name", "Cross")
				.Set("charge_cost", 1)
				.Set("bypass", true)
				.Set("patterns", new ListTag().Add(
					new CompoundTag().Set("type", "row").Set("value", 2))));

		var card = new GenericCard(EntityId.New())
		{
			Data = new CompoundTag().Set("sp_atks", new ListTag()
				.Add(TestSupport.AttackPointer(ResourceId.Vanilla("attacks/cross")))
				.Add(TestSupport.AttackPointer(new CompoundTag()
					.Set("name", "Sweep")
					.Set("charge_cost", 2)
					.Set("patterns", new ListTag().Add(
						new CompoundTag().Set("type", "column").Set("value", "50%"))))))
		};

		var attack = new AttackResolver(registry).Resolve(card);

		Assert.True(attack.IsSpAtk);
		Assert.True(attack.Bypass);
		Assert.Equal(3, attack.ChargeCost);
		Assert.Equal(2, attack.Patterns.Count);
	}

	[Fact]
	public void Resolver_UsesBasicAttackWhenNoSpAtkExists()
	{
		var registry = TestSupport.CreateRegistry();
		var card = new GenericCard(EntityId.New())
		{
			Data = new CompoundTag()
		};

		var attack = new AttackResolver(registry).Resolve(card);

		Assert.False(attack.IsSpAtk);
		Assert.False(attack.Bypass);
		Assert.Equal(1, attack.ChargeCost);
		Assert.Empty(attack.Patterns);
	}

	[Fact]
	public void CardRegistration_StoresSpAtkReferencesForRuntimeResolution()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("attacks/cross.json", """
			{
				"name": "Cross",
				"charge_cost": 1,
				"bypass": true,
				"patterns": [
					{ "type": "cell", "x": 0, "y": -1, "value": 2 }
				]
			}
			""");
		provider.AddFile("cards/units/test_unit.json", """
			{
				"name": "Test Unit",
				"hp": 4,
				"atk": 2,
				"sp_atks": [
					{ "resource": "cardwars:attacks/cross" }
				]
			}
			""");

		var registry = new BattleEngineRegistry();
		var attackContent = new ModContentResult
		{
			Id = ResourceId.Vanilla("attacks/cross"),
			FilePath = new StoragePath("attacks/cross.json", provider),
			Category = ["attacks"]
		};
		var unitContent = new ModContentResult
		{
			Id = ResourceId.Vanilla("cards/units/test_unit"),
			FilePath = new StoragePath("cards/units/test_unit.json", provider),
			Category = ["cards", "units"]
		};

		new VanillaMod().OnLoad(registry, [attackContent, unitContent]);

		var definition = registry.CardDefinitions.Get(unitContent.Id)
			?? throw new InvalidOperationException("Unit definition was not registered");
		var pointer = definition.GetList("sp_atks")?.Get<CompoundTag>(0)
			?? throw new InvalidOperationException("sp_atk pointer was not stored");
		Assert.Equal("cardwars:attacks/cross", pointer.GetString("resource"));

		var card = new GenericCard(EntityId.New()) { Data = (CompoundTag)definition.Clone() };
		var attack = new AttackResolver(registry).Resolve(card);
		Assert.True(attack.IsSpAtk);
		Assert.True(attack.Bypass);
		Assert.Equal("Cross", attack.Name);
	}

	[Fact]
	public void Parser_IgnoresMalformedPatterns()
	{
		var tag = new CompoundTag()
			.Set("patterns", new ListTag()
				.Add(new CompoundTag().Set("type", "unknown").Set("value", 2))
				.Add(new CompoundTag().Set("type", "cell").Set("value", "bad")));

		Assert.True(AttackDefinitionParser.TryParse(tag, "Fallback", out var definition));
		Assert.Empty(definition.Patterns);
	}
}
