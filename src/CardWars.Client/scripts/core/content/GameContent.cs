using System;
using System.Collections.Generic;
using CardWars.ModLoader;

namespace CardWars.Client.scripts.core.content;

// A object to contain all specifications on how the mod results is designed to be. 
// Mod Results are agnostic to what a server/client/battle are
public sealed class GameContent
{
	public required IReadOnlyList<ModContentResult> ServerData { get; init; }
	public required IReadOnlyList<ModContentResult> ClientData { get; init; }
	public required IReadOnlyList<ModContentResult> ClientPacks { get; init; }
	public required IReadOnlyList<ModContentResult> BattleData { get; init; }
	public required IReadOnlyList<ModContentResult> BattleAssets { get; init; }

	public static GameContent Load(ModRepository modRepository)
	{
		// Get server content
		var server = modRepository.GetContent("server");

		// Get cleint content
		var client = modRepository.GetContent("client");
		var clientData = new List<ModContentResult>();
		var clientPacks = new List<ModContentResult>();
		foreach (var file in client.Data)
		{
			// Find all pck
			if (file.FilePath.Extension.Equals(".pck", StringComparison.OrdinalIgnoreCase))
				clientPacks.Add(file);
			else
				clientData.Add(file);
		}

		// Get battle content
		var battle = modRepository.GetContent("battle", "assets");

		return new GameContent
		{
			ServerData = server.Data,
			ClientData = clientData,
			ClientPacks = clientPacks,
			BattleData = battle.Data,
			BattleAssets = battle.Assets
		};
	}
}
