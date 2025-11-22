using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Resolver;

public abstract class ResolverBase
{
	public event Action<List<BlockBatch>>? OnCommited;
	public event Action<ResolverBase>? OnResolverQueued;
	public event Action<IEvent, EventResponse>? OnEventRaised;
	public event Action? OnResolved;

	public bool IsResolved = false;
	public EventResponse? EventResponse;

	private readonly List<BlockBatch> _batch = [];

	public abstract void HandleStart(BattleEngine engine);
	public virtual void HandleInput(BattleEngine engine, IInput input) { throw new NotImplementedException(); }
	public virtual void HandleEnd(BattleEngine engine) { throw new NotImplementedException(); }

	protected void AddBlockBatch(BlockBatch batch)
	{
		_batch.Add(batch);
	}

	protected void QueueResolver(ResolverBase resolver)
	{
		OnResolverQueued?.Invoke(resolver);
	}

	protected void RaiseEvent(IEvent evnt)
	{
		EventResponse ??= new();
		OnEventRaised?.Invoke(evnt, EventResponse);
	}
	protected void RaiseEvent(IEvent evnt, EventResponse response)
	{
		OnEventRaised?.Invoke(evnt, response);
	}

	protected void Commit()
	{
		OnCommited?.Invoke(_batch);
		_batch.Clear();
	}

	protected void Resolved()
	{
		OnResolved?.Invoke();
		IsResolved = true;
	}

	protected void CommitResolved()
	{
		Commit();
		Resolved();
	}
}