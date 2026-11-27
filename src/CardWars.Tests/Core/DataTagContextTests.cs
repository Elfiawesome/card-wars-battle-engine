using CardWars.Core.Data;
using Xunit;

namespace CardWars.Tests.Core;

public sealed class DataTagContextTests
{
	[Fact]
	public void Contexts_HaveIndependentTypeRegistries()
	{
		var first = new DataTagContext("test");
		var second = new DataTagContext("test");

		first.Register<ContextTestType>("test/context_type");

		Assert.Equal(typeof(ContextTestType), first.GetType("test/context_type"));
		Assert.Null(second.GetType("test/context_type"));
	}

	private sealed class ContextTestType;
}
