namespace CardWars.BattleEngine.State.Entity;

public abstract class EntityState<TId>
	where TId : IStateId
{
	public readonly TId Id;

	public EntityState(TId id)
	{
		Id = id;
	}
}

public interface IStateId
{
	public Guid Id { get; set; }
}