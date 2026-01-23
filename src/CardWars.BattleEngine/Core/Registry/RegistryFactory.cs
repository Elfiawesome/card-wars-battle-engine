namespace CardWars.BattleEngine.Core.Registry;

public class RegistryFactory<TId, T>
	where TId : notnull
	where T : notnull
{
	private readonly Dictionary<TId, Func<T>> _registry = [];

	public void Register<TT>(TId id) where TT : T, new() => _registry[id] = () => new TT();

	public T? Get(TId id)
	{
		_registry.TryGetValue(id, out var f);
		if (f == null) { return default; }
		return f.Invoke();
	}
}