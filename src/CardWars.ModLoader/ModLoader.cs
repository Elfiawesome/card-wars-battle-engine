using System.Reflection;
using System.Runtime.Loader;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.ModLoader;

public enum ModLoadState { Discovered, DependenciesResolved, AssemblyLoaded, PreLoaded, Loaded, PostLoaded, Failed }

public class LoadedMod
{
	public required IModManifest Manifest { get; init; }
	public required string RootPath { get; init; }
	public List<Assembly> Assemblies { get; set; } = [];
	public ModLoadState State { get; set; } = ModLoadState.Discovered;
}


public class ModLoader
{
	private readonly string _modDir;
	private Dictionary<string, LoadedMod> _mods = [];
	private readonly List<string> _loadOrder = [];

	public ModLoader(string modDir)
	{
		_modDir = modDir;
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
		foreach (var dir in Directory.GetDirectories(_modDir))
		{
			Logger.Info($"Mod found in '{dir}'");
			var manifestPath = Path.Combine(dir, "mod.json");
			var manifest = ModManifest.Load(manifestPath);
			_mods[manifest.Id] = new()
			{
				Manifest = manifest,
				RootPath = dir
			};
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
			var codeDir = Path.Combine(mod.RootPath, "code");

			if (!Directory.Exists(codeDir)) continue;

			foreach (var dllPath in Directory.GetFiles(codeDir, "*.dll"))
			{
				Logger.Info($"Loading mod dll from '{dllPath}'");
				try
				{
					mod.Assemblies.Add(loadContext.LoadFromAssemblyPath(dllPath));
					mod.State = ModLoadState.AssemblyLoaded;
					Logger.Info($"Successfully loaded dll assembly for '{dllPath}'");
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
			var contentDir = Path.Combine(mod.RootPath, "content", side);

			foreach (var filePath in Directory.EnumerateFiles(contentDir, "*.*", SearchOption.AllDirectories))
			{
				var relFilePath = Path.GetRelativePath(contentDir, filePath);
				var sanePath = Path.ChangeExtension(relFilePath, null).Replace("\\", "/");
				var id = new ResourceId(modId, sanePath);

				var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				if (fs == null) { continue; }
				yield return new ModContentResult()
				{
					Id = id,
					Stream = fs,
					FilePath = filePath
				};
			}
		}
	}
}

public class ModContentResult
{
	public required ResourceId Id;
	public required Stream Stream;
	public required string FilePath;
	public string FileType => Path.GetExtension(FilePath);
}

