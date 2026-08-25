using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class WorldInstanceProvider(WorldRegistry worlds, SessionStorage session)
	: ServerInstanceProvider<WorldInstance>   // note: only one generic argument
{
	public override WorldInstance Create(string id)
	{
		// Load the template if no save, or load the save
		throw new Exception();
	}

	public override string? Save(WorldInstance serverInstance, StoragePath InstanceStoragePath)
	{
		// We can save whatever and however we want under InstanceStoragePath
		return "";
	}
}