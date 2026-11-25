using CardWars.ModLoader;

namespace CardWars.Server;

public interface IServerMod : IModEntry
{
	public void OnLoad(Server registry, IReadOnlyList<ModContentResult> modContents);
}