using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Storage;
using CardWars.ModLoader;

namespace CardWars.Client.Runtime.Application;

public sealed class GameApplication
{
	public IFileProvider Provider { get; }
	public string GamedataPath { get; }
	public DataTagContext DataTags { get; }
	public Logger Logger { get; }
	public StorageManager Storage { get; }
	public ModRepository Mods { get; }

	public GameApplication(IFileProvider provider, string gamedataPath, string dataTagNamespace, ILogSink logSink)
	{
		Provider = provider;
		GamedataPath = provider.GetFullPath(gamedataPath);
		Logger = new Logger(logSink);
		DataTags = new DataTagContext(dataTagNamespace);
		Storage = new StorageManager(GamedataPath, provider, DataTags, Logger);
		Mods = new ModRepository(Storage.GlobalModDirectories, DataTags, Logger);
		Mods.Setup();
	}

	public SessionRuntime CreateSession(
		IClientRuntimeFactory clientFactory,
		string username,
		SessionMode mode,
		string sessionName,
		string targetIpAddress,
		int targetPort)
		=> new(this, clientFactory, username, mode, sessionName, targetIpAddress, targetPort);
}
