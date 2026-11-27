namespace CardWars.Core.Registry;

public abstract class ExtensibleRegistry
{
	private readonly Extensible<object> _extensible = new();

	public void RegisterExtension<T>(T extension) where T : class
		=> _extensible.Register(extension);

	public T? GetExtension<T>() where T : class
		=> _extensible.Get<T>();

	public T GetOrCreateExtension<T>() where T : class, new()
		=> _extensible.GetOrCreate<T>();
}
