using System.Reflection;
using System.Runtime.Loader;
using CardWars.Core.FileSystem;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.ModLoader;

public enum ModLoadState { Discovered, DependenciesResolved, AssemblyLoaded, PreLoaded, Loaded, PostLoaded, Failed }

public class LoadedMod
{
	public required IModManifest Manifest { get; init; }
	public required IPathAddr RootPath { get; init; }
	public List<Assembly> Assemblies { get; set; } = [];
	public ModLoadState State { get; set; } = ModLoadState.Discovered;
}


public class ModLoader
{
	private readonly IEnumerable<IPathAddr> _modDirs;
	private Dictionary<string, LoadedMod> _mods = [];
	private readonly List<string> _loadOrder = [];

	public ModLoader(IPathAddr modDir) { _modDirs = [modDir]; }
	public ModLoader(IEnumerable<IPathAddr> modDirs)
	{
		_modDirs = modDirs;
		// There are 3 stages for mod loader to work
		// 1. Discover Mods from a directory (This will load the manifest and store them, for loading compatabilities and versioning)
		// 2. Resolve Dependencies. This to ensure all mods have their dependencies. Then sort by load order
		// 3. Load Assemblies. By load order, we load the assembly for each dll
		// 4. Then we can get each mod entry from each assembly
	}

	public void Setup()
	{
		DiscoverMods();
		ResolveDependencies();
		LoadAssemblies();
	}

	public void DiscoverMods()
	{
		foreach (var modDir in _modDirs)
		{
			if (!modDir.Exists) continue;
			foreach (var dir in modDir.GetDirectories())
			{
				Logger.Info($"Mod found in '{dir.Path}'");
				var manifestPath = dir.Combine("mod.json");
				if (!manifestPath.Exists) continue;

				var manifest = ModManifest.Load(manifestPath);
				_mods[manifest.Id] = new()
				{
					Manifest = manifest,
					RootPath = dir
				};
			}
		}
	}

	public void ResolveDependencies()
	{
		var resolved = new List<string>();
		var unresolved = new HashSet<string>(_mods.Keys);

		while (unresolved.Count > 0)
		{
			var found = false;
			foreach (var modId in unresolved.ToList())
			{
				var mod = _mods[modId];
				var deps = mod.Manifest.Dependencies
					.Where(d => !d.Optional)
					.Select(d => d.ModId);
				var loadAfter = mod.Manifest.LoadAfter;

				var allResolved = deps.All(d => resolved.Contains(d) || !_mods.ContainsKey(d))
					&& loadAfter.All(la => resolved.Contains(la) || !_mods.ContainsKey(la));

				if (allResolved)
				{
					resolved.Add(modId);
					unresolved.Remove(modId);
					mod.State = ModLoadState.DependenciesResolved;
					found = true;
				}
			}

			if (!found && unresolved.Count > 0)
			{
				throw new InvalidOperationException(
					$"Circular dependency detected. Unresolved: {string.Join(", ", unresolved)}");
			}
		}

		_loadOrder.Clear();
		_loadOrder.AddRange(resolved);
	}

	public void LoadAssemblies()
	{
		var loadContext = AssemblyLoadContext.GetLoadContext(typeof(ModLoader).Assembly) ?? AssemblyLoadContext.Default;

		foreach (var modId in _loadOrder)
		{
			var mod = _mods[modId];
			var codeDir = mod.RootPath.Combine("code");
			if (codeDir.Exists) continue;

			foreach (var dllPath in codeDir.GetFiles("*.dll"))
			{
				Logger.Info($"Loading mod dll from '{dllPath}'");
				try
				{
					using var stream = dllPath.OpenRead();
					mod.Assemblies.Add(loadContext.LoadFromStream(stream));
					mod.State = ModLoadState.AssemblyLoaded;
					Logger.Info($"Successfully loaded dll assembly for '{dllPath.Path}'");
				}
				catch (Exception ex)
				{
					mod.State = ModLoadState.Failed;
					Logger.Error($"Failed to load dll assembly for '{dllPath}': {ex.Message}");
				}
			}
		}
	}

	public List<TModEntry> LoadModEntry<TModEntry>()
		where TModEntry : IModEntry
	{
		Logger.Info($"Locating mod entry of type '{typeof(TModEntry)}'");
		List<TModEntry> modEntries = [];
		foreach (var modId in _loadOrder)
		{
			Logger.Info($"Scanning [{modId}]...");
			var mod = _mods[modId];

			foreach (var assembly in mod.Assemblies)
			{
				assembly.GetTypes()
					.Where(t => typeof(TModEntry).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
					.ToList()
					.ForEach(t =>
					{
						Logger.Info($"Found mod entry type '{typeof(TModEntry)}': '{t.FullName}'");
						var modeEntry = (TModEntry?)Activator.CreateInstance(t);
						if (modeEntry != null)
						{
							modEntries.Add(modeEntry);
						}
					});
			}
		}
		return modEntries;
	}

	public IEnumerable<ModContentResult> GetContentServer() => GetContent("server");
	public IEnumerable<ModContentResult> GetContentClient() => GetContent("client");


	private IEnumerable<ModContentResult> GetContent(string side)
	{
		foreach (var modId in _loadOrder)
		{
			var mod = _mods[modId];
			var contentDir = mod.RootPath.Combine("content", side);
			if (!contentDir.Exists) continue;

			foreach (var (filePath, relPath) in GetAllFiles(contentDir))
			{
				var sanePath = relPath;
				if (Path.HasExtension(sanePath))
				{
					sanePath = sanePath.Substring(0, sanePath.LastIndexOf('.'));
				}
				sanePath = sanePath.Replace('\\', '/');

				var id = new ResourceId(modId, sanePath);
				var parts = sanePath.Split('/');
				var category = parts.Length > 1 ? parts[..^1] : [];

				yield return new ModContentResult()
				{
					Id = id,
					Stream = filePath.OpenRead(),
					FilePath = filePath.Path,
					Category = category
				};
			}
		}
	}

	private IEnumerable<(IPathAddr file, string relPath)> GetAllFiles(IPathAddr dir, string currentRel = "")
	{
		foreach (var file in dir.GetFiles())
		{
			yield return (file, string.IsNullOrEmpty(currentRel) ? file.Name : $"{currentRel}/{file.Name}");
		}

		foreach (var subDir in dir.GetDirectories())
		{
			var nextRel = string.IsNullOrEmpty(currentRel) ? subDir.Name : $"{currentRel}/{subDir.Name}";
			foreach (var item in GetAllFiles(subDir, nextRel))
			{
				yield return item;
			}
		}
	}
}

public class ModContentResult : IDisposable
{
	public required ResourceId Id;
	public required Stream Stream;
	public required string FilePath;
	public required string[] Category;
	public string FileType => Path.GetExtension(FilePath);

	public void Dispose() => Stream?.Dispose();
}
