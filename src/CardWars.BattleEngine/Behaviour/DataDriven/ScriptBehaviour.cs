using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public sealed class ScriptBehaviour(BehaviourDefinition definition, DataDrivenRegistry dataDriven) : IBehaviour
{
	private ScriptRuntimeContext? _waitingRuntime;
	private ScriptScope? _inputScope;
	private IEnumerator<InstructionOutcome>? _eventExecution;
	private IEnumerator<InstructionOutcome>? _inputExecution;

	public Type ListeningEventType => typeof(IEvent);
	public int Priority => definition.Priority;

	public BehaviourResult Start(IEvent evnt, BehaviourContext context)
	{
		ResetExecution();

		var eventId = GetRequestId(evnt.GetType());
		if (eventId is null) return BehaviourResult.Complete;

		var hooks = definition.OnEvent?
			.Where(hook => hook.EventType == eventId.Value)
			.ToList() ?? [];

		if (hooks.Count == 0) return BehaviourResult.Complete;

		var runtime = new ScriptRuntimeContext(context, dataDriven);
		runtime.PushScope();
		SeedEventBindings(runtime, evnt, context);
		_eventExecution = ExecuteHooks(runtime, hooks.Select(hook => hook.Instructions)).GetEnumerator();

		var outcome = Advance(_eventExecution);
		if (outcome == InstructionOutcome.WaitForInput)
		{
			_waitingRuntime = runtime;
			return BehaviourResult.WaitForInput;
		}

		ResetExecution();
		return BehaviourResult.Complete;
	}

	public BehaviourResult Resume(EntityId playerId, IInput input, BehaviourContext context)
	{
		if (_waitingRuntime is not { } runtime) return BehaviourResult.Complete;

		if (_inputExecution is { } inputExecution)
		{
			SeedInputBindings(runtime, _inputScope, playerId, input);
			var pausedOutcome = Advance(inputExecution);
			if (pausedOutcome == InstructionOutcome.WaitForInput) return BehaviourResult.WaitForInput;

			inputExecution.Dispose();
			_inputExecution = null;
			_inputScope = null;

			if (pausedOutcome is not null)
			{
				ResetExecution();
				return BehaviourResult.Complete;
			}

			return ContinueEvent();
		}

		var inputId = GetRequestId(input.GetType());
		if (inputId is null) return BehaviourResult.WaitForInput;

		var hooks = definition.OnInput?
			.Where(hook => hook.InputType == inputId.Value)
			.ToList() ?? [];

		if (hooks.Count == 0) return BehaviourResult.WaitForInput;

		_inputScope = runtime.PushScope();
		SeedInputBindings(runtime, _inputScope, playerId, input);
		var execution = ExecuteHooks(runtime, hooks.Select(hook => hook.Instructions)).GetEnumerator();
		_inputExecution = execution;

		var outcome = Advance(execution);
		if (outcome == InstructionOutcome.WaitForInput) return BehaviourResult.WaitForInput;

		execution.Dispose();
		_inputExecution = null;
		_inputScope = null;

		if (outcome is not null)
		{
			ResetExecution();
			return BehaviourResult.Complete;
		}

		return ContinueEvent();
	}

	private BehaviourResult ContinueEvent()
	{
		if (_eventExecution is null)
		{
			ResetExecution();
			return BehaviourResult.Complete;
		}

		var outcome = Advance(_eventExecution);
		if (outcome == InstructionOutcome.WaitForInput) return BehaviourResult.WaitForInput;

		ResetExecution();
		return BehaviourResult.Complete;
	}

	private static InstructionOutcome? Advance(IEnumerator<InstructionOutcome> execution)
	{
		while (execution.MoveNext())
		{
			var outcome = execution.Current;
			if (outcome == InstructionOutcome.Continue) continue;
			return outcome;
		}

		return null;
	}

	private static IEnumerable<InstructionOutcome> ExecuteHooks(
		ScriptRuntimeContext runtime,
		IEnumerable<IReadOnlyList<IInstruction>?> hooks)
	{
		foreach (var instructions in hooks)
		{
			foreach (var outcome in runtime.Executor.Execute(instructions))
				yield return outcome;
		}
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

	private static void SeedInputBindings(ScriptRuntimeContext runtime, ScriptScope? scope, EntityId playerId, IInput input)
	{
		if (scope is null) return;

		var inputTag = new ObjectRefTag(input);
		runtime.DeclareVariable(scope, ScriptBindings.Input, inputTag);
		runtime.DeclareVariable(scope, ScriptBindings.InputArgs, inputTag);
		runtime.DeclareVariable(scope, ScriptBindings.InputPlayer, playerId.IsNone ? new NullTag() : new GuidTag(playerId.Value));
	}

	private void ResetExecution()
	{
		_inputExecution?.Dispose();
		_eventExecution?.Dispose();
		_inputExecution = null;
		_eventExecution = null;
		_inputScope = null;
		_waitingRuntime = null;
	}

	private static ResourceId? GetRequestId(Type type)
	{
		var typeId = DataTagTypeRegistry.GetTypeId(type);
		if (typeId is null)
		{
			Log.Warn($"Type [{type.Name}] is not registered as a data tag type");
			return null;
		}

		return ResourceId.Parse(typeId);
	}
}
