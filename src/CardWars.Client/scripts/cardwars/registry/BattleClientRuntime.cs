using CardWars.BattleEngine;
using CardWars.Client.scripts.core;

namespace CardWars.Client.scripts.cardwars.registry;

public sealed class BattleClientRuntime(BattleEngineRegistry engine) : IClientRegistryExtension
{
	public BattleEngineRegistry Engine { get; } = engine;
}
