using CardWars.Core.Data;
using CardWars.Core.FileSystem;

namespace CardWars.Server.Persistance;

public class SaveManager
{
	public SaveState SaveState = new();
	private readonly IFileSystem _fs;

	public SaveManager(IFileSystem fs)
	{
		_fs = fs;
		DataTagTypeRegistry.ScanAssembly(typeof(SaveManager).Assembly);
	}

	public void Load(string filename)
	{
		var file = _fs.GetPath(filename);
		if (file.Exists)
		{
			// var dataText = file.ReadAllText();
			// var data = DataTagSerializer.Deserialize<DataTag>(dataText);
			// SaveState = DataTagMapper.FromTag<SaveState>((CompoundTag)data);
		}
		else
		{
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 20 });
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 3 });
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 0 });
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 100 });
			Save(filename);
		}
	}

	public void Save(string filename)
	{
		var file = _fs.GetPath(filename);
		var data = DataTagSerializer.Serialize(DataTagMapper.ToTag(SaveState));
		file.WriteAllText(data);
	}
}

[DataTagType()]
public class SaveState()
{
	[DataTag] public string SessionName { get; set; } = "Save 1";
	[DataTag] public Dictionary<Guid, WorldState> Worlds { get; set; } = [];
	[DataTag] public Dictionary<Guid, PlayerSaveState> PlayerSaves { get; set; } = [];

}
[DataTagType()]
public class WorldState()
{

}

[DataTagType()]
public class PlayerSaveState()
{
	[DataTag] public int Position { get; set; } = 0;
}