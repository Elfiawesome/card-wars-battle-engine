using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class AttackDefinitionTests
{
	[Fact]
	public void DataTagMapper_RoundTripsAttackDefinition()
	{
		var definition = new AttackDefinition(
		[
			new AttackPattern(
				AttackPatternType.Cell,
				new UnitSlotPos(-1, 1),
				new AttackDamageSpec(0.5f, 3)),
			new AttackPattern(
				AttackPatternType.Row,
				new UnitSlotPos(0, 1),
				new AttackDamageSpec(1.0f, 0))
		], "Cross", 2, true);

		var parsed = DataTagMapper.FromTag<AttackDefinition>(DataTagMapper.ToTag(definition));

		Assert.Equal("Cross", parsed.Name);
		Assert.Equal(2, parsed.ChargeCost);
		Assert.True(parsed.Bypass);
		Assert.Equal(2, parsed.Patterns.Count);
		Assert.Equal(AttackPatternType.Cell, parsed.Patterns[0].Type);
		Assert.Equal(-1, parsed.Patterns[0].Offset.X);
		Assert.Equal(1, parsed.Patterns[0].Offset.Y);
		Assert.Equal(3, parsed.Patterns[0].Damage.Resolve(5));
		Assert.Equal(2, parsed.Patterns[1].Damage.Resolve(5));
	}

	[Fact]
	public void Resolver_CombinesInlineSpAtksAndUsesAnyBypass()
	{
		var card = new GenericCard(EntityId.New())
		{
			Data = new CompoundTag().Set("sp_atks", new ListTag()
				.Add(SpAtk("Cross", 1, true, Pattern(AttackPatternType.Cell, 1)))
				.Add(SpAtk("Sweep", 2, false, Pattern(AttackPatternType.Row, 2))))
		};

		var attack = new AttackResolver(TestSupport.CreateRegistry()).Resolve(card);

		Assert.True(attack.IsSpAtk);
		Assert.True(attack.Bypass);
		Assert.Equal(3, attack.ChargeCost);
		Assert.Equal(2, attack.Patterns.Count);

		Assert.True(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(0, 0),
			out var spec));
		Assert.Equal(2, spec.Amount);
	}

	[Fact]
	public void Resolver_ResolvesRegisteredSpAtkPointer()
	{
		var registry = TestSupport.CreateRegistry();
		var definition = new AttackDefinition(
		[
			Pattern(AttackPatternType.Cell, 3)
		], "Registered", 2, true);
		registry.AttackDefinitions.Register(
			ResourceId.Vanilla("attacks/registered"),
			DataTagMapper.ToTag(definition));

		var card = new GenericCard(EntityId.New())
		{
			Data = new CompoundTag().Set("sp_atks", new ListTag()
				.Add(TestSupport.AttackPointer(ResourceId.Vanilla("attacks/registered"))))
		};

		var attack = new AttackResolver(registry).Resolve(card);

		Assert.True(attack.IsSpAtk);
		Assert.True(attack.Bypass);
		Assert.Equal(2, attack.ChargeCost);
		Assert.Equal("Registered", attack.Name);
		Assert.Single(attack.Patterns);
	}
	[Fact]
	public void Resolver_UsesDefaultBasicAttackWhenNoSpAtkExists()
	{
		var card = new GenericCard(EntityId.New())
		{
			Data = new CompoundTag()
		};

		var attack = new AttackResolver(TestSupport.CreateRegistry()).Resolve(card);

		Assert.False(attack.IsSpAtk);
		Assert.False(attack.Bypass);
		Assert.Equal(1, attack.ChargeCost);
		Assert.Single(attack.Patterns);
		Assert.True(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(0, 0),
			out var spec));
		Assert.Equal(5, spec.Resolve(5));
	}

	[Fact]
	public void Resolver_UsesInlineBaAtk()
	{
		var card = new GenericCard(EntityId.New())
		{
			Data = new CompoundTag().Set("ba_atk", TestSupport.AttackPointer(DataTagMapper.ToTag(new AttackDefinition(
			[
				Pattern(AttackPatternType.Cell, 4, 1, 0)
			], "Custom BA", 0, false))))
		};

		var attack = new AttackResolver(TestSupport.CreateRegistry()).Resolve(card);

		Assert.False(attack.IsSpAtk);
		Assert.Equal("Custom BA", attack.Name);
		Assert.Equal(0, attack.ChargeCost);
		Assert.False(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(0, 0),
			out _));
		Assert.True(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(1, 0),
			out var spec));
		Assert.Equal(4, spec.Amount);
	}

	private static CompoundTag SpAtk(
		string name,
		int chargeCost,
		bool bypass,
		params AttackPattern[] patterns)
		=> TestSupport.AttackPointer(DataTagMapper.ToTag(new AttackDefinition(patterns, name, chargeCost, bypass)));

	private static AttackPattern Pattern(
		AttackPatternType type,
		int amount,
		int x = 0,
		int y = 0,
		float percent = 0f)
		=> new(type, new UnitSlotPos(x, y), new AttackDamageSpec(percent, amount));
}