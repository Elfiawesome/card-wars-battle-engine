using System.Runtime.CompilerServices;
using CardWars.BattleEngine.Vanilla;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;

namespace CardWars.Tests.Support;

internal static class TestBootstrap
{
	// Phase 1: build/populate a DataContext instead of Clear()-ing a process global.
	//
	// The existing tests still reach the data layer through the temporary static facade
	// (DataTagMapper.ToTag, DataTagTypeRegistry.GetType, ...), which reads DataContext.Default.
	// So we populate Default by scanning the four assemblies into it. ScanAssembly/Register
	// overwrite by id, so this is idempotent and does not need an all-or-nothing Clear().
	//
	// Session isolation is proven directly in DataContextIsolationTests against independent
	// DataContext instances. Once every caller takes an explicit context (later phases),
	// this module initializer disappears entirely.
	[ModuleInitializer]
	internal static void Initialize()
	{
		var context = DataContext.Default;
		context.ScanAssembly(typeof(DataTag).Assembly);
		context.ScanAssembly(typeof(BattleEngine.BattleEngine).Assembly);
		context.ScanAssembly(typeof(VanillaMod).Assembly);
		context.ScanAssembly(typeof(TestBootstrap).Assembly);
	}
}
