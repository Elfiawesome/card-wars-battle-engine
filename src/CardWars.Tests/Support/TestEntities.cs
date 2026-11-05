using CardWars.BattleEngine;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.Tests;

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
