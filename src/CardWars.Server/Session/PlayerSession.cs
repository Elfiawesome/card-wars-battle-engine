using CardWars.Core.Data;
using CardWars.Core.Network.Transport;
using CardWars.Core.Registry;

namespace CardWars.Server.Session;

[DataTagType()]
public class PlayerSession()
{
	[DataTag] public Guid PlayerId { get; set; } = Guid.Empty;
	[DataTag] public string Username { get; set; } = "";

	public required IConnection Connection { get; set; }
	public IServerInstance? CurrentInstance { get; set; }

	[DataTag] public string CurrentInstanceSaveName { get; set; } = "";
	[DataTag] public ResourceId CurrentInstanceProvider { get; set; } = ResourceId.Empty;
	[DataTag] public int TimePlayed { get; set; } = 0;

	public Dictionary<ResourceId, IPlayerDataComponent> DataComponentsById { get; set; } = [];

	[DataTag]
	public List<PlayerDataComponentEntry> DataComponents // DO NOT USE THIS PROPERTY DIRECTLY; USE GetSetComponent<T>() INSTEAD
	{
		get => [.. DataComponentsById.Select(kvp => new PlayerDataComponentEntry { Id = kvp.Key, Component = kvp.Value })];
		set => DataComponentsById = value.ToDictionary(x => x.Id, x => x.Component);
	}
	[DataTag] public CompoundTag CustomData { get; set; } = new();


	public T GetSetComponent<T>(ResourceId resourceId) where T : IPlayerDataComponent, new()
	{
		if (DataComponentsById.TryGetValue(resourceId, out var component))
		{
			return component is T typedComponent ? typedComponent
				: throw new InvalidOperationException($"Component for resource ID {resourceId} is not of type {typeof(T).Name}.");
		}
		T newComponent = new T();
		DataComponentsById[resourceId] = newComponent;
		return newComponent;
	}
}

public class PlayerDataComponentEntry
{
	[DataTag] public required ResourceId Id { get; set; }
	[DataTag] public required IPlayerDataComponent Component { get; set; }
}

[DataTagType()] public class IPlayerDataComponent { }