using CardWars.Core.Data;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
using CardWars.Core.Storage;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Core;

/// <summary>
/// Phase 5 (doc.md §5 "Storage cleanup", AGENT.md R1): the storage layer's (de)serialization is
/// scoped to an owner-supplied <see cref="DataContext"/> instead of the temporary static
/// <see cref="DataTagSerializer"/> / <see cref="DataContext.Default"/> facade. These tests prove
/// that a <see cref="StorageManager"/> built with a specific context routes SaveData/LoadData
/// through THAT context, and that the default (no context supplied) still behaves exactly as the
/// facade did. Behavior must be preserved: the JSON shape for the default context is unchanged.
/// </summary>
public sealed class StorageContextScopingTests
{
	private static CompoundTag SampleTag()
		=> new CompoundTag()
			.Set("name", "hero")
			.Set("level", 7)
			.Set("id", Guid.Parse("11111111-1111-1111-1111-111111111111"));

	[Fact]
	public void SaveAndLoad_RoundTripsThroughOwnedContext()
	{
		var data = new DataContext();
		var provider = new MemoryFileProvider();
		var storage = new StorageManager("data", provider, logger: null, data: data);
		var session = storage.OpenSession("scoping");

		var path = session.GetPath("roundtrip.json");
		session.SaveData(path, SampleTag());

		var loaded = session.LoadData(path) as CompoundTag;

		Assert.NotNull(loaded);
		Assert.Equal("hero", loaded!.GetString("name"));
		Assert.Equal(7, loaded.GetInt("level"));
		Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), loaded.GetGuid("id"));
	}

	/// <summary>
	/// Distinguishing proof that storage serializes through the OWNED context and not the static
	/// facade. The owned context's <see cref="JsonSerializerOptions"/> is configured with
	/// <c>WriteIndented = true</c>; <see cref="DataContext.Default"/> writes compact JSON. The bytes
	/// persisted by SaveData must therefore be indented and must differ from what the facade
	/// produces. If SessionStorage were reverted to <see cref="DataTagSerializer"/> /
	/// <see cref="DataContext.Default"/>, the stored text would be compact and this test would fail.
	/// </summary>
	[Fact]
	public void SaveData_UsesOwnedContextsSerializerOptions_NotTheStaticFacade()
	{
		var data = new DataContext();
		data.JsonSerializerOptions.WriteIndented = true;

		var provider = new MemoryFileProvider();
		var storage = new StorageManager("data", provider, logger: null, data: data);
		var session = storage.OpenSession("scoping");

		var tag = SampleTag();
		var path = session.GetPath("indented.json");
		session.SaveData(path, tag);

		var stored = path.ReadAllText();

		// The owned context wrote indented JSON (newlines present).
		Assert.Contains("\n", stored);
		// It is byte-for-byte what the owned context produces...
		Assert.Equal(data.Serialize(tag), stored);
		// ...and strictly different from what the static facade (DataContext.Default) would write,
		// so this would fail if storage reverted to the facade.
		Assert.NotEqual(DataTagSerializer.Serialize(tag), stored);
	}

	[Fact]
	public void DefaultContext_PreservesFacadeJsonShape()
	{
		var provider = new MemoryFileProvider();
		// No data context supplied: StorageManager defaults to DataContext.Default (facade parity).
		var storage = new StorageManager("data", provider);
		var session = storage.OpenSession("scoping");

		var tag = SampleTag();
		var path = session.GetPath("default.json");
		session.SaveData(path, tag);

		// Behavior preserved: the persisted bytes equal the legacy static facade's output.
		Assert.Equal(DataTagSerializer.Serialize(tag), path.ReadAllText());
	}
}
