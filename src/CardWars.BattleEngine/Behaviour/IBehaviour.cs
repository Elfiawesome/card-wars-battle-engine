using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Behaviour;

public enum BehaviourResult { WaitForInput, Complete }

public interface IBehaviour
{
	public Type ListeningEventType { get; }
	public int Priority { get; }

	BehaviourResult Start(IEvent evnt, BehaviourContext context);
	BehaviourResult Resume(EntityId playedId, IInput input, BehaviourContext context);
}

public abstract class Behaviour<TEvent> : IBehaviour
	where TEvent : IEvent
{
	public Type ListeningEventType => typeof(TEvent);
	public abstract int Priority { get; }

	BehaviourResult IBehaviour.Start(IEvent evnt, BehaviourContext context)
	{
		if (evnt is TEvent tevnt) { return Start(tevnt, context); }
		return BehaviourResult.Complete;
	}

	protected abstract BehaviourResult Start(TEvent evnt, BehaviourContext context);

	public virtual BehaviourResult Resume(EntityId playedId, IInput input, BehaviourContext context)
		=> BehaviourResult.Complete;
}

public abstract class Behaviour<TEvent, TInput> : IBehaviour
	where TEvent : IEvent
	where TInput : IInput
{
	public Type ListeningEventType => typeof(TEvent);
	public abstract int Priority { get; }

	BehaviourResult IBehaviour.Start(IEvent evnt, BehaviourContext context)
	{
		if (evnt is TEvent tevnt) { return Start(tevnt, context); }
		return BehaviourResult.Complete;
	}

	BehaviourResult IBehaviour.Resume(EntityId playedId, IInput input, BehaviourContext context)
	{
		if (input is TInput tinput) { return Resume(playedId, tinput, context); }
		return BehaviourResult.Complete;
	}

	protected abstract BehaviourResult Start(TEvent evnt, BehaviourContext context);
	protected abstract BehaviourResult Resume(EntityId playedId, TInput input, BehaviourContext context);
}