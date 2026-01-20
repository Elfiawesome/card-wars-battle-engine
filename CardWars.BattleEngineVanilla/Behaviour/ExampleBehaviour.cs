using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngineVanilla.Event;

namespace CardWars.BattleEngineVanilla.Behaviour;

public class ExampleBehaviour : SimpleBehaviour
{
	public override Type ListeningEventType => typeof(TestEvent);
	public override int Priority => 0;


	public override BehaviourResult Start(IEvent evnt, BehaviourContext context)
	{
		Console.WriteLine("Simple Behaviour Ran!");
		context.RaiseEvent(evnt);
		return BehaviourResult.Complete;
	}
}