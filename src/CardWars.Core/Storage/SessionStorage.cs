using CardWars.Core.Data;

namespace CardWars.Core.Storage;

public class SessionStorage
{
	public string SessionName { get; }
	public StoragePath Root { get; }
	public StoragePath SaveFile { get; }
	public StoragePath ModsDir { get; }
	public StoragePath SessionDir { get; }
	public StoragePath PlayersDir { get; }
	public StoragePath UsernamesFile { get; }

	private readonly StorageManager _manager;

	internal SessionStorage(StorageManager manager, string sessionName, StoragePath root)
	{
		_manager = manager;
		SessionName = sessionName;

		Root = root;
		SaveFile = root.Combine("save.json");
		ModsDir = root.Combine("mods");
		SessionDir = root.Combine("session");
		PlayersDir = root.Combine("players");
		UsernamesFile = root.Combine("usernames.json");
	}

	public void EnsureDirectories()
	{
		if (!ModsDir.Exists) ModsDir.CreateDirectory();
		if (!SessionDir.Exists) SessionDir.CreateDirectory();
		if (!PlayersDir.Exists) PlayersDir.CreateDirectory();
	}

	public StoragePath GetPath(string relativePath)
		=> Root.Combine(relativePath);

	public DataTag? LoadData(StoragePath path)
	{
		if (path.Exists) return DataTagSerializer.Deserialize<CompoundTag>(path.ReadAllText());
		return null;
	}

	public void SaveData(StoragePath path, DataTag data)
		=> path.WriteAllText(DataTagSerializer.Serialize(data));

	// --- World ---
	public DataTag? LoadWorld(string name)
		=> LoadData(SessionDir.Combine(name).WithExtension("json"));

	public void SaveWorld(string name, DataTag data)
		=> SaveData(SessionDir.Combine(name).WithExtension("json"), data);

	// --- Player ---
	public DataTag? LoadPlayer(Guid playerId)
		=> LoadData(PlayersDir.Combine($"{playerId}").WithExtension("json"));

	public void SavePlayer(Guid playerId, CompoundTag data)
		=> SaveData(PlayersDir.Combine($"{playerId}").WithExtension("json"), data);

	public void SaveUsernameMapping(string username, Guid id)
	{
		var usernames = LoadUsernames();
		usernames.Set(username, id);
		SaveData(UsernamesFile, usernames);
	}

	public Guid? UsernameToPlayerId(string username)
	{
		var file = LoadUsernames();
		if (!file.Has(username)) return null;
		return file.GetGuid(username);
	}

	private CompoundTag LoadUsernames()
	{
		if (UsernamesFile.Exists)
			return DataTagSerializer.Deserialize<CompoundTag>(UsernamesFile.ReadAllText()) ?? new CompoundTag();
		return new CompoundTag();
	}
}