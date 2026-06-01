using CardWars.Core.Logging;

namespace CardWars.Core.Storage;

public class StorageManager
{
	public string DataRoot { get; }
	public StoragePath ModsDir { get; }
	public StoragePath SavesDir { get; }
	public StoragePath SettingsFile { get; }

	public SessionStorage? CurrentSession { get; private set; }

	internal IFileProvider Provider { get; }

	public StorageManager(string dataRoot, IFileProvider provider)
	{
		DataRoot = System.IO.Path.GetFullPath(dataRoot);
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
		Logger.Info($"Storage: Opened session '{sessionName}' at '{sessionRoot.FullPath}'");
		return CurrentSession;
	}

	public void CloseSession()
	{
		if (CurrentSession != null)
		{
			Logger.Info($"Storage: Closed session '{CurrentSession.SessionName}'");
			CurrentSession = null;
		}
	}

	public string[] ListSessions()
		=> SavesDir.GetDirectories()
			.Select(d => d.Name)
			.ToArray();

	public string[] ListGlobalMods()
		=> ModsDir.GetDirectories()
			.Select(d => d.Name)
			.ToArray();

	public StoragePath? GetGlobalModPath(string modId)
	{
		var path = ModsDir.Combine(modId);
		return path.Exists ? path : null;
	}

	public StoragePath[] GetModDirectories()
	{
		var dirs = new List<StoragePath>();

		foreach (var modDir in ModsDir.GetDirectories())
			dirs.Add(modDir);

		if (CurrentSession != null)
		{
			if (CurrentSession.ModsDir.Exists)
			{
				foreach (var modDir in CurrentSession.ModsDir.GetDirectories())
					dirs.Add(modDir);
			}
		}

		return dirs.ToArray();
	}

	public string ReadSettings()
	{
		if (SettingsFile.Exists)
			return SettingsFile.ReadAllText();
		return "{}";
	}

	public void WriteSettings(string json)
		=> SettingsFile.WriteAllText(json);
}
