using System;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private readonly Action<BlockBatch>? _onBlockBatchEvent;
	private Transaction? _transaction;

	public BattleEngine(BattleEngineRegistry registry, Action<BlockBatch>? onBlockBatchEvent = null)
	{
		Registry = registry;
		State = new GameState();
		_onBlockBatchEvent = onBlockBatchEvent;
	}

	public BattleEngineRegistry Registry { get; }
	public GameState State { get; }

	public void HandleInput(EntityId playerId, IInput input)
	{
		if (_transaction is null)
			_transaction = new Transaction(Registry, State, _onBlockBatchEvent);

		_transaction.ProcessInput(playerId, input);

		if (_transaction.IsIdle) { _transaction = null; }
	}
}
