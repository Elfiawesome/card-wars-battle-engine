using System.Reflection;
using System.Runtime.Loader;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Serialization.Json;
using CardWars.Core.Data.Tags;
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


public class ModRepository
{
	private readonly IEnumerable<StoragePath> _modDirs;
	private readonly Dictionary<string, LoadedMod> _mods = [];
	private readonly List<string> _loadOrder = [];
	private readonly Dictionary<(string ContentRoot, string? AssetRoot), ModContentFiles> _content = [];

	public ModRepository(StoragePath modDir) { _modDirs = [modDir]; }
	public ModRepository(IEnumerable<StoragePath> modDirs)
	{
		_modDirs = modDirs;
	}

	public void Setup()
	{
		DiscoverMods();
		ResolveDependencies();
		LoadAssemblies();
		DataTagTypeRegistry.ScanLoadedAssemblies();
	}

	public void DiscoverMods()
	{
		foreach (var modDir in _modDirs)
		{
			if (!modDir.Exists) continue;

			var manifestPath = modDir.Combine("mod.json");
			if (!manifestPath.Exists) continue;

			Log.Info($"Mod found in '{modDir.FullPath}'");

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
		var loadContext = AssemblyLoadContext.GetLoadContext(typeof(ModRepository).Assembly) ?? AssemblyLoadContext.Default;

		var dllPathsBySimpleName = new Dictionary<string, StoragePath>(StringComparer.OrdinalIgnoreCase);
		foreach (var modId in _loadOrder)
		{
			var codeDir = _mods[modId].RootPath.Combine("code");
			if (!codeDir.Exists) continue;
			foreach (var dllPath in codeDir.GetFiles("*.dll"))
			{
				dllPathsBySimpleName.TryAdd(dllPath.GetFileNameWithoutExtension(), dllPath);
			}
		}

		loadContext.Resolving += (_, assemblyName) =>
		{
			if (assemblyName.Name is not { } name) return null;
			if (!dllPathsBySimpleName.TryGetValue(name, out var dllPath)) return null;
			using var stream = dllPath.OpenRead();
			return loadContext.LoadFromStream(stream);
		};

		foreach (var modId in _loadOrder)
		{
			var mod = _mods[modId];
			var codeDir = mod.RootPath.Combine("code");
			if (!codeDir.Exists) continue;

			foreach (var dllPath in codeDir.GetFiles("*.dll"))
			{
				if (mod.Assemblies.Any(a => a.Location == dllPath.FullPath)) continue;
				Log.Info($"Loading mod dll from '{dllPath}'");
				try
				{
					using var stream = dllPath.OpenRead();
					var assembly = loadContext.LoadFromStream(stream);
					mod.Assemblies.Add(assembly);
					mod.State = ModLoadState.AssemblyLoaded;
					Log.Info($"Successfully loaded dll assembly for '{dllPath.FullPath}'");
				}
				catch (Exception ex)
				{
					mod.State = ModLoadState.Failed;
					Log.Error($"Failed to load dll assembly for '{dllPath}': {ex.Message}");
				}
			}
			
		}
	}

	public List<TModEntry> LoadModEntry<TModEntry>()
		where TModEntry : IModEntry
	{
		Log.Info($"Locating mod entry of type [{typeof(TModEntry)}]");
		List<TModEntry> modEntries = [];
		foreach (var modId in _loadOrder)
		{
			Log.Info($"Scanning [{typeof(TModEntry)}] in [{modId}]...");
			var mod = _mods[modId];

			foreach (var assembly in mod.Assemblies)
			{
				assembly.GetTypes()
					.Where(t => typeof(TModEntry).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
					.ToList()
					.ForEach(t =>
					{
						Log.Info($"Found mod entry type '{typeof(TModEntry)}': '{t.FullName}'");
						var modeEntry = (TModEntry?)Activator.CreateInstance(t);
						if (modeEntry is not null)
						{
							modEntries.Add(modeEntry);
						}
					});
			}
		}
		return modEntries;
	}

	public ModContentFiles GetContent(string contentRoot, string? assetRoot = null)
	{
		var key = (contentRoot, assetRoot);
		if (_content.TryGetValue(key, out var cached)) return cached;

		var data = new List<ModContentResult>();
		var assets = new List<ModContentResult>();

		foreach (var modId in _loadOrder)
		{
			var mod = _mods[modId];
			foreach (var contentPath in mod.Manifest.ContentPaths)
			{
				var rootDir = mod.RootPath.Combine(contentPath).Combine(contentRoot);
				if (!rootDir.Exists) continue;

				LoadContentFiles(modId, rootDir, assetRoot, data, assets);
			}
		}

		var result = new ModContentFiles(data, assets);
		_content[key] = result;
		Log.Info($"Content loaded [{contentRoot}]: data {data.Count}, assets {assets.Count}");
		return result;
	}

	private static void LoadContentFiles(
		string modId,
		StoragePath rootDir,
		string? assetRoot,
		List<ModContentResult> data,
		List<ModContentResult> assets)
	{
		foreach (var (filePath, relPath) in rootDir.Walk())
		{
			var parts = filePath.SplitPath(relPath);
			var isAsset = false;
			var idPath = relPath;
			if (assetRoot is { } assetFolder && parts.Length > 1 && parts[0] == assetFolder)
			{
				isAsset = true;
				idPath = relPath[(assetFolder.Length + 1)..];
			}

			var idParts = isAsset ? parts[1..] : parts;
			if (idParts.Length == 0) continue;

			var dir = filePath.GetDirectoryName(idPath);
			var stem = filePath.GetFileNameWithoutExtension(idPath);
			var sanePath = dir.Length > 0 ? filePath.JoinPath(dir, stem) : stem;
			var category = idParts.Length > 1 ? idParts[..^1] : [];

			var result = new ModContentResult
			{
				Id = new ResourceId(modId, sanePath),
				FilePath = filePath,
				Category = category
			};

			if (isAsset)
			{
				assets.Add(result);
			}
			else
			{
				data.Add(result);
			}
		}
	}
}

public sealed record ModContentFiles(
	IReadOnlyList<ModContentResult> Data,
	IReadOnlyList<ModContentResult> Assets);

public class ModContentResult
{
	public required ResourceId Id;
	public required StoragePath FilePath;
	public required string[] Category;

	public Stream OpenStream() => FilePath.OpenRead();
	public string ReadAllText() => FilePath.ReadAllText();

	public byte[] ReadAllBytes()
	{
		using var stream = OpenStream();
		using var memory = new MemoryStream();
		stream.CopyTo(memory);
		return memory.ToArray();
	}

	public T? ReadAs<T>()
		where T : DataTag
	{
		var json = ReadAllText();
		var tag = DataTagSerializer.Deserialize<T>(json);
		return tag;
	}

	public T ReadAsObject<T>()
	{
		var json = ReadAllText();
		var tag = DataTagSerializer.Deserialize<CompoundTag>(json)
			?? throw new InvalidOperationException($"Invalid Serialization as object for '{Id}'");
		return DataTagMapper.FromTag<T>(tag);
	}
}
