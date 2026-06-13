using System.Linq;
using CardWars.Core.Data;
using CardWars.Core.Logging;
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
	public StoragePath UsernamesFile { get; }

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
		UsernamesFile = root.Combine("usernames.json");
	}

	public void EnsureDirectories()
	{
		if (!ModsDir.Exists) ModsDir.CreateDirectory();
		if (!WorldsDir.Exists) WorldsDir.CreateDirectory();
		if (!PlayersDir.Exists) PlayersDir.CreateDirectory();
	}

	public StoragePath GetPath(string relativePath)
		=> Root.Combine(relativePath);


	// --- WORLDS ---
	public DataTag? LoadAndCreateWorld(ResourceId worldId)
	{
		var WorldFile = WorldsDir.Combine(worldId.ToFlatString()).WithExtension("json");
		if (WorldFile.Exists)
		{
			// TODO
		}
		else
		{
		}
		return null;
	}


	// --- PLAYER ---
	public DataTag? LoadPlayer(Guid playerId)
	{
		var playerSave = PlayersDir.Combine($"{playerId}").WithExtension("json");
		if (playerSave.Exists) { return DataTagSerializer.Deserialize<CompoundTag>(playerSave.ReadAllText()); }
		return null;
	}

	public void SavePlayer(Guid playerId, CompoundTag data)
	{
		var playerSave = PlayersDir.Combine($"{playerId}").WithExtension("json");
		playerSave.WriteAllText(DataTagSerializer.Serialize(data));
	}

	// --- USERNAMES ---
	private CompoundTag? GetUsernameFile()
		=> UsernamesFile.Exists
		? DataTagSerializer.Deserialize<CompoundTag>(UsernamesFile.ReadAllText()) ?? null
		: null;

	public void SetUsernameFromPlayerId(string username, Guid id)
	{
		var usernames = GetUsernameFile() ?? new CompoundTag();
		usernames.Set(username, id);
		UsernamesFile.WriteAllText(DataTagSerializer.Serialize(usernames));
	}

	public Guid? UsernameToPlayerId(string username)
	{
		var file = GetUsernameFile();
		if (file == null || !file.Has(username)) return null;
		return file.GetGuid(username);
	}
}

// Save
// Load
// List
// Exists
