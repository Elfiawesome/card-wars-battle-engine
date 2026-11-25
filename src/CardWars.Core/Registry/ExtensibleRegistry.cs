namespace CardWars.Core.Registry;

public abstract class ExtensibleRegistry
{
	private readonly Dictionary<Type, object> _extensions = [];

	public void RegisterExtension<T>(T extension) where T : class
		=> _extensions[typeof(T)] = extension;

	public T? GetExtension<T>() where T : class
		=> _extensions.TryGetValue(typeof(T), out var extension) && extension is T typed ? typed : null;

	public T GetOrCreateExtension<T>() where T : class, new()
	{
		if (GetExtension<T>() is { } existing) return existing;

		var created = new T();
		RegisterExtension(created);
		return created;
	}
}
