using CardWars.Core.Data;

namespace CardWars.ModLoader;

public class ModManifest : IModManifest
{
	[DataTag("id")] public string Id { get; set; } = "";

	[DataTag("name")] public string Name { get; set; } = "";

	[DataTag("version")] public string Version { get; set; } = "1.0.0";

	[DataTag("entry_point")] public string? EntryPoint { get; set; }

	[DataTag("dependencies")] public List<ModDependency> DependenciesList { get; set; } = [];

	public IReadOnlyList<ModDependency> Dependencies => DependenciesList;

	[DataTag("load_after")] public List<string> LoadAfterList { get; set; } = [];

	public IReadOnlyList<string> LoadAfter => LoadAfterList;

	[DataTag("load_before")] public List<string> LoadBeforeList { get; set; } = [];

	public IReadOnlyList<string> LoadBefore => LoadBeforeList;

	[DataTag("content_paths")] public List<string> ContentPathsList { get; set; } = ["content"];

	public IReadOnlyList<string> ContentPaths => ContentPathsList;

	public static ModManifest Load(string jsonPath)
	{
		var json = File.ReadAllText(jsonPath);
		var tag = DataTagSerializer.Deserialize<DataTag>(json);
		if (tag is CompoundTag compoundTag)
		{
			return DataTagMapper.FromTag<ModManifest>(compoundTag);
		}
		throw new InvalidOperationException($"Failed to parse mod manifest: {jsonPath}");
	}
}