using CardWars.Core.Data;

namespace CardWars.Server.Session;

public enum InstancePolicy
{
	SingletonByKey,
	Multi,
}

public enum InstancePersistence
{
	Persistent,
	Transient,
}

public sealed class InstanceCreateContext
{
	public required string Kind { get; init; }
	public required string Key { get; init; }
}

public sealed class InstanceLoadContext
{
	public required string Kind { get; init; }
	public required string Key { get; init; }
	public required CompoundTag SavedData { get; init; }
}

public interface IInstanceProvider
{
	IServerInstance Create(InstanceCreateContext ctx);
	IServerInstance Load(InstanceLoadContext ctx);
}

public sealed record InstanceKindDescriptor(
	IInstanceProvider Provider,
	InstancePolicy Policy,
	InstancePersistence Persistence
);

public sealed class InstanceKindRegistry
{
	private readonly Dictionary<string, InstanceKindDescriptor> _kinds = [];

	public void Register(string kindId, InstanceKindDescriptor descriptor)
		=> _kinds[kindId] = descriptor;

	public InstanceKindDescriptor? Get(string kindId)
		=> _kinds.TryGetValue(kindId, out var d) ? d : null;

	public IReadOnlyDictionary<string, InstanceKindDescriptor> All => _kinds;
}
