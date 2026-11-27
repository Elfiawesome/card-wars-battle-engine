using CardWars.BattleEngine;
using CardWars.BattleEngine.registry;
using CardWars.Client.Runtime.Content;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Storage;

namespace CardWars.Client.Runtime.Application;

public sealed class SessionContext(
	DataTagContext dataTags,
	Logger logger,
	SessionStorage? storage,
	GameContent content,
	BattleEngineRegistry battleRegistry)
{
	public DataTagContext DataTags { get; } = dataTags;
	public Logger Logger { get; } = logger;
	public SessionStorage? Storage { get; } = storage;
	public GameContent Content { get; } = content;
	public BattleEngineRegistry BattleRegistry { get; } = battleRegistry;
}
