using CardWars.Core.Storage;

namespace CardWars.Server.Session;

public interface IServerInstanceProvider
{
	// Pass Id -> get template/load instance
	IServerInstance Create(string id);

	// Saves the id and passes the reference (if have any)
	string? Save(IServerInstance serverInstance, StoragePath InstanceStoragePath);
}

public abstract class ServerInstanceProvider<TServerInstance> : IServerInstanceProvider
	where TServerInstance : IServerInstance
{
	public abstract TServerInstance Create(string id);
	public abstract string? Save(TServerInstance serverInstance, StoragePath InstanceStoragePath);

	IServerInstance IServerInstanceProvider.Create(string id) => Create(id);

	string? IServerInstanceProvider.Save(IServerInstance serverInstance, StoragePath InstanceStoragePath)
	{
		if (serverInstance is TServerInstance typedInstance)
			return Save(typedInstance, InstanceStoragePath);

		// Optional: throw an exception if the type doesn't match
		throw new ArgumentException(
			$"Expected instance of type {typeof(TServerInstance).Name}, " +
			$"but got {serverInstance.GetType().Name}.");
	}
}
