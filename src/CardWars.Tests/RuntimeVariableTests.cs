using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests;

public sealed class RuntimeVariableTests
{
	[Fact]
	public void LocalVariables_CanBeDeclaredReadAndUpdated()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var name = ResourceId.Vanilla("local");

		runtime.DeclareVariable(name, new IntTag(1));
		Assert.True(runtime.TryGetVariable(name, out var value));
		Assert.Equal(1, Assert.IsType<IntTag>(value).Value);

		Assert.True(runtime.TrySetVariable(name, new IntTag(2)));
		Assert.True(runtime.TryGetVariable(name, out value));
		Assert.Equal(2, Assert.IsType<IntTag>(value).Value);
	}

	[Fact]
	public void ConstantVariables_CannotBeUpdated()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var name = ResourceId.Vanilla("constant");

		runtime.DeclareVariable(name, new IntTag(1), isConstant: true);
		Assert.False(runtime.TrySetVariable(name, new IntTag(2)));
		Assert.True(runtime.TryGetVariable(name, out var value));
		Assert.Equal(1, Assert.IsType<IntTag>(value).Value);
	}

	[Fact]
	public void GlobalVariables_AreScopedToTheBattleState()
	{
		var registry = TestSupport.CreateRegistry();
		var stateA = new GameState();
		var stateB = new GameState();
		var name = ResourceId.Vanilla("global");

		var runtimeA = TestSupport.CreateRuntime(registry, stateA, EntityId.None);
		runtimeA.DeclareVariable(name, new IntTag(5), isGlobal: true);

		Assert.True(stateA.GlobalScope.Variables.ContainsKey(name));
		Assert.False(stateB.GlobalScope.Variables.ContainsKey(name));

		var runtimeA2 = TestSupport.CreateRuntime(registry, stateA, EntityId.None);
		Assert.True(runtimeA2.TryGetVariable(name, out var value));
		Assert.Equal(5, Assert.IsType<IntTag>(value).Value);

		var runtimeB = TestSupport.CreateRuntime(registry, stateB, EntityId.None);
		Assert.False(runtimeB.TryGetVariable(name, out _));
	}

	[Fact]
	public void LocalVariables_ShadowGlobalsWithoutChangingThem()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var name = ResourceId.Vanilla("shadowed");

		runtime.DeclareVariable(name, new IntTag(1), isGlobal: true);
		runtime.PushScope();
		runtime.DeclareVariable(name, new IntTag(2));

		Assert.True(runtime.TryGetVariable(name, out var localValue));
		Assert.Equal(2, Assert.IsType<IntTag>(localValue).Value);

		Assert.True(runtime.TrySetVariable(name, new IntTag(3)));
		Assert.True(runtime.TryGetVariable(name, out localValue));
		Assert.Equal(3, Assert.IsType<IntTag>(localValue).Value);
		Assert.Equal(1, Assert.IsType<IntTag>(state.GlobalScope.Variables[name]).Value);
	}

	[Fact]
	public void ConstantLocal_BlocksWritesToSameNamedGlobal()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var name = ResourceId.Vanilla("shadowed");

		runtime.DeclareVariable(name, new IntTag(1), isGlobal: true);
		runtime.PushScope();
		runtime.DeclareVariable(name, new IntTag(2), isConstant: true);

		Assert.False(runtime.TrySetVariable(name, new IntTag(3)));
		Assert.Equal(1, Assert.IsType<IntTag>(state.GlobalScope.Variables[name]).Value);
	}

	[Fact]
	public void RedeclaringAConstant_DoesNotOverwriteIt()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var name = ResourceId.Vanilla("constant");

		runtime.DeclareVariable(name, new IntTag(1), isConstant: true);
		runtime.DeclareVariable(name, new IntTag(2), isConstant: true);

		Assert.True(runtime.TryGetVariable(name, out var value));
		Assert.Equal(1, Assert.IsType<IntTag>(value).Value);
	}

	[Fact]
	public void GlobalConstants_PersistAcrossRuntimes()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var name = ResourceId.Vanilla("global_constant");

		var first = TestSupport.CreateRuntime(registry, state, EntityId.None);
		first.DeclareVariable(name, new IntTag(1), isConstant: true, isGlobal: true);

		var second = TestSupport.CreateRuntime(registry, state, EntityId.None);
		Assert.False(second.TrySetVariable(name, new IntTag(2)));
		Assert.True(second.TryGetVariable(name, out var value));
		Assert.Equal(1, Assert.IsType<IntTag>(value).Value);
	}

	[Fact]
	public void Clear_RemovesBattleGlobals()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var name = ResourceId.Vanilla("global");

		runtime.DeclareVariable(name, new IntTag(1), isConstant: true, isGlobal: true);
		state.Clear();

		Assert.Empty(state.GlobalScope.Variables);
		Assert.Empty(state.GlobalScope.Constants);
	}

	[Fact]
	public void UnknownVariables_ReturnFalseAndNull()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var runtime = TestSupport.CreateRuntime(registry, state, EntityId.None);

		Assert.False(runtime.TryGetVariable(ResourceId.Vanilla("missing"), out var value));
		Assert.Null(value);
		Assert.False(runtime.TrySetVariable(ResourceId.Vanilla("missing"), new IntTag(1)));
	}

	[Fact]
	public void LoopLimit_IsPerRuntime()
	{
		var registry = TestSupport.CreateRegistry();
		var state = new GameState();
		var first = TestSupport.CreateRuntime(registry, state, EntityId.None);
		var second = TestSupport.CreateRuntime(registry, state, EntityId.None);

		first.MaxLoopIterations = 3;

		Assert.Equal(3, first.MaxLoopIterations);
		Assert.Equal(10_000, second.MaxLoopIterations);
	}
}
