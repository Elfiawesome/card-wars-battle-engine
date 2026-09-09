using System;
using System.Collections.Generic;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.registry;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.core;

public class ClientRegistry
{
	public HandlerRegistry<PacketContextClient> PacketHandlers = new();
	public SceneRegistry<ResourceId> Instances = new();
	public SceneRegistry<ResourceId> UserInterface = new();
	public SceneRegistry<ResourceId> GameObjects = new();

	private readonly Dictionary<Type, IClientRegistryExtension> _extensions = [];

	public void RegisterExtension<T>(T registryExtension) where T : IClientRegistryExtension
		=> _extensions[typeof(T)] = registryExtension;

	public T? GetExtension<T>() where T : IClientRegistryExtension
	{
		if (_extensions.TryGetValue(typeof(T), out var ext))
		{
			return (T)ext;
		}
		return default;
	}
}

public interface IClientRegistryExtension;