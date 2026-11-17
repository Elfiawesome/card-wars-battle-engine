using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

/// <summary>
/// Behavioral proof that owner-level <see cref="DataContext"/> isolation is REAL as of Phase 2
/// (doc.md §5, Phase 1 limitations 1 &amp; 2): assigning a non-default context to
/// <see cref="BattleEngine.BattleEngine.Data"/> actually changes how that engine resolves types
/// on its runtime serialize/deserialize path (Transaction -> BehaviourContext ->
/// ScriptContext/ScriptRuntimeContext -> vanilla handlers -> ScriptBehaviour.GetRequestId).
///
/// If the owner call sites were reverted to <see cref="DataContext.Default"/>, both engines
/// would resolve identically and every assertion that the two engines diverge would fail.
/// </summary>
public sealed class OwnerContextIsolationTests
{
	private const string EventId = "cardwars:test/event";

	// Builds the shared behaviour definition that listens for the TestEvent and, when the hook
	// fires, writes a global variable we can observe from GameState.
	private static BehaviourDefinition BuildDefinition() => new(
	[
		new EventHook(
			ResourceId.Vanilla("test/event"),
			[
				new DeclarationInstruction(
					ResourceId.Vanilla("event_value"),
					new MemberExpression(new VariableExpression(ScriptBindings.Event), "value"),
					IsGlobal: true)
			])
	]);

	private static BattleEngine.BattleEngine CreateEngine(DataContext data)
	{
		var engine = new BattleEngine.BattleEngine { Data = data };
		new VanillaMod().OnLoad(engine.Registry, engine.Data, []);
		engine.Registry.InputHandlers.Register(new TestEventInputHandler());
		engine.Registry.EventHandlers.Register(new TestEventHandler());
		return engine;
	}

	private static DataContext ScannedContext()
	{
		var data = new DataContext();
		data.ScanAssembly(typeof(DataTag).Assembly);
		data.ScanAssembly(typeof(BattleEngine.BattleEngine).Assembly);
		data.ScanAssembly(typeof(VanillaMod).Assembly);
		data.ScanAssembly(typeof(TestBootstrap).Assembly);
		return data;
	}

	private static void RunEvent(BattleEngine.BattleEngine engine, CompoundTag definitionTag)
	{
		engine.State.Add(new TestEntity
		{
			Id = EntityId.New(),
			Hp = 10,
			Behaviours = [new ContentPointer(Definition: definitionTag)]
		});
		engine.HandleInput(EntityId.None, new TestEventInput());
	}

	[Fact]
	public void DataDrivenBehaviour_ResolvesEventTypeThroughEachEnginesOwnContext()
	{
		// Context A maps TestEvent to the id the behaviour hook listens for.
		var contextA = ScannedContext();

		// Context B is identical except TestEvent is remapped to a DIFFERENT id, so the hook's
		// event type id will no longer match what B resolves for a TestEvent instance.
		var contextB = ScannedContext();
		contextB.Register(typeof(TestEvent), "cardwars:test/remapped_event");

		var engineA = CreateEngine(contextA);
		var engineB = CreateEngine(contextB);

		// Serialize the definition through each engine's OWN context so the pointer tag is valid
		// in that context (the script instruction/expression ids are registered in both).
		var definitionTagA = contextA.ToTag(BuildDefinition());
		var definitionTagB = contextB.ToTag(BuildDefinition());

		RunEvent(engineA, definitionTagA);
		RunEvent(engineB, definitionTagB);

		var key = ResourceId.Vanilla("event_value");

		// Engine A resolves TestEvent -> "test/event", the hook fires, the global is written.
		Assert.True(
			engineA.State.GlobalScope.Variables.TryGetValue(key, out var valueA),
			"Engine A should have fired the data-driven hook via its own context");
		Assert.Equal(9, Assert.IsType<IntTag>(valueA).Value);

		// Engine B resolves TestEvent -> "test/remapped_event", which does not match the hook,
		// so the behaviour never fires and the global is absent. This is only possible because
		// GetRequestId reads engine B's OWN context rather than DataContext.Default.
		Assert.False(
			engineB.State.GlobalScope.Variables.ContainsKey(key),
			"Engine B must resolve the event type through its own (remapped) context, not the default");
	}

	[Fact]
	public void EngineWithoutTypeRegistration_DoesNotResolveWhatAnotherEngineCan()
	{
		// Engine A knows the script and test types; engine B's context knows nothing.
		var contextA = ScannedContext();
		var contextB = new DataContext();

		var engineA = CreateEngine(contextA);
		var engineB = CreateEngine(contextB);

		var definitionTag = contextA.ToTag(BuildDefinition());

		RunEvent(engineA, definitionTag);
		RunEvent(engineB, definitionTag);

		var key = ResourceId.Vanilla("event_value");

		Assert.True(engineA.State.GlobalScope.Variables.ContainsKey(key));
		// Engine B cannot even deserialize the BehaviourDefinition pointer through its empty
		// context, so no hook fires. Reverting to DataContext.Default would let B resolve it.
		Assert.False(engineB.State.GlobalScope.Variables.ContainsKey(key));
	}
}
