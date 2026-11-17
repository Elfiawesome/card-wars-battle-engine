using CardWars.Core.Data;
using CardWars.ModLoader;

namespace CardWars.BattleEngine;

public interface IBattleEngineMod : IModEntry
{
	// Name/Version are inherited from IModEntry (the old ModName spelling is removed).
	// The owning DataContext is threaded in so content tags produced at mod-load time serialize
	// through the owner's context rather than the static DataTagMapper facade (Phase 2).
	public void OnLoad(BattleEngineRegistry registry, DataContext data, List<ModContentResult> modContents);
}
