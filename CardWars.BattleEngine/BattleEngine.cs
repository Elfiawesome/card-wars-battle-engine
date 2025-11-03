using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private readonly BlockDispatcher _blockDispatcher = new();
	private readonly InputDispatcher _inputDispatcher = new();

	private readonly EntityService _entityService = new();
	private readonly TurnService _turnService = new();


	private readonly List<ResolverBase> _resolverStack = [];


	public BattleEngine()
	{
		_blockDispatcher.Register();
		_inputDispatcher.Register();
	}

	public void HandleInput(PlayerId playerId,IInput input)
	{
		if (!_turnService.IsPlayerInputAllowed(playerId)) { return; }
		_inputDispatcher.Handle(this, input);

		if (_resolverStack.Count > 0)
		{
			_resolverStack[0].HandleInput(this, input);
		}
	}

	public void HandleBlock(IBlock block)
	{
		_blockDispatcher.Handle(this, block);
	}

	public void QueueResolver(ResolverBase resolver)
	{
		resolver.OnCommited += (blockBatches) => blockBatches.ForEach(
			(BlockBatch) =>
			{
				BlockBatch.Actions.ForEach(
					(block) =>
					{
						HandleBlock(block);
					}
				);
			}
		);
		resolver.OnResolved += () =>
		{
			_resolverStack.Remove(resolver);
		};

		if (!resolver.IsResolved)
		{
			_resolverStack.Add(resolver);
		}
	}

	public PlayerId AddPlayer()
	{
		var playerId = new PlayerId(Guid.NewGuid());
		var player = new Player(_entityService, playerId);
		_entityService.Players.Add(playerId, player);
		_turnService.AddPlayer(playerId);
		return playerId;
	}
}
