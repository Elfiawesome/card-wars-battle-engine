using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Behaviour;

// Needs a new naming...
// This is for any data-driven behaviour
public class DataDrivenBehaviour : IBehaviour
{
	public Type ListeningEventType { get; } = typeof(IEvent);
	public int Priority { get; set; } = 0;


	public BehaviourResult Resume(IInput input, BehaviourContext context) { return BehaviourResult.Complete; }
	public BehaviourResult Start(IEvent evnt, BehaviourContext context) { return BehaviourResult.Complete; }
}