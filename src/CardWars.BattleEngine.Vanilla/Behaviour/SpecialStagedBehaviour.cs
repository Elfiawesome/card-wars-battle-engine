using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Vanilla.Event;
using CardWars.BattleEngine.Vanilla.Input;

namespace CardWars.BattleEngine.Vanilla.Behaviour;

public class SpecialStagedBehaviour : IBehaviour
{
	public Type ListeningEventType => typeof(TestEvent);
	public int Priority => 0;

	public BehaviourResult Resume(IInput input, BehaviourContext context)
	{
		if (input is TestAdditionalInput testAdditionalInput)
		{
			Console.WriteLine("Stage 2 of Behaviour was ran");
			return BehaviourResult.Complete;
		}
		Console.WriteLine("Behaviour received a wrong input?");
		return BehaviourResult.WaitForInput;
	}

	public BehaviourResult Start(IEvent evnt, BehaviourContext context)
	{
		Console.WriteLine("Stage 1 of Behaviour was ran");
		return BehaviourResult.WaitForInput;
	}
}