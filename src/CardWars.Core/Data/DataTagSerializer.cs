using System.Text.Json;

namespace CardWars.Core.Data;

public static class DataTagSerializer
{
	public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions();

	static DataTagSerializer()
	{
		JsonSerializerOptions.Converters.Add(new DataTagJsonConverter());
	}

	public static T? Deserialize<T>(string json)
		where T : DataTag
	{
		var tag = JsonSerializer.Deserialize<DataTag>(json, JsonSerializerOptions);
		if (tag == null) return null;
		return tag as T;
	}

	public static string Serialize(DataTag tag)
	{
		var json = JsonSerializer.Serialize(tag, JsonSerializerOptions);
		return json;
	}
}