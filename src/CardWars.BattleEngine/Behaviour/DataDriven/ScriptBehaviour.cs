using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public sealed class ScriptBehaviour(BehaviourDefinition definition, DataDrivenRegistry dataDriven) : IBehaviour
{
	private ScriptRuntimeContext? _waitingRuntime;

	public Type ListeningEventType => typeof(IEvent);
	public int Priority => definition.Priority;

	public BehaviourResult Start(IEvent evnt, BehaviourContext context)
	{
		var eventId = GetRequestId(evnt.GetType());
		if (eventId == null) return BehaviourResult.Complete;

		var hooks = definition.OnEvent?
			.Where(hook => hook.EventType == eventId.Value)
			.ToList() ?? [];

		if (hooks.Count == 0) return BehaviourResult.Complete;

		var runtime = new ScriptRuntimeContext(context, dataDriven);
		runtime.PushScope();
		SeedEventBindings(runtime, evnt, context);

		foreach (var hook in hooks)
		{
			var outcome = runtime.Executor.Execute(hook.Instructions);
			if (outcome == InstructionOutcome.WaitForInput)
			{
				_waitingRuntime = runtime;
				return BehaviourResult.WaitForInput;
			}

			if (outcome == InstructionOutcome.Return) break;
		}

		return BehaviourResult.Complete;
	}

	public BehaviourResult Resume(EntityId playerId, IInput input, BehaviourContext context)
	{
		if (_waitingRuntime == null) return BehaviourResult.Complete;

		var inputId = GetRequestId(input.GetType());
		if (inputId == null) return BehaviourResult.WaitForInput;

		var hooks = definition.OnInput?
			.Where(hook => hook.InputType == inputId.Value)
			.ToList() ?? [];

		if (hooks.Count == 0) return BehaviourResult.WaitForInput;

		_waitingRuntime.PushScope();
		SeedInputBindings(_waitingRuntime, playerId, input);

		foreach (var hook in hooks)
		{
			var outcome = _waitingRuntime.Executor.Execute(hook.Instructions);
			if (outcome == InstructionOutcome.WaitForInput) return BehaviourResult.WaitForInput;
			if (outcome == InstructionOutcome.Return) break;
		}

		_waitingRuntime = null;
		return BehaviourResult.Complete;
	}

	private static void SeedEventBindings(ScriptRuntimeContext runtime, IEvent evnt, BehaviourContext context)
	{
		var eventTag = new ObjectRefTag(evnt);
		runtime.DeclareVariable(ScriptBindings.Event, eventTag);
		runtime.DeclareVariable(ScriptBindings.EventArgs, eventTag);

		DataTag ownerTag = context.Owner is { } owner ? new ObjectRefTag(owner) : new NullTag();
		runtime.DeclareVariable(ScriptBindings.Owner, ownerTag);
		runtime.DeclareVariable(ScriptBindings.Self, ownerTag);
		runtime.DeclareVariable(ScriptBindings.State, new ObjectRefTag(context.State));
		runtime.DeclareVariable(ScriptBindings.Vars, new ObjectRefTag(runtime));
	}

	private static void SeedInputBindings(ScriptRuntimeContext runtime, EntityId playerId, IInput input)
	{
		var inputTag = new ObjectRefTag(input);
		runtime.DeclareVariable(ScriptBindings.Input, inputTag);
		runtime.DeclareVariable(ScriptBindings.InputArgs, inputTag);
		runtime.DeclareVariable(ScriptBindings.InputPlayer, playerId.IsNone ? new NullTag() : new GuidTag(playerId.Value));
	}

	private static ResourceId? GetRequestId(Type type)
	{
		var typeId = DataTagTypeRegistry.GetTypeId(type);
		if (typeId == null)
		{
			Log.Warn($"Type [{type.Name}] is not registered as a data tag type");
			return null;
		}

		return ResourceId.Parse(typeId);
	}
}
