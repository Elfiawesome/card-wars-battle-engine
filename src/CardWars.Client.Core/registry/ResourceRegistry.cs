using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scripts.core.registry;

public class ResourceRegistry<TId> : Registry<TId, Resource>
	where TId : notnull
{
	public void Register(TId id, string resPath)
	{
		Register(id, GD.Load<Resource>(resPath));
	}

	public virtual TResource? Instantiate<TResource>(TId id)
		where TResource : Resource
	{
		var r = Get(id);
		if (r is TResource tr) return tr;
		return null;
	}
}