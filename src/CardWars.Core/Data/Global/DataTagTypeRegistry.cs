using System.Reflection;

namespace CardWars.Core.Data.Global;

/// <summary>
/// TEMPORARY Phase 1 compatibility shim. Delegates every call to
/// <see cref="DataContext.Default"/>, which now owns the id&lt;-&gt;Type maps as instance
/// state (doc.md §5, AGENT.md R1). Kept so existing callers keep compiling unchanged while
/// the data layer migrates to per-instance <see cref="DataContext"/> ownership; slated for
/// removal in a later phase.
/// </summary>
public static class DataTagTypeRegistry
{
	/// <summary>
	/// Scans an assembly for all types decorated with [DataTagType] and registers them.
	/// Call this when loading mod assemblies.
	/// </summary>
	public static void ScanAssembly(Assembly assembly) => DataContext.Default.ScanAssembly(assembly);

	public static void Register<T>(string id) => DataContext.Default.Register<T>(id);

	public static void Register(Type type, string id) => DataContext.Default.Register(type, id);

	public static Type? GetType(string typeId) => DataContext.Default.GetType(typeId);

	public static string? GetTypeId(Type type) => DataContext.Default.GetTypeId(type);

	public static void Clear() => DataContext.Default.Clear();
}
