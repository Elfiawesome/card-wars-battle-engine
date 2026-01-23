using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Behaviour;

public class BehaviourContext
{
	private readonly Transaction _transaction;
	private readonly List<BlockBatch> _stagedBlocks = new();

	public GameState State { get; }
	public EntityId OwnerEntityId { get; init; }

	internal BehaviourContext(Transaction transaction, GameState state, EntityId owner)
	{
		_transaction = transaction;
		State = state;
		OwnerEntityId = owner;
	}

	// Read owner entity
	public IEntity? Owner => State.Get(OwnerEntityId);
	public T? OwnerAs<T>() where T : class, IEntity => State.Get<T>(OwnerEntityId);

	// --- Block Management ---

	public void StageBlocks(BlockBatch batch) => _stagedBlocks.Add(batch);

	public void StageBlock(IBlock block, Guid targetPlayerId, string animationId = "")
		=> _stagedBlocks.Add(new BlockBatch([block], targetPlayerId, animationId));

	public void CommitStagedBlocks()
	{
		foreach (var batch in _stagedBlocks)
			_transaction.ApplyBlockBatch(batch);
		_stagedBlocks.Clear();
	}

	public void CommitBlocks(BlockBatch batch)
	{
		StageBlocks(batch);
		CommitStagedBlocks();
	}

	public void RaiseEvent(IEvent evnt) => _transaction.QueueEvent(evnt);
}