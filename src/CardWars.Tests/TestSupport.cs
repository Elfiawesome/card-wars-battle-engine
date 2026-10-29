using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven.Functions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.Tests;

internal static class TestSupport
{
	public static BattleEngineRegistry CreateRegistry()
	{
		var registry = new BattleEngineRegistry();
		new VanillaMod().OnLoad(registry, []);
		return registry;
	}

	public static BehaviourContext CreateBehaviourContext(BattleEngineRegistry registry, GameState state, EntityId owner)
	{
		var transaction = new Transaction { Registry = registry, State = state };
		return new BehaviourContext(transaction, state, owner);
	}

	public static ScriptRuntimeContext CreateRuntime(BattleEngineRegistry registry, GameState state, EntityId owner)
	{
		var context = CreateBehaviourContext(registry, state, owner);
		return new ScriptRuntimeContext(context, registry.DataDriven);
	}

	public static TestEntity AddEntity(GameState state, int hp = 10)
	{
		var entity = new TestEntity { Id = EntityId.New(), Hp = hp };
		state.Add(entity);
		return entity;
	}
}

[DataTagType("test/entity")]
public sealed class TestEntity : IEntity
{
	[DataTag] public EntityId Id { get; init; }
	[DataTag] public int Hp { get; set; }
	[DataTag] public CompoundTag Data { get; set; } = new();
	public List<BehaviourPointer> Behaviours { get; set; } = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => Behaviours;
}

[DataTagType("test/event")]
public sealed class TestEvent : IEvent
{
	[DataTag] public int Value { get; set; }
}

[DataTagType("test/input")]
public sealed class TestInput : IInput
{
	[DataTag] public int Value { get; set; }
}

[DataTagType("test/other_input")]
public sealed class OtherInput : IInput
{
}

[DataTagType("test/event_input")]
public sealed class TestEventInput : IInput
{
}

public sealed class TestEventInputHandler : IInputHandler<TestEventInput>
{
	public void Handle(InputContext context, TestEventInput request)
		=> context.Transaction.QueueEvent(new TestEvent { Value = 9 });
}

[DataTagType("test/block")]
public sealed record class TestBlock([property: DataTag] int Value) : IBlock;

public sealed class TestBlockHandler : IBlockHandler<TestBlock>
{
	public List<TestBlock> Blocks { get; } = [];

	public void Handle(GameState context, TestBlock request) => Blocks.Add(request);
}

public sealed class TestEventHandler : IEventHandler<TestEvent>
{
	public int Count { get; private set; }

	public void Handle(Transaction context, TestEvent request) => Count++;
}

public sealed class OtherInputHandler : IInputHandler<OtherInput>
{
	public void Handle(InputContext context, OtherInput request)
	{
	}
}

public sealed class CountingFunction : IDataDrivenFunction
{
	public int Calls { get; private set; }

	public DataTag Invoke(ScriptContext context, IReadOnlyList<DataTag> arguments)
	{
		Calls++;
		return new IntTag(1);
	}
}
