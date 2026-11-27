using System.Runtime.CompilerServices;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;

namespace CardWars.Tests.Support;

internal static class TestBootstrap
{
	public static DataTagContext DataTags { get; } = new(CardWarsIds.DefaultNamespace);
	public static Logger Logger { get; } = new(new NullLogSink());

	[ModuleInitializer]
	internal static void Initialize() => DataTags.ScanLoadedAssemblies();
}
