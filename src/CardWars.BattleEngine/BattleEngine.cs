using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;
using CardWars.ModLoader;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	public BattleEngineRegistry Registry = new();
	public GameState State = new();
	private Transaction? _transaction = null;
	public Action<BlockBatch>? OnBlockBatchEvent;

	public BattleEngine()
	{
	}

	public void LoadMod(IBattleEngineMod mod, List<ModContentResult> modContents)
	{
		mod.OnLoad(Registry, modContents);
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