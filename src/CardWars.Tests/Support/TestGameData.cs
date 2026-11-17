using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.ModLoader;

namespace CardWars.Tests.Support;

/// <summary>
/// Helpers that feed the real vanilla gamedata files (under gamedata/mods/cardwars) into the
/// in-memory load path used by the Vanilla registration tests. Phase 7 moved the card-type
/// defaults out of C# switch arms into content/server/card_defaults/*.json; the oracle tests load
/// those real JSON files through <see cref="MemoryFileProvider"/> + <see cref="ModContentResult"/>
/// so they exercise the actual on-disk template content, not a C# duplicate.
/// </summary>
internal static class TestGameData
{
	private const string VanillaServerContentRelative = "gamedata/mods/cardwars/content/server";

	/// <summary>
	/// Locates the repo-root gamedata/mods/cardwars/content/server directory by walking up from
	/// the test assembly location until the expected path is found.
	/// </summary>
	public static string VanillaServerContentRoot()
	{
		var dir = AppContext.BaseDirectory;
		while (dir is not null)
		{
			var candidate = Path.Combine(dir, VanillaServerContentRelative);
			if (Directory.Exists(candidate)) return candidate;
			dir = Path.GetDirectoryName(dir);
		}

		throw new DirectoryNotFoundException(
			$"Could not locate '{VanillaServerContentRelative}' walking up from '{AppContext.BaseDirectory}'");
	}

	/// <summary>
	/// Reads a card-type default template (unit/hero/spell) verbatim from the real gamedata JSON
	/// file. Throws if the file is missing.
	/// </summary>
	public static string ReadCardDefaultJson(string category)
	{
		var path = Path.Combine(VanillaServerContentRoot(), "card_defaults", $"{category}.json");
		if (!File.Exists(path))
			throw new FileNotFoundException($"Missing card_defaults template: {path}");
		return File.ReadAllText(path);
	}

	/// <summary>
	/// Builds a <see cref="ModContentResult"/> for a card-type default template backed by the real
	/// gamedata JSON content, registered under category ['card_defaults'] exactly as the live mod
	/// loader would yield it.
	/// </summary>
	public static ModContentResult CardDefaultContent(string category, MemoryFileProvider provider)
	{
		var rel = $"card_defaults/{category}.json";
		provider.AddFile(rel, ReadCardDefaultJson(category));
		return new ModContentResult
		{
			Id = ResourceId.Vanilla($"card_defaults/{category}"),
			FilePath = new StoragePath(rel, provider),
			Category = ["card_defaults"]
		};
	}
}
