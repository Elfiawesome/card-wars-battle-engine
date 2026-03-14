using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class EndPhaseEvent() : IEvent
{
	[DataTag] public required TurnState TurnState { get; set; }
	[DataTag] public bool PhaseChanged { get; set; } = false;
}

[DataTagType()]
public class EndTurnEvent() : IEvent
{
	[DataTag] public required TurnState TurnState { get; set; }
	[DataTag] public bool PhaseChanged { get; set; } = false;
}

public class EndPhaseEventHandler : IEventHandler<EndPhaseEvent>
{
	public void Handle(Transaction context, EndPhaseEvent request)
	{
	}
}

public class EndTurnEventHandler : IEventHandler<EndTurnEvent>
{
	public void Handle(Transaction context, EndTurnEvent request)
	{
		var batch = new BlockBatch([]);
		batch.Blocks.Add(new UpdateTurnStateBlock(request.TurnState));
		context.ApplyBlockBatch(batch);
	}
}