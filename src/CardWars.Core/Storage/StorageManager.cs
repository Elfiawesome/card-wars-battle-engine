using CardWars.Core.Data;
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

	private readonly ILogger _logger;

	// The data context whose instance Serialize/Deserialize the sessions this manager opens use
	// (Phase 5 storage scoping). Optional and defaulting to DataContext.Default so existing
	// positional callers compile unchanged and behavior is preserved (DataContext.Default produces
	// byte-identical JSON to the former static DataTagSerializer facade).
	private readonly DataContext _data;


	public StorageManager(string dataRoot, IFileProvider provider, ILogger? logger = null, DataContext? data = null)
	{
		_logger = logger ?? NullLogger.Instance;
		_data = data ?? DataContext.Default;
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
		if (CurrentSession is not null)
			CloseSession();

		var sessionRoot = SavesDir.Combine(sessionName);
		if (!sessionRoot.Exists)
			sessionRoot.CreateDirectory();

		CurrentSession = new SessionStorage(this, sessionName, sessionRoot, _data);
		CurrentSession.EnsureDirectories();
		_logger.Info($"Storage: Opened session '{sessionName}' at '{sessionRoot.FullPath}'");
		return CurrentSession;
	}

	public void CloseSession()
	{
		if (CurrentSession is not null)
		{
			_logger.Info($"Storage: Closed session '{CurrentSession.SessionName}'");
			CurrentSession = null;
		}
	}

	public StoragePath[] GlobalModDirectories => ModsDir.GetDirectories();
	public StoragePath[] SessionModDirectories => CurrentSession?.ModsDir.GetDirectories() ?? [];
	public StoragePath[] AllModDirectories => [.. GlobalModDirectories, .. SessionModDirectories];

}