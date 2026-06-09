using System.Reflection;
using System.Runtime.Loader;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Core.Storage;

namespace CardWars.ModLoader;

public enum ModLoadState { Discovered, DependenciesResolved, AssemblyLoaded, PreLoaded, Loaded, PostLoaded, Failed }

public class LoadedMod
{
	public required IModManifest Manifest { get; init; }
	public required StoragePath RootPath { get; init; }
	public List<Assembly> Assemblies { get; set; } = [];
	public ModLoadState State { get; set; } = ModLoadState.Discovered;
}


public class ModLoader
{
	private readonly IEnumerable<StoragePath> _modDirs;
	private Dictionary<string, LoadedMod> _mods = [];
	private readonly List<string> _loadOrder = [];

	public ModLoader(StoragePath modDir) { _modDirs = [modDir]; }
	public ModLoader(IEnumerable<StoragePath> modDirs)
	{
		_modDirs = modDirs;
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

			var manifestPath = modDir.Combine("mod.json");
			if (!manifestPath.Exists) continue;

			Logger.Info($"Mod found in '{modDir.FullPath}'");

			var manifest = ModManifest.Load(manifestPath);
			_mods[manifest.Id] = new()
			{
				Manifest = manifest,
				RootPath = modDir
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
			var codeDir = mod.RootPath.Combine("code");
			if (!codeDir.Exists) continue;

			foreach (var dllPath in codeDir.GetFiles("*.dll"))
			{
				Logger.Info($"Loading mod dll from '{dllPath}'");
				try
				{
					using var stream = dllPath.OpenRead();
					mod.Assemblies.Add(loadContext.LoadFromStream(stream));
					mod.State = ModLoadState.AssemblyLoaded;
					Logger.Info($"Successfully loaded dll assembly for '{dllPath.FullPath}'");
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
		Logger.Info($"Locating mod entry of type [{typeof(TModEntry)}]");
		List<TModEntry> modEntries = [];
		foreach (var modId in _loadOrder)
		{
			Logger.Info($"Scanning [{typeof(TModEntry)}] in [{modId}]...");
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
			var contentDir = mod.RootPath.Combine("content").Combine(side);
			if (!contentDir.Exists) continue;

			foreach (var (filePath, relPath) in contentDir.Walk())
			{
				var dir = filePath.GetDirectoryName(relPath);
				var stem = filePath.GetFileNameWithoutExtension(relPath);
				var sanePath = dir.Length > 0 ? $"{dir}/{stem}" : stem;

				var id = new ResourceId(modId, sanePath);
				var parts = sanePath.Split('/'); // TODO FIX DONT USE '/'
				var category = parts.Length > 1 ? parts[..^1] : [];

				yield return new ModContentResult()
				{
					Id = id,
					FilePath = filePath,
					Category = category
				};
			}
		}
	}
}

public class ModContentResult
{
	public required ResourceId Id;
	public required StoragePath FilePath;
	public required string[] Category;

	public Stream OpenStream() => FilePath.OpenRead();
	public string ReadAllText() => FilePath.ReadAllText();

	public T ReadAs<T>()
	{
		var json = ReadAllText();
		var tag = Core.Data.DataTagSerializer.Deserialize<Core.Data.DataTag>(json);
		if (tag is Core.Data.CompoundTag ct)
			return Core.Data.DataTagMapper.FromTag<T>(ct);
		throw new InvalidOperationException($"Content '{Id}' is not a CompoundTag.");
	}
}
