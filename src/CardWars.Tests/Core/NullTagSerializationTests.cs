using CardWars.Tests.Support;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class NullTagSerializationTests
{
	[Fact]
	public void LiteralNull_RoundTrips()
	{
		var tag = TestBootstrap.DataTags.ToTag(new LiteralExpression(new NullTag()));
		var json = TestBootstrap.DataTags.Serialize(tag);
		var restoredTag = TestBootstrap.DataTags.Deserialize<CompoundTag>(json) ?? new CompoundTag();
		var restored = TestBootstrap.DataTags.FromTag<LiteralExpression>(restoredTag);

		Assert.IsType<NullTag>(restored.Value);
	}

	[Fact]
	public void NestedNulls_ArePreservedInCompoundsAndLists()
	{
		var compound = new CompoundTag().Set("empty", new NullTag());
		var compoundJson = TestBootstrap.DataTags.Serialize(compound);
		var restoredCompound = TestBootstrap.DataTags.Deserialize<CompoundTag>(compoundJson) ?? new CompoundTag();
		Assert.True(restoredCompound.Has("empty"));
		Assert.IsType<NullTag>(restoredCompound.Get("empty"));

		var list = new ListTag().Add(new NullTag()).Add(1);
		var listJson = TestBootstrap.DataTags.Serialize(list);
		var restoredList = TestBootstrap.DataTags.Deserialize<ListTag>(listJson) ?? new ListTag();
		Assert.Equal(2, restoredList.Count);
		Assert.IsType<NullTag>(restoredList.Get(0));
		Assert.Equal(1, Assert.IsType<IntTag>(restoredList.Get(1)).Value);
	}

	[Fact]
	public void MissingDataTagProperty_DefaultsToNullTag()
	{
		var literal = TestBootstrap.DataTags.FromTag<LiteralExpression>(new CompoundTag());
		Assert.IsType<NullTag>(literal.Value);
	}

	[Fact]
	public void NullTag_ConvertsToNullForNullableValueTypes()
	{
		var tag = new CompoundTag().Set("target_entity_id", new NullTag());
		var restored = TestBootstrap.DataTags.FromTag<UseCardRequestInput>(tag);

		Assert.Null(restored.TargetEntityId);
	}

	[Fact]
	public void ObjectRefTag_CannotBeSerialized()
	{
		Assert.Throws<NotSupportedException>(() => TestBootstrap.DataTags.Serialize(new ObjectRefTag(new object())));
	}

	[Fact]
	public void JsonNull_DeserializesToNullTag()
	{
		var tag = TestBootstrap.DataTags.Deserialize<DataTag>("null");
		Assert.IsType<NullTag>(tag);
	}
}
