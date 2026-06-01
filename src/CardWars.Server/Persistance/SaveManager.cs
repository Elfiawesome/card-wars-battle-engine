using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Storage;

namespace CardWars.Server.Persistance;

public class SaveManager
{
	public SaveState SaveState = new();
	private readonly SessionStorage _session;

	public SaveManager(SessionStorage session)
	{
		_session = session;
		DataTagTypeRegistry.ScanAssembly(typeof(SaveManager).Assembly);
		Load();
	}

	public void Load()
	{
		if (_session.SaveExists)
		{
			try
			{
				SaveState = _session.LoadSave<SaveState>();
			}
			catch (Exception ex)
			{
				Logger.Warn($"Failed to load save, creating new: {ex.Message}");
				CreateDefaultSave();
			}
		}
		else
		{
			CreateDefaultSave();
		}
	}

	private void CreateDefaultSave()
	{
		SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 20 });
		SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 3 });
		SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 0 });
		SaveState.PlayerSaves.Add(Guid.NewGuid(), new PlayerSaveState() { Position = 100 });
		Save();
	}

	public void Save()
	{
		_session.SaveSave(SaveState);
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
