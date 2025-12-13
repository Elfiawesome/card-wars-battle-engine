namespace CardWars.BattleEngine.State;

public abstract class EntityState<TId>
	where TId : struct
{
	public readonly TId Id;

	public EntityState(TId id)
	{
		Id = id;
	}
}