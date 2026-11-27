using CardWars.BattleEngine;
using CardWars.Client.scripts.core.content;
using CardWars.Core.Data;
using CardWars.Core.Storage;

namespace CardWars.Client.scripts.core.application;

public sealed class SessionContext(
	DataTagContext dataTags,
	SessionStorage? storage,
	GameContent content,
	BattleEngineRegistry battleRegistry)
{
	public DataTagContext DataTags { get; } = dataTags;
	public SessionStorage? Storage { get; } = storage;
	public GameContent Content { get; } = content;
	public BattleEngineRegistry BattleRegistry { get; } = battleRegistry;
}
