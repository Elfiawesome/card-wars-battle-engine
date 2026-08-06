namespace CardWars.Server.Session;

public interface IServerInstanceProvider
{
	IServerInstance Create(object id);
	void Save(IServerInstance instance);
}

public abstract class ServerInstanceProvider<TServerInstance, TId> : IServerInstanceProvider
	where TServerInstance : IServerInstance
	where TId : notnull
{
	IServerInstance IServerInstanceProvider.Create(object id) => Create((TId)id);
	void IServerInstanceProvider.Save(IServerInstance instance) => Save((TServerInstance)instance);

	protected abstract TServerInstance Create(TId id);
	protected abstract void Save(TServerInstance instance);
}
