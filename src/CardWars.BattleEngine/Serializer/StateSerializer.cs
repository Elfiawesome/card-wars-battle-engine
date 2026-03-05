using System.Text.Json;
using System.Text.Json.Serialization;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Serializer;

public class StateJsonConverter : JsonConverter<EntityId>
{
	public override EntityId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.String => EntityId.None,
			_ => throw new JsonException($"Unexpected token: {reader.TokenType}")
		};
	}

	public override void Write(Utf8JsonWriter writer, EntityId value, JsonSerializerOptions options)
	{
		switch (value)
		{
			case EntityId t: writer.WriteStringValue(value.Value); break;
		}
	}
}