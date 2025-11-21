using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Entity;

public abstract class Entity<TId> : IEventListener
	where TId : struct
{
	public readonly TId Id;
	public readonly EntityService EntityService;
	public bool IsDestroyed { get; private set; } = false;

	public int EventPriority => 0;

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

	public virtual void OnGameEvent(BattleEngine engine, IEvent gameEvent, out EventResponse eventResponse) { eventResponse = new(); }
}