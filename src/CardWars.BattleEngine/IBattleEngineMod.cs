using CardWars.ModLoader;

namespace CardWars.BattleEngine;

public interface IBattleEngineMod : IModEntry
{
	string ModName { get; }
	string Version { get; }

	public void OnLoad(BattleEngineRegistry registry);
}