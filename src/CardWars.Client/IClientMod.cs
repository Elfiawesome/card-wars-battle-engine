using CardWars.ModLoader;
using System.Collections.Generic;

namespace CardWars.Client;

public interface IClientMod : IModEntry
{
	string ModName { get; }
	string Version { get; }

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents);
};