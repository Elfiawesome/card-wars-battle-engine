using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scripts.core.registry;

public class SceneRegistry<TId> : Registry<TId, PackedScene>
	where TId : notnull
{
	public void Register(TId id, string resPath)
	{
		Register(id, GD.Load<PackedScene>(resPath));
	}

	public virtual TNode? Instantiate<TNode>(TId id)
		where TNode : Node
	{
		var packedScene = Get(id);
		var n = packedScene?.Instantiate<TNode>();
		if (n == null) return null;
		return n;
	}
}