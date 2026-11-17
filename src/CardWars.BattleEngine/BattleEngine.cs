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
	/// sites keep working, while a session can supply its own isolated context. As of Phase 2
	/// this context is flowed into the runtime serialize path (Transaction -> BehaviourContext
	/// -> ScriptContext -> vanilla handlers), so assigning a non-default context here actually
	/// changes how this engine resolves types and converters.
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
			_transaction = new Transaction { Registry = Registry, State = State, Data = Data, OnBlockBatchEvent = OnBlockBatchEvent };
		}
		_transaction.ProcessInput(playerId, input);

		if (_transaction.IsIdle) { _transaction = null; }
	}
}