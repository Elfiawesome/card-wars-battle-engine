using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Resolver;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public interface IServiceContainer
{
	public StateService State { get; set; }
	public ResolverService Resolver { get; set; }
	public EventService EventService { get; set; }
	public BlockDispatcher BlockDispatcher { get; set; }
	public InputDispatcher InputDispatcher { get; set; }
	public EventResolverDispatcher EventResolverDispatcher { get; set; }
}