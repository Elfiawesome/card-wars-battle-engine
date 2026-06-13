using CardWars.Core.Data;

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

	public DataTag? LoadData(StoragePath path)
	{
		if (path.Exists) return DataTagSerializer.Deserialize<CompoundTag>(path.ReadAllText());
		return null;
	}

	public void SaveData(StoragePath path, DataTag data)
	{
		path.WriteAllText(DataTagSerializer.Serialize(data));
	}
}