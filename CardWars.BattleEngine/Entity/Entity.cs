using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Entity;

public abstract class Entity<TId>
	where TId : struct
{
	public readonly TId Id;

	public Entity(TId id)
	{
		Id = id;
	}
}