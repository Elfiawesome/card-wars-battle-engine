using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Behaviour;

public enum BehaviourResult { WaitForInput, Complete }

public interface IBehaviour
{
	// ResourceId Id { get; }
	public Type ListeningEventType { get; }
	public int Priority { get; }

	BehaviourResult Start(IEvent evnt, BehaviourContext context);
	BehaviourResult Resume(EntityId playedId, IInput input, BehaviourContext context);
}