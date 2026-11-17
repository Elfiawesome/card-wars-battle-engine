using CardWars.ModLoader;

namespace CardWars.Server;

public interface IServerMod : IModEntry
{
	// Name/Version are inherited from IModEntry (IServerMod previously declared neither).
	public void OnLoad(Server registry, List<ModContentResult> modContents);
}