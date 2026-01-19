namespace CardWars.BattleEngine.Core.Registry;

public class Registry<TId, T>
	where TId : notnull
	where T : notnull
{
	private readonly Dictionary<TId, T> _registry = [];

	public void Register(TId id, T item) => _registry[id] = item;

	public T? Get(TId id) => _registry.TryGetValue(id, out var v) ? v : default;
}