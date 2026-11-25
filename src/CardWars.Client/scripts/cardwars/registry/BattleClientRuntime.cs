using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla;

namespace CardWars.Client.scripts.cardwars.registry;

public sealed class BattleClientRuntime(BattleEngineRegistry engine)
{
	public BattleEngineRegistry Engine { get; } = engine;
	public CardWarsDefinitions? Definitions { get; } = engine.GetExtension<CardWarsDefinitions>();
}
