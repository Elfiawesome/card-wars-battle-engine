using System.Linq;
using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.Core.Storage;

public class SessionStorage
{
	public string SessionName { get; }
	public StoragePath Root { get; }
	public StoragePath SaveFile { get; }
	public StoragePath ModsDir { get; }
	public StoragePath WorldsDir { get; }
	public StoragePath PlayersDir { get; }

	private readonly StorageManager _manager;

	internal SessionStorage(StorageManager manager, string sessionName, StoragePath root)
	{
		_manager = manager;
		SessionName = sessionName;

		// Initialize Directories
		Root = root;
		SaveFile = root.Combine("save.json");
		ModsDir = root.Combine("mods");
		WorldsDir = root.Combine("worlds");
		PlayersDir = root.Combine("players");
	}

	public void EnsureDirectories()
	{
		if (!ModsDir.Exists) ModsDir.CreateDirectory();
		if (!WorldsDir.Exists) WorldsDir.CreateDirectory();
		if (!PlayersDir.Exists) PlayersDir.CreateDirectory();
	}

	public StoragePath GetPath(string relativePath)
		=> Root.Combine(relativePath);
}

// Save
// Load
// List
// Exists
