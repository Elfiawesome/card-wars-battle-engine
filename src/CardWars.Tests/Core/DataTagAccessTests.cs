using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Access;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using Xunit;

namespace CardWars.Tests;

public sealed class DataTagAccessTests
{
	[Fact]
	public void MemberAccess_UsesPropertiesAndDataBag()
	{
		var model = new AccessModel { Hp = 7, Name = "unit" };
		model.Data.Set("mp", 3);

		Assert.True(DataTagAccess.TryGetMember(model, "hp", out var hp));
		Assert.Equal(7, Assert.IsType<IntTag>(hp).Value);
		Assert.True(DataTagAccess.TryGetMember(model, "name", out var name));
		Assert.Equal("unit", Assert.IsType<StringTag>(name).Value);
		Assert.True(DataTagAccess.TryGetMember(model, "mp", out var mp));
		Assert.Equal(3, Assert.IsType<IntTag>(mp).Value);

		Assert.True(DataTagAccess.TrySetMember(model, "hp", new IntTag(9)));
		Assert.Equal(9, model.Hp);
	}

	[Fact]
	public void MemberAccess_ReturnsNullTagForExistingNullValue()
	{
		var model = new AccessModel { NullableName = null };
		Assert.True(DataTagAccess.TryGetMember(model, "nullable_name", out var value));
		Assert.IsType<NullTag>(value);
	}

	[Fact]
	public void MemberAccess_ExposesTypeId()
	{
		var entity = new TestEntity { Id = EntityId.New(), Hp = 1 };
		Assert.True(DataTagAccess.TryGetMember(entity, DataTagMapper.TypeDiscriminatorKey, out var value));
		Assert.Equal("cardwars:test/entity", Assert.IsType<StringTag>(value).Value);
	}

	[Fact]
	public void IndexAccess_UsesTagsDictionariesListsAndIndexers()
	{
		var compound = new CompoundTag().Set("hp", 4);
		Assert.True(DataTagAccess.TryGetIndex(compound, new StringTag("hp"), out var hp));
		Assert.Equal(4, Assert.IsType<IntTag>(hp).Value);

		var list = new ListTag().Add(1).Add(2);
		Assert.True(DataTagAccess.TryGetIndex(list, new IntTag(1), out var second));
		Assert.Equal(2, Assert.IsType<IntTag>(second).Value);

		var entity = new TestEntity { Id = EntityId.New(), Hp = 3 };
		var dictionary = new Dictionary<EntityId, IEntity> { [entity.Id] = entity };
		Assert.True(DataTagAccess.TryGetIndex(dictionary, new GuidTag(entity.Id.Value), out var found));
		Assert.Same(entity, Assert.IsType<ObjectRefTag>(found).Value);

		var indexed = new IndexedModel();
		Assert.True(DataTagAccess.TryGetIndex(indexed, new IntTag(2), out var indexedValue));
		Assert.Equal(20, Assert.IsType<IntTag>(indexedValue).Value);
	}

	[Fact]
	public void GameStateIndex_ResolvesEntityReferences()
	{
		var state = new GameState();
		var entity = new TestEntity { Id = EntityId.New(), Hp = 5 };
		state.Add(entity);

		Assert.True(DataTagAccess.TryGetIndex(state, new GuidTag(entity.Id.Value), out var value));
		Assert.Same(entity, Assert.IsType<ObjectRefTag>(value).Value);
		Assert.False(DataTagAccess.TryGetIndex(state, new GuidTag(Guid.NewGuid()), out _));
	}

	[Fact]
	public void ScriptContext_ExposesVariablesAsMembersAndIndexes()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var context = new ScriptContext(state, registry.DataDriven);
		var name = CardWars.Core.Registry.ResourceId.Vanilla("x");
		context.DeclareVariable(name, new IntTag(6));

		Assert.True(DataTagAccess.TryGetMember(context, "x", out var memberValue));
		Assert.Equal(6, Assert.IsType<IntTag>(memberValue).Value);
		Assert.True(DataTagAccess.TrySetMember(context, "x", new IntTag(7)));
		Assert.True(DataTagAccess.TryGetIndex(context, new StringTag("x"), out var indexValue));
		Assert.Equal(7, Assert.IsType<IntTag>(indexValue).Value);
	}

	private sealed class AccessModel
	{
		[DataTag] public int Hp { get; set; }
		public string Name { get; set; } = "";
		public string? NullableName { get; set; }
		public CompoundTag Data { get; set; } = new();
	}

	private sealed class IndexedModel
	{
		public int this[int index] => index * 10;
	}
}
