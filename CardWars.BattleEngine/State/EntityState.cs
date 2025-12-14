namespace CardWars.BattleEngine.State;

public abstract class EntityState<TId>
	where TId : EntityId
{
	public readonly TId Id;

	public EntityState(TId id)
	{
		Id = id;
	}
}

public interface EntityId
{
	public Guid Id { get; set; }
}