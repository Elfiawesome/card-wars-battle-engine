using Logger = CardWars.Core.Logging.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using Godot;

namespace CardWars.Client.scripts.core.application;

public sealed class GameApplication
{
	public IFileProvider Provider { get; }
	public string GamedataPath { get; }
	public DataTagContext DataTags { get; }
	public Logger Logger { get; }
	public StorageManager Storage { get; }
	public ModRepository Mods { get; }

	public GameApplication(IFileProvider provider, string gamedataPath, ILogSink logSink)
	{
		Provider = provider;
		GamedataPath = provider.GetFullPath(gamedataPath);
		Logger = new Logger(logSink);
		DataTags = new DataTagContext();
		Storage = new StorageManager(GamedataPath, provider, DataTags, Logger);
		Mods = new ModRepository(Storage.GlobalModDirectories, DataTags, Logger);
		Mods.Setup();
	}

	public SessionRuntime CreateSession(
		Node root,
		string username,
		SessionMode mode,
		string sessionName,
		string targetIpAddress,
		int targetPort)
		=> new(this, root, username, mode, sessionName, targetIpAddress, targetPort);

	public IEnumerable<TMod> CreateModEntries<TMod>()
		where TMod : class, IModEntry
		=> typeof(GameApplication).Assembly
			.GetTypes()
			.Where(type => typeof(TMod).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
			.Select(type => Activator.CreateInstance(type))
			.OfType<TMod>();
}
