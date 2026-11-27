using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class ResourceIdNamespaceTests
{
	[Fact]
	public void ContextDefault_AppliesToUnqualifiedResourceIds()
	{
		var context = new DataTagContext("test");
		var tag = new CompoundTag().Set("resource", "thing");

		var pointer = context.FromTag<ContentPointer>(tag);

		Assert.Equal(new ResourceId("test", "thing"), pointer.Resource);
	}

	[Fact]
	public void ExplicitNamespace_WinsOverContextDefault()
	{
		var context = new DataTagContext("test");
		var tag = new CompoundTag().Set("resource", "other:thing");

		var pointer = context.FromTag<ContentPointer>(tag);

		Assert.Equal(new ResourceId("other", "thing"), pointer.Resource);
	}
}
