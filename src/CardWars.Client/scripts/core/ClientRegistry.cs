using System;
using System.Collections.Generic;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.registry;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.Client.scripts.core;

// Phase 6: compiled/validated against Godot 4.7. ClientRegistry stays in the game assembly (not
// CardWars.Client.Core) because it references PacketContextClient, which is bound to the scene
// Node GameSession; the compiler-enforced core/vanilla boundary lives in CardWars.Client.Core,
// which carries the GameSession-free Godot registry/storage classes. The optional ctor logger
// mirrors BattleEngineRegistry(ILogger?) so the owning session can supply its logger and mod-load
// diagnostics reach the session sink instead of always falling back to NullLogger.Instance
// (Phase 3, R9). No mutable static state (R1).
public class ClientRegistry(ILogger? logger = null)
{
	// The owning session's logger (or the immutable no-op NullLogger.Instance when none was
	// supplied). Exposed so client mod-load code (the vanilla mod's resource-registration loop)
	// can route its "Registered ..." diagnostics through the owning session's sink (R9).
	public ILogger Logger { get; } = logger ?? NullLogger.Instance;

	public HandlerRegistry<PacketContextClient> PacketHandlers = new();
	public SceneRegistry<ResourceId> Instances = new();
	public SceneRegistry<ResourceId> UserInterface = new();
	public SceneRegistry<ResourceId> GameObjects = new();
	public ResourceRegistry<ResourceId> Resources = new();

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