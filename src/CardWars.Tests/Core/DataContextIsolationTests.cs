using CardWars.Core.Data;
using Xunit;

namespace CardWars.Tests.Core;

/// <summary>
/// Behavioral proof of session isolation introduced in Phase 1 (doc.md §5): registering a
/// type or converter into one <see cref="DataContext"/> must not leak into another. This
/// replaces the implicit guarantee that TestBootstrap's old all-or-nothing Clear() used to
/// provide. These tests exercise DataContext instance methods directly, never the static facade.
/// </summary>
public sealed class DataContextIsolationTests
{
	private sealed class IsolatedModel { }

	[Fact]
	public void RegisteringTypeIntoOneContext_DoesNotLeakIntoAnother()
	{
		var contextA = new DataContext();
		var contextB = new DataContext();

		const string typeId = "cardwars:test/isolated_model";
		contextA.Register(typeof(IsolatedModel), typeId);

		// Resolution succeeds in the context that registered the type...
		Assert.Equal(typeof(IsolatedModel), contextA.GetType(typeId));
		Assert.Equal(typeId, contextA.GetTypeId(typeof(IsolatedModel)));

		// ...and is absent from the independent context.
		Assert.Null(contextB.GetType(typeId));
		Assert.Null(contextB.GetTypeId(typeof(IsolatedModel)));
	}

	[Fact]
	public void RegisteringConverterIntoOneContext_DoesNotLeakIntoAnother()
	{
		var contextA = new DataContext();
		var contextB = new DataContext();

		contextA.RegisterConverter(typeof(IsolatedModel), new NoOpConverter());

		Assert.True(contextA.HasConverter(typeof(IsolatedModel)));
		Assert.False(contextB.HasConverter(typeof(IsolatedModel)));
	}

	[Fact]
	public void DiscriminatorKey_IsPerContext()
	{
		var contextA = new DataContext { TypeDiscriminatorKey = "_custom" };
		var contextB = new DataContext();

		Assert.Equal("_custom", contextA.TypeDiscriminatorKey);
		Assert.Equal("_type", contextB.TypeDiscriminatorKey);
	}

	private sealed class NoOpConverter : CardWars.Core.Data.Converters.DataTagConverter<IsolatedModel>
	{
		protected override CardWars.Core.Data.Tags.DataTag? ConvertTo(IsolatedModel value) => new CardWars.Core.Data.Tags.CompoundTag();
		protected override IsolatedModel ConvertFrom(CardWars.Core.Data.Tags.DataTag tag) => new();
	}
}
