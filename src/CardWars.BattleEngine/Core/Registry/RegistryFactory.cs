namespace CardWars.BattleEngine.Core.Registry;

public class RegistryFactory<TId, T>
	where TId : notnull
	where T : notnull
{
	private readonly Dictionary<TId, Func<T>> _registry = [];

	public void Register<TT>(TId id) where TT : T, new() => _registry[id] = () => new TT();

	public T? Create(TId id) =>
		_registry.TryGetValue(id, out var f) ? f.Invoke() : default;
}

public class RegistryFactory<TId, TArg, T>
	where TId : notnull
	where T : notnull
{
	private readonly Dictionary<TId, Func<TArg, T>> _registry = [];

	public void Register(TId id, Func<TArg, T> factory) => _registry[id] = factory;

	public T? Create(TId id, TArg arg) =>
		_registry.TryGetValue(id, out var f) ? f(arg) : default;
}