using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Resolver;

public abstract class Resolver
{
	public event Action<List<BlockBatch>>? OnCommited;
	public event Action? OnResolved;
	
	private readonly List<BlockBatch> _batch = [];

	public abstract void HandleStart(BattleEngine engine);
	public virtual void HandleInput(BattleEngine engine, IInput input) { throw new NotImplementedException(); }
	public virtual void HandleEnd(BattleEngine engine) { throw new NotImplementedException(); }

	protected void AddActionBatch(BlockBatch batch)
	{
		_batch.Add(batch);
	}

	protected void Commit()
	{
		OnCommited?.Invoke(_batch);
		_batch.Clear();
	}

	protected void Resolved()
	{
		OnResolved?.Invoke();
	}
}