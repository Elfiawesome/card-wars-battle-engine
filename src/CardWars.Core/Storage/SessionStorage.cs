using CardWars.Core.Data;

namespace CardWars.Core.Storage;

public class SessionStorage
{
	public string SessionName { get; }
	public StoragePath Root { get; }
	public StoragePath SaveFile { get; }
	public StoragePath ModsDir { get; }
	public StoragePath WorldsDir { get; }

	private readonly StorageManager _manager;

	internal SessionStorage(StorageManager manager, string sessionName, StoragePath root)
	{
		_manager = manager;
		SessionName = sessionName;

		// Initialize Dir
		Root = root;
		SaveFile = root.Combine("save.json");
		ModsDir = root.Combine("mods");
		WorldsDir = root.Combine("worlds");
	}

	public void EnsureDirectories()
	{
		if (!ModsDir.Exists) ModsDir.CreateDirectory();
		if (!WorldsDir.Exists) WorldsDir.CreateDirectory();
	}

	public StoragePath GetPath(string relativePath)
		=> Root.Combine(relativePath);

	// public T LoadSave<T>()
	// {
	// 	if (!SaveFile.Exists)
	// 		throw new InvalidOperationException(
	// 			$"Save file '{SaveFile.FullPath}' does not exist for session '{SessionName}'.");

	// 	var json = SaveFile.ReadAllText();
	// 	var tag = DataTagSerializer.Deserialize<DataTag>(json);
	// 	if (tag is CompoundTag compoundTag)
	// 		return DataTagMapper.FromTag<T>(compoundTag);

	// 	throw new InvalidOperationException($"Save file '{SaveFile.FullPath}' did not deserialize to CompoundTag.");
	// }

	// public void SaveSave<T>(T data)
	// {
	// 	var tag = DataTagMapper.ToTag(data!);
	// 	var json = DataTagSerializer.Serialize(tag);
	// 	SaveFile.WriteAllText(json);
	// }

	public bool SaveExists => SaveFile.Exists;
}
