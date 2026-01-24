using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardWars.ModLoader;

public class ModManifest : IModManifest
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("version")]
	public string Version { get; set; } = "1.0.0";

	[JsonPropertyName("entry_point")]
	public string? EntryPoint { get; set; }

	[JsonPropertyName("dependencies")]
	public List<ModDependency> DependenciesList { get; set; } = [];

	[JsonIgnore]
	public IReadOnlyList<ModDependency> Dependencies => DependenciesList;

	[JsonPropertyName("load_after")]
	public List<string> LoadAfterList { get; set; } = [];

	[JsonIgnore]
	public IReadOnlyList<string> LoadAfter => LoadAfterList;

	[JsonPropertyName("load_before")]
	public List<string> LoadBeforeList { get; set; } = [];

	[JsonIgnore]
	public IReadOnlyList<string> LoadBefore => LoadBeforeList;

	[JsonPropertyName("content_paths")]
	public List<string> ContentPathsList { get; set; } = ["content"];

	[JsonIgnore]
	public IReadOnlyList<string> ContentPaths => ContentPathsList;

	public static ModManifest Load(string jsonPath)
	{
		var json = File.ReadAllText(jsonPath);
		return JsonSerializer.Deserialize<ModManifest>(json)
			?? throw new InvalidOperationException($"Failed to parse mod manifest: {jsonPath}");
	}
}