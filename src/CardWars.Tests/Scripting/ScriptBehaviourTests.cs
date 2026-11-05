using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Scripting;

public sealed class ScriptBehaviourTests
{
	[Fact]
	public void Start_BindsEventAndWritesGlobal()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var owner = TestSupport.AddEntity(state);
		var context = TestSupport.CreateBehaviourContext(registry, state, owner.Id);

		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("test/event"),
					[
						new DeclarationInstruction(
							ResourceId.Vanilla("event_value"),
							new MemberExpression(new VariableExpression(ScriptBindings.Event), "value"),
							IsGlobal: true),
						new DeclarationInstruction(
							ResourceId.Vanilla("self_hp"),
							new MemberExpression(new VariableExpression(ScriptBindings.Self), "hp"),
							IsGlobal: true),
						new DeclarationInstruction(
							ResourceId.Vanilla("turn_number"),
							new MemberExpression(
								new MemberExpression(new VariableExpression(ScriptBindings.State), "turn"),
								"turn_number"),
							IsGlobal: true),
						new DeclarationInstruction(
							ResourceId.Vanilla("copy_hp"),
							new MemberExpression(new VariableExpression(ScriptBindings.Vars), "self_hp"),
							IsGlobal: true)
					])
			]);

		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);
		var result = behaviour.Start(new TestEvent { Value = 9 }, context);

		Assert.Equal(BehaviourResult.Complete, result);
		Assert.Equal(9, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("event_value")]).Value);
		Assert.Equal(10, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("self_hp")]).Value);
		Assert.Equal(0, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("turn_number")]).Value);
		Assert.Equal(10, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("copy_hp")]).Value);
	}

	[Fact]
	public void WaitAndResume_RunsInputHookAndBindsInput()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var owner = TestSupport.AddEntity(state);
		var context = TestSupport.CreateBehaviourContext(registry, state, owner.Id);

		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("test/event"),
					[
						new DeclarationInstruction(
							ResourceId.Vanilla("event_value"),
							new MemberExpression(new VariableExpression(ScriptBindings.Event), "value"),
							IsGlobal: true),
						new WaitInstruction()
					])
			],
			[
				new InputHook(
					ResourceId.Vanilla("test/input"),
					[
						new DeclarationInstruction(
							ResourceId.Vanilla("input_value"),
							new MemberExpression(new VariableExpression(ScriptBindings.Input), "value"),
							IsGlobal: true)
					])
			]);

		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);
		var start = behaviour.Start(new TestEvent { Value = 9 }, context);
		Assert.Equal(BehaviourResult.WaitForInput, start);
		Assert.Equal(9, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("event_value")]).Value);

		var unmatched = behaviour.Resume(EntityId.None, new OtherInput(), context);
		Assert.Equal(BehaviourResult.WaitForInput, unmatched);
		Assert.False(state.GlobalScope.Variables.ContainsKey(ResourceId.Vanilla("input_value")));

		var resumed = behaviour.Resume(EntityId.None, new TestInput { Value = 4 }, context);
		Assert.Equal(BehaviourResult.Complete, resumed);
		Assert.Equal(4, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("input_value")]).Value);
	}

	[Fact]
	public void Wait_ResumesInsideInputHook()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var owner = TestSupport.AddEntity(state);
		var context = TestSupport.CreateBehaviourContext(registry, state, owner.Id);

		var definition = new BehaviourDefinition(
			[
				new EventHook(ResourceId.Vanilla("test/event"), [new WaitInstruction()])
			],
			[
				new InputHook(
					ResourceId.Vanilla("test/input"),
					[
						new WaitInstruction(),
						new DeclarationInstruction(
							ResourceId.Vanilla("input_resumed"),
							new LiteralExpression(new IntTag(1)),
							IsGlobal: true)
					])
			]);

		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);

		Assert.Equal(BehaviourResult.WaitForInput, behaviour.Start(new TestEvent { Value = 9 }, context));
		Assert.Equal(BehaviourResult.WaitForInput, behaviour.Resume(EntityId.None, new TestInput { Value = 4 }, context));
		Assert.False(state.GlobalScope.Variables.ContainsKey(ResourceId.Vanilla("input_resumed")));

		Assert.Equal(BehaviourResult.Complete, behaviour.Resume(EntityId.None, new TestInput { Value = 5 }, context));
		Assert.Equal(1, Assert.IsType<IntTag>(state.GlobalScope.Variables[ResourceId.Vanilla("input_resumed")]).Value);
	}

	[Fact]
	public void Wait_ResumesInsideNestedLoop()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var owner = TestSupport.AddEntity(state);
		var context = TestSupport.CreateBehaviourContext(registry, state, owner.Id);
		var flag = ResourceId.Vanilla("flag");
		var continued = ResourceId.Vanilla("continued");

		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("test/event"),
					[
						new DeclarationInstruction(flag, new LiteralExpression(new BoolTag(true))),
						new DeclarationInstruction(continued, new LiteralExpression(new IntTag(0)), IsGlobal: true),
						new WhileInstruction(
							new VariableExpression(flag),
							[
								new WaitInstruction(),
								new SetInstruction(continued, new LiteralExpression(new IntTag(1))),
								new SetInstruction(flag, new LiteralExpression(new BoolTag(false)))
							])
					])
			],
			[
				new InputHook(ResourceId.Vanilla("test/input"), [])
			]);

		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);

		var start = behaviour.Start(new TestEvent { Value = 9 }, context);
		Assert.Equal(BehaviourResult.WaitForInput, start);
		Assert.Equal(0, Assert.IsType<IntTag>(state.GlobalScope.Variables[continued]).Value);

		var resumed = behaviour.Resume(EntityId.None, new TestInput { Value = 4 }, context);
		Assert.Equal(BehaviourResult.Complete, resumed);
		Assert.Equal(1, Assert.IsType<IntTag>(state.GlobalScope.Variables[continued]).Value);
	}

	[Fact]
	public void Start_IgnoresUnmatchedEvent()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var owner = TestSupport.AddEntity(state);
		var context = TestSupport.CreateBehaviourContext(registry, state, owner.Id);

		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("other/event"),
					[new DeclarationInstruction(ResourceId.Vanilla("x"), new LiteralExpression(new IntTag(1)))])
			]);

		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);
		var result = behaviour.Start(new TestEvent { Value = 9 }, context);

		Assert.Equal(BehaviourResult.Complete, result);
		Assert.False(state.GlobalScope.Variables.ContainsKey(ResourceId.Vanilla("x")));
	}

	[Fact]
	public void Wait_CommitsStagedBlocks()
	{
		var registry = TestSupport.CreateRegistry();
		var blockHandler = new TestBlockHandler();
		registry.BlockHandlers.Register(blockHandler);
		var state = new GameState();
		var owner = TestSupport.AddEntity(state);
		var context = TestSupport.CreateBehaviourContext(registry, state, owner.Id);

		var definition = new BehaviourDefinition(
			[
				new EventHook(
					ResourceId.Vanilla("test/event"),
					[
						new StageBlockInstruction(new ConstructExpression(
							ResourceId.Vanilla("test/block"),
							new Dictionary<string, IExpression>
							{
								["value"] = new LiteralExpression(new IntTag(3))
							})),
						new WaitInstruction()
					])
			]);

		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);
		var result = behaviour.Start(new TestEvent { Value = 9 }, context);

		Assert.Equal(BehaviourResult.WaitForInput, result);
		var block = Assert.Single(blockHandler.Blocks);
		Assert.Equal(3, block.Value);
	}

	[Fact]
	public void PriorityAndListeningEventType_AreExposed()
	{
		var registry = TestSupport.CreateRegistry();
		var definition = new BehaviourDefinition { Priority = 4 };
		var behaviour = new ScriptBehaviour(definition, registry.DataDriven);

		Assert.Equal(4, behaviour.Priority);
		Assert.Equal(typeof(BattleEngine.Event.IEvent), behaviour.ListeningEventType);
	}
}
