namespace CardWars.Server.Session;

public interface IServerInstanceProvider<TId>
	where TServerInstance : IServerInstance
	where TId : notnull // So for worlds we can have a record or a resourceid/string to identify different worlds. Then battles can just be identified with guids.
{
	IServerInstance Create(TId instanceId);
	void Save(IServerInstance instance);
}

// Some kind of strongly typed one here?
// public interface IServerInstanceProvider<T>