using System;
using System.Text.Json;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Data.Serialization.Json;

// Temporary pure JSON helper. Remove once logging no longer depends on static DataTag access.
internal static class DataTagJson
{
	private static readonly JsonSerializerOptions _options = CreateOptions();

	public static T? Deserialize<T>(string json)
		where T : DataTag
	{
		var tag = JsonSerializer.Deserialize<DataTag>(json, _options);
		if (tag is null && string.Equals(json.Trim(), "null", StringComparison.Ordinal))
			tag = new NullTag();
		return tag is T typed ? typed : null;
	}

	public static string Serialize(DataTag tag)
		=> JsonSerializer.Serialize(tag, _options);

	private static JsonSerializerOptions CreateOptions()
	{
		var options = new JsonSerializerOptions();
		options.Converters.Add(new DataTagJsonConverter());
		return options;
	}
}
