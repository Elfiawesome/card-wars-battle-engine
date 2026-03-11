using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	public BattleEngineRegistry Registry = new();
	public GameState State = new();
	private Transaction? _transaction = null;
	public Action<BlockBatch>? OnBlockBatchEvent;

	public BattleEngine()
	{
		// --- DataTag registry for base Battle Engine --- 
		DataTagTypeRegistry.ScanAssembly(typeof(BattleEngine).Assembly);
	}

	public void LoadMod(IBattleEngineMod mod)
	{
		// --- DataTag registry for mods --- 
		DataTagTypeRegistry.ScanAssembly(mod.GetType().Assembly);
		mod.OnLoad(Registry);
	}

	public void HandleInput(EntityId playerId, IInput input)
	{
		if (_transaction == null)
		{
			_transaction = new Transaction { Registry = Registry, State = State, OnBlockBatchEvent = OnBlockBatchEvent };
		}
		_transaction.ProcessInput(playerId, input);

		if (_transaction.IsIdle) { _transaction = null; }
	}
}