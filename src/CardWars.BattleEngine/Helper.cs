using System.Text.Json;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine;

// TODO: Get rid. Its a test helper for now
public static class Helper
{
	public static string SerializeBlock(IBlock block)
	{
		return JsonSerializer.Serialize(block, block.GetType());
	}

	// Debug/test dump. Takes the owning DataContext so it serializes through the engine's own
	// context rather than the static facade; it has no engine-runtime call site.
	public static string GameStateDump(GameState state, DataContext data)
	{
		CompoundTag gameState = new();
		gameState.Set("turn_state", data.ToTag(state.Turn));

		ListTag entities = new();
		foreach (var entity in state.All)
		{
			entities.Add(data.ToTag(entity));
		}
		gameState.Set("entities", entities);

		var options = new JsonSerializerOptions { };
		options.Converters.Add(new DataTagJsonConverter());
		return JsonSerializer.Serialize(gameState, options);
	}
}