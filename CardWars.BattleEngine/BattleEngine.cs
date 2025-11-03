using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	public readonly EntityService EntityService = new();
	public readonly TurnService TurnService = new();

	private readonly BlockDispatcher _blockDispatcher = new();
	private readonly InputDispatcher _inputDispatcher = new();

	private readonly List<ResolverBase> _resolverStack = [];


	public BattleEngine()
	{
		_blockDispatcher.Register();
		_inputDispatcher.Register();
	}

	public void HandleInput(PlayerId playerId, IInput input)
	{
		if (!TurnService.IsPlayerInputAllowed(playerId)) { return; }
		_inputDispatcher.Handle(new InputHandlerContext(this, playerId), input);

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
				BlockBatch.Blocks.ForEach(
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
			HandleResolver();
		};
		resolver.OnResolverQueued += QueueResolver;

		_resolverStack.Add(resolver);
		HandleResolver();
	}

	private void HandleResolver()
	{
		if (_resolverStack.Count > 0)
		{
			Console.WriteLine("Handling Resolver: " + _resolverStack[0].GetType().Name);
			_resolverStack[0].HandleStart(this);
		}
	}

	public PlayerId AddPlayer()
	{
		var playerId = new PlayerId(Guid.NewGuid());
		QueueResolver(new PlayerJoinedResolver(playerId));
		return playerId;
	}
}
