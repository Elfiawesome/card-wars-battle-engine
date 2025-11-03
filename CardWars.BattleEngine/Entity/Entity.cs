namespace CardWars.BattleEngine.Entity;

public abstract class Entity<TId>
	where TId : struct
{
	public readonly TId Id;
	public readonly EntityService EntityService;
	public bool IsDestroyed { get; private set; } = false;

	public Entity(EntityService service, TId id)
	{
		Id = id;
		EntityService = service;
		Ready(service, id);
	}

	public virtual void Ready(EntityService service, TId id) { }

	public void Destroy()
	{
		IsDestroyed = true;
	}
}