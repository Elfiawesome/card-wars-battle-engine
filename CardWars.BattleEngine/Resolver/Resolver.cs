using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Resolver;

public abstract class Resolver
{
	public IServiceContainer? ServiceContainer;
	public bool IsResolved = false;
	public BlockBatch? _currentBlockBatch;

	public abstract void HandleStart();
	public virtual void HandleInput(PlayerId playerId, IInput input) { }

	public BlockBatch Open()
	{
		if (_currentBlockBatch == null)
		{
			_currentBlockBatch = new BlockBatch();
			return _currentBlockBatch;
		}
		return _currentBlockBatch;
	}

	public void Commit()
	{
		if (_currentBlockBatch == null) { return; }
		ServiceContainer?.BlockDispatcher.Handle(ServiceContainer, _currentBlockBatch);
		_currentBlockBatch = null;
	}

	protected void Resolved()
	{
		IsResolved = true;
	}

	protected void CommitResolved()
	{
		Commit();
		Resolved();
	}
}