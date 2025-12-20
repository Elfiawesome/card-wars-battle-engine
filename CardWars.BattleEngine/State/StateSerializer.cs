using System.Text.Json;
using System.Text.Json.Serialization;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.State;

public static class StateSerializer
{
	public static string ToJson(StateService state)
	{
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		};
		options.Converters.Add(new EntityIdJsonConverterFactory());
		return JsonSerializer.Serialize(state, options);

	}

	private class EntityIdJsonConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(IStateId).IsAssignableFrom(typeToConvert);
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			Type converterType = typeof(EntityIdConverter<>).MakeGenericType(typeToConvert);
			return (JsonConverter)Activator.CreateInstance(converterType)!;
		}

		private class EntityIdConverter<T> : JsonConverter<T> where T : IStateId
		{
			public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.Id.ToString());
			}

			public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(value.Id.ToString());
			}
			
			public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType != JsonTokenType.String)
					throw new JsonException();

				var guid = Guid.Parse(reader.GetString()!);

				return (T)Activator.CreateInstance(typeof(T), guid)!;
			}

			public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var guid = Guid.Parse(reader.GetString()!);
				return (T)Activator.CreateInstance(typeof(T), guid)!;
			}
		}
	}
}