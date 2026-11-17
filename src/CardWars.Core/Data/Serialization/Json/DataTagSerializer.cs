using System.Text.Json;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Serialization.Json;

/// <summary>
/// TEMPORARY Phase 1 compatibility shim. Delegates every call to
/// <see cref="DataContext.Default"/>, which now owns its own
/// <see cref="System.Text.Json.JsonSerializerOptions"/> and <see cref="DataTagJsonConverter"/>
/// as instance state (doc.md §5, AGENT.md R1). Kept so existing callers keep compiling
/// unchanged while the data layer migrates to per-instance <see cref="DataContext"/>
/// ownership; slated for removal in a later phase.
/// </summary>
public static class DataTagSerializer
{
	public static JsonSerializerOptions JsonSerializerOptions => DataContext.Default.JsonSerializerOptions;

	public static T? Deserialize<T>(string json)
		where T : DataTag
		=> DataContext.Default.Deserialize<T>(json);

	public static string Serialize(DataTag tag) => DataContext.Default.Serialize(tag);
}
