using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private readonly BlockDispatcher _blockDispatcher = new();
	private readonly InputDispatcher _inputDispatcher = new();
	private readonly List<Resolver.Resolver> _resolverStack = [];


	public BattleEngine()
	{
		_blockDispatcher.Register();
		_inputDispatcher.Register();
	}

	public void HandleInput(IInput input)
	{
		_inputDispatcher.Handle(this, input);
	}

	public void QueueResolver()
	{
		
	}
}
