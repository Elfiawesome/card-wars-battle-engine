using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardWars.Core.Data;

public class DataTagJsonConverter : JsonConverter<DataTag>
{
	public override DataTag? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True or JsonTokenType.False => new BoolTag(reader.GetBoolean()),
			JsonTokenType.String => new StringTag(reader.GetString()!),
			JsonTokenType.Number => reader.TryGetInt32(out var i) ? new IntTag(i) : new FloatTag(reader.GetSingle()),
			JsonTokenType.StartObject => ReadCompound(ref reader, options),
			JsonTokenType.StartArray => ReadList(ref reader, options),
			JsonTokenType.Null => null,
			_ => throw new JsonException($"Unexpected token: {reader.TokenType}")
		};
	}

	private CompoundTag ReadCompound(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		var tag = new CompoundTag();
		while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
		{
			var key = reader.GetString()!;
			reader.Read();
			var value = Read(ref reader, typeof(DataTag), options);
			if (value != null) tag.Set(key, value);
		}
		return tag;
	}

	private ListTag ReadList(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		var tag = new ListTag();
		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			var item = Read(ref reader, typeof(DataTag), options);
			if (item != null) tag.Add(item);
		}
		return tag;
	}

	public override void Write(Utf8JsonWriter writer, DataTag value, JsonSerializerOptions options)
	{
		switch (value)
		{
			case IntTag t: writer.WriteNumberValue(t.Value); break;
			case FloatTag t: writer.WriteNumberValue(t.Value); break;
			case StringTag t: writer.WriteStringValue(t.Value); break;
			case BoolTag t: writer.WriteBooleanValue(t.Value); break;
			case CompoundTag t:
				writer.WriteStartObject();
				foreach (var (key, tag) in t.Entries)
				{
					writer.WritePropertyName(key);
					Write(writer, tag, options);
				}
				writer.WriteEndObject();
				break;
			case ListTag t:
				writer.WriteStartArray();
				foreach (var item in t.Items) Write(writer, item, options);
				writer.WriteEndArray();
				break;
		}
	}
}