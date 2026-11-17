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

	/// <summary>
	/// The data context this engine owns. Defaults to <see cref="DataContext.Default"/> so
	/// existing <c>new BattleEngine()</c> / <c>new BattleEngine { Registry = ... }</c> call
	/// sites keep working, while a session can supply its own isolated context. Phase 1 only
	/// makes the engine a context owner; internal serialize calls still route through the
	/// temporary static facade (which reads <see cref="DataContext.Default"/>).
	/// </summary>
	public DataContext Data { get; set; } = DataContext.Default;
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
		if (_transaction is null)
		{
			_transaction = new Transaction { Registry = Registry, State = State, OnBlockBatchEvent = OnBlockBatchEvent };
		}
		_transaction.ProcessInput(playerId, input);

		if (_transaction.IsIdle) { _transaction = null; }
	}
}