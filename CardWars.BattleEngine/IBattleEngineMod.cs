namespace CardWars.BattleEngine;

public interface IBattleEngineMod
{
	string ModName { get; }
	string Version { get; }

	public void OnLoad(BattleEngineRegistry registry);
}