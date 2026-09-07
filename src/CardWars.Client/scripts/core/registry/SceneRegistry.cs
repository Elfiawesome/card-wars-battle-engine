using CardWars.Core.Registry;
using Godot;

namespace CardWars.Client.scripts.core.registry;

public class SceneRegistry<TId> : Registry<TId, PackedScene> where TId : notnull
{
	public void Register(TId id, string resPath)
	{
		Register(id, GD.Load<PackedScene>(resPath));
	}
}