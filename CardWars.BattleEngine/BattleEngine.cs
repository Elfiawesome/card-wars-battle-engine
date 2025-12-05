using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Definition;
using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	public event Action<IBlock>? OnBlockEvent;

	public readonly EntityService EntityService = new();
	public readonly TurnService TurnService = new();
	public readonly EventService EventService;
	public readonly DefinitionService DefinitionService = new();

	private readonly BlockDispatcher _blockDispatcher = new();
	private readonly InputDispatcher _inputDispatcher = new();

	private readonly List<ResolverBase> _resolverQueue = [];


	public BattleEngine()
	{
		EventService = new(this); // Improve better way to do this?
		_blockDispatcher.Register();
		_inputDispatcher.Register();
	}

	public void HandleInput(PlayerId playerId, IInput input)
	{
		if (!TurnService.IsPlayerInputAllowed(playerId)) { return; }
		_inputDispatcher.Handle(new InputHandlerContext(this, playerId), input);

		if (_resolverQueue.Count > 0)
		{
			_resolverQueue[0].HandleInput(this, input);
		}
	}

	public void HandleBlock(IBlock block)
	{
		if (!_blockDispatcher.Handle(this, block))
		{
			Console.WriteLine("!BLOCK UNHANDLED!: " + block.GetType().Name);
		}
		else
		{
			// NEW: Invoke the event
			OnBlockEvent?.Invoke(block);
		}
	}

	public void QueueResolver(ResolverBase resolver)
	{
		// Can we check if this is bad for memeory or somethin?
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
			_resolverQueue.Remove(resolver);
			HandleResolver();
		};
		resolver.OnResolverQueued += QueueResolver;
		resolver.OnEventRaised += EventService.Raise;

		_resolverQueue.Add(resolver);
		HandleResolver();
	}

	private void HandleResolver()
	{
		if (_resolverQueue.Count > 0)
		{
			Console.WriteLine("Handling Resolver: " + _resolverQueue[0].GetType().Name);
			_resolverQueue[0].HandleStart(this);
		}
	}

	public PlayerId AddPlayer()
	{
		var playerId = new PlayerId(Guid.NewGuid());
		QueueResolver(new PlayerJoinedResolver(playerId));
		return playerId;
	}
}
