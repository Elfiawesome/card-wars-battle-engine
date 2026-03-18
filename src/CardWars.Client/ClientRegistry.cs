using CardWars.Core.Registry;

namespace CardWars.Client;

public class ClientRegistry
{
	public Registry<ResourceId, object> SomeRegistryTest { get; } = new(); 
}
