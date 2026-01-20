using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Behaviour;

public enum BehaviourResult { Continue, WaitForInput, Complete }

public interface IBehaviour
{
	// ResourceId Id { get; }
	public Type ListeningEventType { get; }
	public int Priority { get; }

	BehaviourResult Start(IEvent evnt, BehaviourContext context);
	BehaviourResult Resume(IInput input, BehaviourContext context);
}