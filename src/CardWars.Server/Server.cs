using CardWars.BattleEngine;

namespace CardWars.Server;

public class Server
{
	// --- Registry ---
	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();

	public Server() { }

	public void LoadMod(IServerMod mod) => mod.OnLoad(Registry);
	public void LoadMod(IBattleEngineMod mod) => mod.OnLoad(SharedBattleEngineRegistry);
}