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

	public StoragePath[] GlobalModDirectories => ModsDir.GetDirectories();
	public StoragePath[] SessionModDirectories => CurrentSession?.ModsDir.GetDirectories() ?? [];
	public StoragePath[] AllModDirectories => [.. GlobalModDirectories, .. SessionModDirectories];

}