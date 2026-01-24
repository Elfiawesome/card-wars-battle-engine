namespace CardWars.ModLoader;

public interface IModManifest
{
	string Id { get; }
	string Name { get; }
	string Version { get; }
	string? EntryPoint { get; }  // Assembly-qualified type name for code mods
	IReadOnlyList<ModDependency> Dependencies { get; }
	IReadOnlyList<string> LoadAfter { get; }
	IReadOnlyList<string> LoadBefore { get; }
	IReadOnlyList<string> ContentPaths { get; }  // Paths to content directories
}

public record struct ModDependency(string ModId, string? MinVersion = null, bool Optional = false);