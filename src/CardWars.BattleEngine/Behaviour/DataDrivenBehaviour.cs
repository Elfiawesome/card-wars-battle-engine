using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expression;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Global;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour;

// Needs a new naming...
// This is for any data-driven behaviour
public class DataDrivenBehaviour : IBehaviour
{
	public Type ListeningEventType { get; } = typeof(IEvent);
	public int Priority { get; set; } = 0;
	public BehaviourInstruction BehaviourInstruction { get; set; }


	public BehaviourResult Resume(EntityId playerId, IInput input, BehaviourContext context)
	{
		return BehaviourResult.Complete;
	}
	public BehaviourResult Start(IEvent evnt, BehaviourContext context)
	{
		var _runtime = new ScriptRuntimeContext();
		// Idk how to do this yet lol
		// runtime.Variables[ResourceId.Vanilla("event")] = evnt??
		// runtime.Variables[ResourceId.Vanilla("state")] = context.State??
		// runtime.Variables[ResourceId.Vanilla("owner")] = context.Owner??

		var typeId = DataTagTypeRegistry.GetTypeId(evnt.GetType());
		if (typeId == null) return BehaviourResult.Complete;

		var eventId = ResourceId.Parse(typeId);
		var eventInstructions = BehaviourInstruction.OnEvent.Where(i => i.EventType == eventId);
		if (eventInstructions == null) return BehaviourResult.Complete;

		foreach (var instruction in eventInstructions)
		{
			foreach (var instructionLine in instruction.Instructions)
			{
				// To be handled via handlers
				
				// Behaviour specific
				switch (instructionLine)
				{
					case RaiseEvent raiseEvent:
						break;
					case StageBlock stageBlock:
						break;
					case CommitBlock commitBlock:
						break;
				}
			}
		}

		return BehaviourResult.Complete;
	}
}