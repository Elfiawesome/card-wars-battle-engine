using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Event.Player;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Resolver;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class BattleEngine : IServiceContainer
{
	public StateService State { get; set; }
	public ResolverService Resolver { get; set; }
	public EventService EventService { get; set; }

	public BlockDispatcher BlockDispatcher { get; set; }
	public InputDispatcher InputDispatcher { get; set; }
	public EventResolverDispatcher EventResolverDispatcher { get; set; }

	public BattleEngine()
	{
		State = new(this);
		Resolver = new(this);
		EventService = new(this);
		BlockDispatcher = new();
		InputDispatcher = new();
		EventResolverDispatcher = new();
		BlockDispatcher.Register();
		InputDispatcher.Register();
		EventResolverDispatcher.Register();
	}

	public PlayerId AddPlayer()
	{
		var newPlayerId = new PlayerId(Guid.NewGuid());
		EventService.Raise(new PlayerJoinedEvent() { PlayerId = newPlayerId });
		return newPlayerId;
	}

	public void HandleInput(PlayerId playerId, IInput input)
	{
		if (!State.AllowedPlayerInputs.Contains(playerId))
		{

		}
		else
		{
			InputDispatcher.Handle(new InputHandlerContext(this, playerId), input);
		}
		// IDK maybe u wanna have handle input even on another players turn????
		Resolver.HandleInput(playerId, input);
	}
}