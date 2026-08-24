using CardWars.Core.Data;

namespace CardWars.Server.Session;

public interface IServerInstanceProvider
{
	IServerInstance GetOrCreate(string id);
	IServerInstance Deserialize(CompoundTag data, string id);
	CompoundTag Serialize(IServerInstance instance);
}

public abstract class ServerInstanceProvider<TServerInstance, TId> : IServerInstanceProvider
	where TServerInstance : IServerInstance
	where TId : notnull
{
	private readonly Dictionary<TId, TServerInstance> _cache = new();

	IServerInstance IServerInstanceProvider.GetOrCreate(string id)
	{
		var typedId = Parse(id);
		if (_cache.TryGetValue(typedId, out var existing))
			return existing;

		var instance = Create(typedId);
		_cache[typedId] = instance;
		return instance;
	}

	IServerInstance IServerInstanceProvider.Deserialize(CompoundTag data, string id)
	{
		var typedId = Parse(id);
		if (_cache.TryGetValue(typedId, out var cached))
			return cached;

		var instance = Deserialize(data, typedId);
		_cache[typedId] = instance;
		return instance;
	}

	CompoundTag IServerInstanceProvider.Serialize(IServerInstance instance) => Serialize((TServerInstance)instance);

	protected abstract TServerInstance Create(TId id);
	protected abstract TServerInstance Deserialize(CompoundTag data, TId id);
	protected abstract CompoundTag Serialize(TServerInstance instance);
	protected abstract TId Parse(string id);
}
