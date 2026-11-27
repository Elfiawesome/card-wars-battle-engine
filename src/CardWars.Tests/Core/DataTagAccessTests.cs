using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class DataTagAccessTests
{
	[Fact]
	public void MemberAccess_UsesPropertiesAndDataBag()
	{
		var model = new AccessModel { Hp = 7, Name = "unit" };
		model.Data.Set("mp", 3);

		Assert.True(TestBootstrap.DataTags.TryGetMember(model, "hp", out var hp));
		Assert.Equal(7, Assert.IsType<IntTag>(hp).Value);
		Assert.True(TestBootstrap.DataTags.TryGetMember(model, "name", out var name));
		Assert.Equal("unit", Assert.IsType<StringTag>(name).Value);
		Assert.True(TestBootstrap.DataTags.TryGetMember(model, "mp", out var mp));
		Assert.Equal(3, Assert.IsType<IntTag>(mp).Value);

		Assert.True(TestBootstrap.DataTags.TrySetMember(model, "hp", new IntTag(9)));
		Assert.Equal(9, model.Hp);
	}

	[Fact]
	public void MemberAccess_ReturnsNullTagForExistingNullValue()
	{
		var model = new AccessModel { NullableName = null };
		Assert.True(TestBootstrap.DataTags.TryGetMember(model, "nullable_name", out var value));
		Assert.IsType<NullTag>(value);
	}

	[Fact]
	public void MemberAccess_ExposesTypeId()
	{
		var entity = new TestEntity { Id = EntityId.New(), Hp = 1 };
		Assert.True(TestBootstrap.DataTags.TryGetMember(entity, TestBootstrap.DataTags.TypeDiscriminatorKey, out var value));
		Assert.Equal("cardwars:test/entity", Assert.IsType<StringTag>(value).Value);
	}

	[Fact]
	public void IndexAccess_UsesTagsDictionariesListsAndIndexers()
	{
		var compound = new CompoundTag().Set("hp", 4);
		Assert.True(TestBootstrap.DataTags.TryGetIndex(compound, new StringTag("hp"), out var hp));
		Assert.Equal(4, Assert.IsType<IntTag>(hp).Value);

		var list = new ListTag().Add(1).Add(2);
		Assert.True(TestBootstrap.DataTags.TryGetIndex(list, new IntTag(1), out var second));
		Assert.Equal(2, Assert.IsType<IntTag>(second).Value);

		var entity = new TestEntity { Id = EntityId.New(), Hp = 3 };
		var dictionary = new Dictionary<EntityId, IEntity> { [entity.Id] = entity };
		Assert.True(TestBootstrap.DataTags.TryGetIndex(dictionary, new GuidTag(entity.Id.Value), out var found));
		Assert.Same(entity, Assert.IsType<ObjectRefTag>(found).Value);

		var indexed = new IndexedModel();
		Assert.True(TestBootstrap.DataTags.TryGetIndex(indexed, new IntTag(2), out var indexedValue));
		Assert.Equal(20, Assert.IsType<IntTag>(indexedValue).Value);
	}

	[Fact]
	public void GameStateIndex_ResolvesEntityReferences()
	{
		var state = new GameState();
		var entity = new TestEntity { Id = EntityId.New(), Hp = 5 };
		state.Add(entity);

		Assert.True(TestBootstrap.DataTags.TryGetIndex(state, new GuidTag(entity.Id.Value), out var value));
		Assert.Same(entity, Assert.IsType<ObjectRefTag>(value).Value);
		Assert.False(TestBootstrap.DataTags.TryGetIndex(state, new GuidTag(Guid.NewGuid()), out _));
	}

	[Fact]
	public void ScriptContext_ExposesVariablesAsMembersAndIndexes()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var context = new ScriptContext(state, registry.DataDriven);
		var name = CardWars.Core.Registry.ResourceId.Vanilla("x");
		context.DeclareVariable(name, new IntTag(6));

		Assert.True(TestBootstrap.DataTags.TryGetMember(context, "x", out var memberValue));
		Assert.Equal(6, Assert.IsType<IntTag>(memberValue).Value);
		Assert.True(TestBootstrap.DataTags.TrySetMember(context, "x", new IntTag(7)));
		Assert.True(TestBootstrap.DataTags.TryGetIndex(context, new StringTag("x"), out var indexValue));
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
