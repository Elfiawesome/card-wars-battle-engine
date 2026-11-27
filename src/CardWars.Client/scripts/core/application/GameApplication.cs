using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using Godot;

namespace CardWars.Client.scripts.core.application;

public sealed class GameApplication
{
	public IFileProvider Provider { get; }
	public string GamedataPath { get; }
	public StorageManager Storage { get; }
	public ModRepository Mods { get; }

	public GameApplication(IFileProvider provider, string gamedataPath)
	{
		Provider = provider;
		GamedataPath = provider.GetFullPath(gamedataPath);
		Storage = new StorageManager(GamedataPath, provider);
		Mods = new ModRepository(Storage.AllModDirectories);
		Mods.Setup();
	}

	public SessionRuntime CreateSession(
		Node root,
		string username,
		SessionMode mode,
		string targetIpAddress,
		int targetPort)
		=> new(this, root, username, mode, targetIpAddress, targetPort);

	public IEnumerable<TMod> CreateModEntries<TMod>()
		where TMod : class, IModEntry
		=> typeof(GameApplication).Assembly
			.GetTypes()
			.Where(type => typeof(TMod).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
			.Select(type => Activator.CreateInstance(type))
			.OfType<TMod>();
}
