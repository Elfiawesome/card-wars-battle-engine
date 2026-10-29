using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests;

public sealed class InstructionHandlerTests
{
	[Fact]
	public void DeclarationAndSet_UpdateVariables()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var outcome = runtime.Executor.Execute(
		[
			new DeclarationInstruction(ResourceId.Vanilla("x"), new LiteralExpression(new IntTag(1))),
			new SetInstruction(ResourceId.Vanilla("x"), new LiteralExpression(new IntTag(2)))
		]);

		Assert.Equal(InstructionOutcome.Continue, outcome);
		Assert.True(runtime.TryGetVariable(ResourceId.Vanilla("x"), out var value));
		Assert.Equal(2, Assert.IsType<IntTag>(value).Value);
	}

	[Fact]
	public void IfInstruction_ExecutesOnlyTheSelectedBranch()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var x = ResourceId.Vanilla("x");

		runtime.DeclareVariable(x, new IntTag(0));

		runtime.Executor.Execute(
		[
			new IfInstruction(
				new LiteralExpression(new BoolTag(true)),
				[new SetInstruction(x, new LiteralExpression(new IntTag(1)))],
				[new SetInstruction(x, new LiteralExpression(new IntTag(2)))])
		]);
		Assert.True(runtime.TryGetVariable(x, out var value));
		Assert.Equal(1, Assert.IsType<IntTag>(value).Value);

		runtime.Executor.Execute(
		[
			new IfInstruction(
				new LiteralExpression(new BoolTag(false)),
				[new SetInstruction(x, new LiteralExpression(new IntTag(3)))],
				[new SetInstruction(x, new LiteralExpression(new IntTag(4)))])
		]);
		Assert.True(runtime.TryGetVariable(x, out value));
		Assert.Equal(4, Assert.IsType<IntTag>(value).Value);
	}

	[Fact]
	public void WhileInstruction_RepeatsUntilConditionIsFalse()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var flag = ResourceId.Vanilla("flag");
		var count = ResourceId.Vanilla("count");

		runtime.DeclareVariable(flag, new BoolTag(true));
		runtime.DeclareVariable(count, new IntTag(0));

		var outcome = runtime.Executor.Execute(
		[
			new WhileInstruction(
				new VariableExpression(flag),
				[
					new SetInstruction(count, new LiteralExpression(new IntTag(1))),
					new SetInstruction(flag, new LiteralExpression(new BoolTag(false)))
				])
		]);

		Assert.Equal(InstructionOutcome.Continue, outcome);
		Assert.True(runtime.TryGetVariable(count, out var countValue));
		Assert.Equal(1, Assert.IsType<IntTag>(countValue).Value);
	}

	[Fact]
	public void ForInstruction_RunsInitBodyAndStep()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var flag = ResourceId.Vanilla("flag");
		var steps = ResourceId.Vanilla("steps");

		runtime.DeclareVariable(flag, new BoolTag(true));
		runtime.DeclareVariable(steps, new IntTag(0));

		var outcome = runtime.Executor.Execute(
		[
			new ForInstruction(
				new SetInstruction(steps, new LiteralExpression(new IntTag(1))),
				new VariableExpression(flag),
				new SetInstruction(steps, new LiteralExpression(new IntTag(2))),
				[new SetInstruction(flag, new LiteralExpression(new BoolTag(false)))])
		]);

		Assert.Equal(InstructionOutcome.Continue, outcome);
		Assert.True(runtime.TryGetVariable(steps, out var stepsValue));
		Assert.Equal(2, Assert.IsType<IntTag>(stepsValue).Value);
	}

	[Fact]
	public void ForEachInstruction_IteratesAllItems()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var item = ResourceId.Vanilla("item");
		var last = ResourceId.Vanilla("last");

		runtime.DeclareVariable(last, new IntTag(0));

		var outcome = runtime.Executor.Execute(
		[
			new ForEachInstruction(
				item,
				new ListExpression(
				[
					new LiteralExpression(new IntTag(1)),
					new LiteralExpression(new IntTag(2)),
					new LiteralExpression(new IntTag(3))
				]),
				[new SetInstruction(last, new VariableExpression(item))])
		]);

		Assert.Equal(InstructionOutcome.Continue, outcome);
		Assert.True(runtime.TryGetVariable(last, out var lastValue));
		Assert.Equal(3, Assert.IsType<IntTag>(lastValue).Value);
	}

	[Fact]
	public void BreakAndContinue_ControlLoopExecution()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var flag = ResourceId.Vanilla("flag");
		var last = ResourceId.Vanilla("last");
		var item = ResourceId.Vanilla("item");

		runtime.DeclareVariable(flag, new BoolTag(true));
		runtime.DeclareVariable(last, new IntTag(0));

		var breakOutcome = runtime.Executor.Execute(
		[
			new WhileInstruction(new VariableExpression(flag), [new BreakInstruction()])
		]);
		Assert.Equal(InstructionOutcome.Continue, breakOutcome);

		var continueOutcome = runtime.Executor.Execute(
		[
			new ForEachInstruction(
				item,
				new ListExpression([new LiteralExpression(new IntTag(1))]),
				[
					new ContinueInstruction(),
					new SetInstruction(last, new VariableExpression(item))
				])
		]);
		Assert.Equal(InstructionOutcome.Continue, continueOutcome);
		Assert.True(runtime.TryGetVariable(last, out var lastValue));
		Assert.Equal(0, Assert.IsType<IntTag>(lastValue).Value);
	}

	[Fact]
	public void ReturnInstruction_StopsExecution()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var outcome = runtime.Executor.Execute(
		[
			new ReturnInstruction(new LiteralExpression(new IntTag(1))),
			new DeclarationInstruction(ResourceId.Vanilla("x"), new LiteralExpression(new IntTag(2)))
		]);

		Assert.Equal(InstructionOutcome.Return, outcome);
		Assert.False(runtime.TryGetVariable(ResourceId.Vanilla("x"), out _));
	}

	[Fact]
	public void WhileInstruction_StopsAtTheRuntimeLoopLimit()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		runtime.MaxLoopIterations = 2;
		var flag = ResourceId.Vanilla("flag");
		runtime.DeclareVariable(flag, new BoolTag(true));

		var outcome = runtime.Executor.Execute(
		[
			new WhileInstruction(new VariableExpression(flag), [new ContinueInstruction()])
		]);

		Assert.Equal(InstructionOutcome.Continue, outcome);
	}

	[Fact]
	public void RaiseInstruction_QueuesAnEvent()
	{
		var registry = TestSupport.CreateRegistry();
		var eventHandler = new TestEventHandler();
		registry.EventHandlers.Register(eventHandler);
		registry.InputHandlers.Register(new OtherInputHandler());
		var state = new GameState();
		var transaction = new Transaction { Registry = registry, State = state };
		var behaviourContext = new BehaviourContext(transaction, state, EntityId.None);
		var runtime = new ScriptRuntimeContext(behaviourContext, registry.DataDriven);

		runtime.Executor.Execute(
		[
			new RaiseInstruction(new ConstructExpression(
				ResourceId.Vanilla("test/event"),
				new Dictionary<string, IExpression> { ["value"] = new LiteralExpression(new IntTag(5)) }))
		]);

		transaction.ProcessInput(EntityId.None, new OtherInput());

		Assert.Equal(1, eventHandler.Count);
	}

	[Fact]
	public void StageBlockAndCommit_AppliesTheBlock()
	{
		var registry = TestSupport.CreateRegistry();
		var blockHandler = new TestBlockHandler();
		registry.BlockHandlers.Register(blockHandler);
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var outcome = runtime.Executor.Execute(
		[
			new StageBlockInstruction(ConstructBlock(5)),
			new CommitInstruction()
		]);

		Assert.Equal(InstructionOutcome.Continue, outcome);
		var block = Assert.Single(blockHandler.Blocks);
		Assert.Equal(5, block.Value);
	}

	[Fact]
	public void StageBlocksAndWait_CommitStagedBlocks()
	{
		var registry = TestSupport.CreateRegistry();
		var blockHandler = new TestBlockHandler();
		registry.BlockHandlers.Register(blockHandler);
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		var outcome = runtime.Executor.Execute(
		[
			new StageBlocksInstruction(
				new ListExpression([ConstructBlock(1), ConstructBlock(2)])),
			new WaitInstruction()
		]);

		Assert.Equal(InstructionOutcome.WaitForInput, outcome);
		Assert.Equal(2, blockHandler.Blocks.Count);
		Assert.Equal(1, blockHandler.Blocks[0].Value);
		Assert.Equal(2, blockHandler.Blocks[1].Value);
	}

	private static ConstructExpression ConstructBlock(int value)
		=> new(
			ResourceId.Vanilla("test/block"),
			new Dictionary<string, IExpression>
			{
				["value"] = new LiteralExpression(new IntTag(value))
			});
}
