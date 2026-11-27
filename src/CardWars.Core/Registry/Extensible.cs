namespace CardWars.Core.Registry;

public class Extensible<T>
	where T : class
{
	private readonly Dictionary<Type, T> _extensions = [];

	public IReadOnlyCollection<T> Values => _extensions.Values;

	public void Register<TT>(TT extension) where TT : T
		=> _extensions[typeof(TT)] = extension;

	public void Register(Type type, T extension)
		=> _extensions[type] = extension;

	public TT? Get<TT>() where TT : T
		=> _extensions.TryGetValue(typeof(TT), out var extension) && extension is TT typed ? typed : default;

	public TT GetOrCreate<TT>() where TT : T, new()
	{
		if (Get<TT>() is { } existing) return existing;

		var created = new TT();
		Register(created);
		return created;
	}

	public void Clear() => _extensions.Clear();
}