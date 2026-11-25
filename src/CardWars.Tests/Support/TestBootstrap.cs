using System.Runtime.CompilerServices;
using CardWars.Core.Data.Global;

namespace CardWars.Tests.Support;

internal static class TestBootstrap
{
	[ModuleInitializer]
	internal static void Initialize()
	{
		DataTagTypeRegistry.Clear();
		DataTagTypeRegistry.ScanLoadedAssemblies();
	}
}
