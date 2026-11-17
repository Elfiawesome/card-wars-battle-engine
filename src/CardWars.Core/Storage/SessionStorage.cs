using CardWars.Core.Data;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Storage;

public class SessionStorage
{
	public string SessionName { get; }
	public StoragePath Root { get; }
	public StoragePath SaveFile { get; }
	public StoragePath ModsDir { get; }
	public StoragePath InstancesDir { get; }
	public StoragePath PlayersDir { get; }
	public StoragePath UsernamesFile { get; }

	private readonly StorageManager _manager;

	// The owning context whose instance Serialize/Deserialize this session uses (Phase 5 storage
	// scoping), replacing the static DataTagSerializer facade. Supplied by the StorageManager.
	private readonly DataContext _data;

	internal SessionStorage(StorageManager manager, string sessionName, StoragePath root, DataContext data)
	{
		_manager = manager;
		_data = data;
		SessionName = sessionName;

		Root = root;
		SaveFile = root.Combine("save.json");
		ModsDir = root.Combine("mods");
		InstancesDir = root.Combine("instances");
		PlayersDir = root.Combine("players");
		UsernamesFile = root.Combine("usernames.json");
	}

	public void EnsureDirectories()
	{
		if (!ModsDir.Exists) ModsDir.CreateDirectory();
		if (!InstancesDir.Exists) InstancesDir.CreateDirectory();
		if (!PlayersDir.Exists) PlayersDir.CreateDirectory();
	}

	public StoragePath GetPath(string relativePath)
		=> Root.Combine(relativePath);

	public DataTag? LoadData(StoragePath path)
	{
		if (path.Exists) return _data.Deserialize<CompoundTag>(path.ReadAllText());
		return null;
	}

	public void SaveData(StoragePath path, DataTag data)
		=> path.WriteAllText(_data.Serialize(data));

	// --- World ---
	public DataTag? LoadInstance(string name)
		=> LoadData(InstancesDir.Combine(name).WithExtension("json"));

	public void SaveInstance(string name, DataTag data)
		=> SaveData(InstancesDir.Combine(name).WithExtension("json"), data);

	// --- Player ---
	public CompoundTag? LoadPlayer(Guid playerId)
	{
		var d = LoadData(PlayersDir.Combine($"{playerId}").WithExtension("json"));
		if (d is CompoundTag c) return c;
		return null;
	}

	public void SavePlayer(Guid playerId, CompoundTag data)
		=> SaveData(PlayersDir.Combine($"{playerId}").WithExtension("json"), data);

	public void SaveUsernameMapping(string username, Guid id)
	{
		var usernames = LoadUsernames();
		usernames.Set(username, id);
		SaveData(UsernamesFile, usernames);
	}

	public Guid UsernameToPlayerId(string username)
	{
		var file = LoadUsernames();
		if (!file.Has(username)) return Guid.Empty;
		return file.GetGuid(username);
	}

	private CompoundTag LoadUsernames()
	{
		if (UsernamesFile.Exists)
			return _data.Deserialize<CompoundTag>(UsernamesFile.ReadAllText()) ?? new CompoundTag();
		return new CompoundTag();
	}
}