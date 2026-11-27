using System.Runtime.CompilerServices;
using CardWars.Core.Data;

namespace CardWars.Tests.Support;

internal static class TestBootstrap
{
	public static DataTagContext DataTags { get; } = new();

	[ModuleInitializer]
	internal static void Initialize() => DataTags.ScanLoadedAssemblies();
}
