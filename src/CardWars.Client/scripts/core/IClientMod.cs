using CardWars.ModLoader;
using System.Collections.Generic;

namespace CardWars.Client.scripts.core;

public interface IClientMod : IModEntry
{
	string ModName { get; }
	string Version { get; }

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents);
};