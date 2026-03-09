using System.Text.Json;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Serializer;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine;

// TODO: Get rid. Its a test helper for now
public static class Helper
{
	public static string SerializeBlock(IBlock block)
	{
		return JsonSerializer.Serialize(block, block.GetType());
	}

	public static string GameStateDump(GameState state)
	{
		Dictionary<string, object> data = [];
		Dictionary<Guid, object> Entities = [];
		data.Add("turn_state", state.Turn);

		foreach (var entity in state.All)
		{
			Dictionary<string, object> entityData = new() { 
				{ "type", entity.GetType().Name },
				{ "data", entity } 
			};
			Entities[entity.Id.Value] = entityData;
		}
		data.Add("entities", Entities);

		var options = new JsonSerializerOptions
		{
			IgnoreReadOnlyProperties = true
		};
		options.Converters.Add(new StateJsonConverter());
		options.Converters.Add(new DataTagJsonConverter());
		return JsonSerializer.Serialize(data, options);
	}
}