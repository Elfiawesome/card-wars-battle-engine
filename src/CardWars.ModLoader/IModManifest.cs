using CardWars.Core.Data;

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

[DataTagType]
public record struct ModDependency(
	[property: DataTag()] string ModId,
	[property: DataTag()] string? MinVersion = null,
	[property: DataTag()] bool Optional = false
);