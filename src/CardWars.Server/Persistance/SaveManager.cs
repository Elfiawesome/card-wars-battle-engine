using CardWars.Core.Data;

namespace CardWars.Server.Persistance;

public class SaveManager
{
	public SaveState SaveState = new();


	// API
	// LoadPlayer
	// SavePlayer
	// LoadWorld
	// SaveWorld

	public SaveManager()
	{
		DataTagTypeRegistry.ScanAssembly(typeof(SaveManager).Assembly);
	}

	public void Load(string path)
	{
		if (File.Exists(path))
		{
			// var dataText = File.ReadAllText(path);
			// var data = DataTagSerializer.Deserialize<DataTag>(dataText);
		}
		else
		{
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() {Position = 20});
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() {Position = 3});
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() {Position = 0});
			SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() {Position = 100});
			Save(path);
		}
	}

	public void Save(string path)
	{
		var data = DataTagSerializer.Serialize(DataTagMapper.ToTag(SaveState));
		File.WriteAllText(path, data);
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