using CardWars.ModLoader;

namespace CardWars.Client;

public interface IClientMod : IModEntry
{
	string ModName { get; }
	string Version { get; }

	public void OnLoad(ClientRegistry registry);
};