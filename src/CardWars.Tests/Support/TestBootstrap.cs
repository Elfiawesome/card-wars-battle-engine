using System.Runtime.CompilerServices;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.Tests.Support;

internal static class TestBootstrap
{
	public static DataTagContext DataTags { get; } = new();
	public static Logger Logger { get; } = new(new NullLogSink());

	[ModuleInitializer]
	internal static void Initialize() => DataTags.ScanLoadedAssemblies();
}
