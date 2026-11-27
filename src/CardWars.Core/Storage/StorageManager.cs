using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.Core.Storage;

public class StorageManager
{
	public string DataRoot { get; }
	public StoragePath ModsDir { get; }
	public StoragePath SavesDir { get; }
	public StoragePath SettingsFile { get; }
	public DataTagContext DataTags { get; }
	public Logger Logger { get; }

	internal IFileProvider Provider { get; }

	public StorageManager(string dataRoot, IFileProvider provider, DataTagContext dataTags, Logger logger)
	{
		DataTags = dataTags;
		Logger = logger;
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
		var sessionRoot = SavesDir.Combine(sessionName);
		if (!sessionRoot.Exists)
			sessionRoot.CreateDirectory();

		var session = new SessionStorage(sessionName, sessionRoot, DataTags);
		session.EnsureDirectories();
		Logger.Info($"Storage: Opened session '{sessionName}' at '{sessionRoot.FullPath}'");
		return session;
	}

	public StoragePath[] GlobalModDirectories => ModsDir.GetDirectories();
}
