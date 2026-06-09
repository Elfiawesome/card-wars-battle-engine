using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.Core.Storage;

public class StorageManager
{
	public string DataRoot { get; }
	public StoragePath ModsDir { get; }
	public StoragePath SavesDir { get; }
	public StoragePath SettingsFile { get; }

	public SessionStorage? CurrentSession { get; private set; }

	internal IFileProvider Provider { get; }

	private Dictionary<string, StoragePath> _modDirsById = [];
	// private bool _modDirsResolved;

	private Dictionary<(string Side, string Namespace, string Path), StoragePath> _contentCache = [];
	// private bool _contentIndexed;

	public StorageManager(string dataRoot, IFileProvider provider)
	{
		DataRoot = provider.GetFullPath(dataRoot);
		Provider = provider;

		ModsDir = new StoragePath(provider.Combine(DataRoot, "mods"), provider);
		SavesDir = new StoragePath(provider.Combine(DataRoot, "saves"), provider);
		SettingsFile = new StoragePath(provider.Combine(DataRoot, "settings.json"), provider);

		EnsureDirectories();
	}

	private void EnsureDirectories()
	{
		if (!ModsDir.Exists) ModsDir.CreateDirectory();
		if (!SavesDir.Exists) SavesDir.CreateDirectory();
	}

	public SessionStorage OpenSession(string sessionName)
	{
		if (CurrentSession != null)
			CloseSession();

		var sessionRoot = SavesDir.Combine(sessionName);
		if (!sessionRoot.Exists)
			sessionRoot.CreateDirectory();

		var modsDir = sessionRoot.Combine("mods");
		if (!modsDir.Exists)
			modsDir.CreateDirectory();

		CurrentSession = new SessionStorage(this, sessionName, sessionRoot);
		// _modDirsResolved = false;
		// _contentIndexed = false;
		Logger.Info($"Storage: Opened session '{sessionName}' at '{sessionRoot.FullPath}'");
		return CurrentSession;
	}

	public void CloseSession()
	{
		if (CurrentSession != null)
		{
			Logger.Info($"Storage: Closed session '{CurrentSession.SessionName}'");
			CurrentSession = null;
			// _modDirsResolved = false;
			// _contentIndexed = false;
		}
	}

	public StoragePath[] GlobalModDirectories => ModsDir.GetDirectories();
	public StoragePath[] SessionModDirectories => CurrentSession?.ModsDir.GetDirectories() ?? [];
	public StoragePath[] AllModDirectories => [.. GlobalModDirectories, .. SessionModDirectories];


	// public string[] ListSessions()
	// 	=> SavesDir.GetDirectories()
	// 		.Select(d => d.Name)
	// 		.ToArray();

	// public string[] ListGlobalMods()
	// 	=> ModsDir.GetDirectories()
	// 		.Select(d => d.Name)
	// 		.ToArray();

	// public StoragePath? GetGlobalModPath(string modId)
	// {
	// 	var path = ModsDir.Combine(modId);
	// 	return path.Exists ? path : null;
	// }

	// public StoragePath[] GetModDirectories()
	// {
	// 	var dirs = new List<StoragePath>();

	// 	foreach (var modDir in ModsDir.GetDirectories())
	// 		dirs.Add(modDir);

	// 	if (CurrentSession != null)
	// 	{
	// 		if (CurrentSession.ModsDir.Exists)
	// 		{
	// 			foreach (var modDir in CurrentSession.ModsDir.GetDirectories())
	// 				dirs.Add(modDir);
	// 		}
	// 	}

	// 	return dirs.ToArray();
	// }

	// public string ReadSettings()
	// {
	// 	if (SettingsFile.Exists)
	// 		return SettingsFile.ReadAllText();
	// 	return "{}";
	// }

	// public void WriteSettings(string json)
	// 	=> SettingsFile.WriteAllText(json);

	// public StoragePath? ResolveContent(ResourceId resourceId, string side = "server")
	// {
	// 	EnsureContentIndexed();
	// 	var key = (side, resourceId.Namespace, resourceId.Path);
	// 	return _contentCache.TryGetValue(key, out var path) ? path : null;
	// }

	// public Stream? OpenContent(ResourceId resourceId, string side = "server")
	// {
	// 	var path = ResolveContent(resourceId, side);
	// 	return path?.OpenRead();
	// }

	// public string? ReadContentText(ResourceId resourceId, string side = "server")
	// {
	// 	var path = ResolveContent(resourceId, side);
	// 	if (path == null) return null;
	// 	var result = path.ReadAllText();
	// 	return result;
	// }

	// public T? ReadContentAs<T>(ResourceId resourceId, string side = "server")
	// {
	// 	var json = ReadContentText(resourceId, side);
	// 	if (json == null) return default;
	// 	var tag = DataTagSerializer.Deserialize<DataTag>(json);
	// 	if (tag is CompoundTag ct)
	// 		return DataTagMapper.FromTag<T>(ct);
	// 	return default;
	// }

	// private void EnsureModDirsResolved()
	// {
	// 	if (_modDirsResolved) return;
	// 	_modDirsResolved = true;
	// 	_modDirsById = [];

	// 	foreach (var modDir in GetModDirectories())
	// 	{
	// 		var manifestPath = modDir.Combine("mod.json");
	// 		if (!manifestPath.Exists) continue;

	// 		try
	// 		{
	// 			var json = manifestPath.ReadAllText();
	// 			var tag = DataTagSerializer.Deserialize<DataTag>(json);
	// 			if (tag is CompoundTag ct && ct.Get("id") is StringTag idTag)
	// 				_modDirsById[idTag.Value] = modDir;
	// 		}
	// 		catch
	// 		{
	// 		}
	// 	}
	// }

	// private void EnsureContentIndexed()
	// {
	// 	if (_contentIndexed) return;
	// 	_contentIndexed = true;
	// 	_contentCache = [];

	// 	EnsureModDirsResolved();

	// 	foreach (var (modId, modDir) in _modDirsById)
	// 	{
	// 		IndexContentSide(modId, modDir, "server");
	// 		IndexContentSide(modId, modDir, "client");
	// 	}
	// }

	// private void IndexContentSide(string modId, StoragePath modDir, string side)
	// {
	// 	var contentDir = modDir.Combine("content").Combine(side);
	// 	if (!contentDir.Exists) return;

	// 	foreach (var (file, relPath) in contentDir.Walk())
	// 	{
	// 		var dir = file.GetDirectoryName(relPath);
	// 		var stem = file.GetFileNameWithoutExtension(relPath);
	// 		var sanePath = dir.Length > 0 ? $"{dir}/{stem}" : stem;
	// 		var key = (side, modId, sanePath);
	// 		_contentCache[key] = file;
	// 	}
	// }
}
