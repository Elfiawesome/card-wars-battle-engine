using System.Reflection;

namespace CardWars.ModLoader;

public enum ModLoadState { Discovered, DependenciesResolved, AssemblyLoaded, PreLoaded, Loaded, PostLoaded, Failed }

public class LoadedMod
{
	public required IModManifest Manifest { get; init; }
	public required string RootPath { get; init; }
	public Assembly? Assembly { get; set; }
	public ModLoadState State { get; set; } = ModLoadState.Discovered;
}


public class ModLoader
{
	private string _modDir;
	private Dictionary<string, LoadedMod> _mods = [];
	private readonly List<string> _loadOrder = [];

	public ModLoader(string modDir)
	{
		_modDir = modDir;
	}

	public void DiscoverMods()
	{
		foreach (var dir in Directory.GetDirectories(_modDir))
		{
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
		foreach (var modId in _loadOrder)
		{
			var mod = _mods[modId];
			var codeDir = Path.Combine(mod.RootPath, "code");

			if (!Directory.Exists(codeDir)) continue;

			foreach (var dllPath in Directory.GetFiles(codeDir, "*.dll"))
			{
				try
				{
					mod.Assembly = Assembly.LoadFrom(dllPath);
					mod.State = ModLoadState.AssemblyLoaded;
				}
				catch (Exception ex)
				{
					mod.State = ModLoadState.Failed;
					Console.WriteLine($"Failed to load assembly for {modId}: {ex.Message}");
				}
			}
		}
	}

	public List<TModEntry> LoadModEntry<TModEntry>()
		where TModEntry : IModEntry
	{
		List<TModEntry> modEntries = [];
		foreach (var modId in _loadOrder)
		{
			var mod = _mods[modId];
			if (mod.Assembly == null) continue;

			mod.Assembly.GetTypes().Where((t) => typeof(TModEntry).IsAssignableFrom(t)).ToList().ForEach(
				(t) =>
				{
					var modeEntry = (TModEntry?)Activator.CreateInstance(t);
					if (modeEntry != null)
					{
						modEntries.Add(modeEntry);
					}
				});
		}
		return modEntries;
	}
}

