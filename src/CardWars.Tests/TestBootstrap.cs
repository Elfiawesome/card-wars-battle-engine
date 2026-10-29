using System.Runtime.CompilerServices;
using CardWars.BattleEngine;
using CardWars.BattleEngine.Vanilla;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Tags;

namespace CardWars.Tests;

internal static class TestBootstrap
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		DataTagTypeRegistry.Clear();
		DataTagTypeRegistry.ScanAssembly(typeof(DataTag).Assembly);
		DataTagTypeRegistry.ScanAssembly(typeof(CardWars.BattleEngine.BattleEngine).Assembly);
		DataTagTypeRegistry.ScanAssembly(typeof(VanillaMod).Assembly);
		DataTagTypeRegistry.ScanAssembly(typeof(TestBootstrap).Assembly);
	}
}
