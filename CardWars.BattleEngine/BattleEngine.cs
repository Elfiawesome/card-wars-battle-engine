using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	public BattleEngineRegistry Registry = new();
	public GameState State = new();
	private Transaction? _transaction = null;

	public void LoadMod(IBattleEngineMod mod)
	{
		mod.OnLoad(Registry);
	}

	public void HandleInput(IInput input)
	{
		if (_transaction == null)
		{
			_transaction = new Transaction() { Registry = Registry, State = State };
		}
		_transaction.ProcessInput(input);

		if (_transaction.IsIdle) { _transaction = null; }
	}
}