using System.Text.Json;
using CardWars.BattleEngine.Block;

namespace CardWars.BattleEngine;

// TODO: Get rid. Its a test helper for now
public static class Helper
{
	public static string SerializeBlock(IBlock block)
	{
		return JsonSerializer.Serialize(block, block.GetType());
	}
}