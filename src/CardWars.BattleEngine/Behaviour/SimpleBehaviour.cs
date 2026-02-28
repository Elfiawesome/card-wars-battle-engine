using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Behaviour;

// Base class for simpler behaviours that don't need input
public abstract class SimpleBehaviour : IBehaviour
{
	public abstract Type ListeningEventType { get; }
	public virtual int Priority => 0;

	public abstract BehaviourResult Start(IEvent evnt, BehaviourContext context);

	public virtual BehaviourResult Resume(EntityId playerId, IInput input, BehaviourContext context)
		=> throw new InvalidOperationException($"{GetType().Name} does not accept input");
}