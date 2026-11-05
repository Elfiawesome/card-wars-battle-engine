using CardWars.Core.Data.Tags;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class DataTagTruthinessTests
{
	[Fact]
	public void PrimitiveTags_ReportExpectedTruthiness()
	{
		Assert.False(new NullTag().IsTruthy);
		Assert.False(new IntTag(0).IsTruthy);
		Assert.True(new IntTag(1).IsTruthy);
		Assert.False(new FloatTag(0).IsTruthy);
		Assert.True(new FloatTag(0.1f).IsTruthy);
		Assert.False(new StringTag("").IsTruthy);
		Assert.True(new StringTag("x").IsTruthy);
		Assert.False(new BoolTag(false).IsTruthy);
		Assert.True(new BoolTag(true).IsTruthy);
		Assert.False(new GuidTag(Guid.Empty).IsTruthy);
		Assert.True(new GuidTag(Guid.NewGuid()).IsTruthy);
	}

	[Fact]
	public void ContainerAndReferenceTags_ReportExpectedTruthiness()
	{
		Assert.False(new ListTag().IsTruthy);
		Assert.True(new ListTag().Add(new IntTag(1)).IsTruthy);
		Assert.False(new CompoundTag().IsTruthy);
		Assert.True(new CompoundTag().Set("x", 1).IsTruthy);
		Assert.True(new ObjectRefTag(new object()).IsTruthy);
	}
}
