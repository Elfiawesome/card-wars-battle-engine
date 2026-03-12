using System.Text.Json;
using CardWars.BattleEngine.Block;
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
		CompoundTag gameState = new();
		gameState.Set("turn_state", DataTagMapper.ToTag(state.Turn));
		
		ListTag entities = new();
		foreach (var entity in state.All)
		{
			entities.Add(DataTagMapper.ToTag(entity));
		}
		gameState.Set("entities", entities);

		var options = new JsonSerializerOptions { };
		options.Converters.Add(new DataTagJsonConverter());
		return JsonSerializer.Serialize(gameState, options);
	}
}