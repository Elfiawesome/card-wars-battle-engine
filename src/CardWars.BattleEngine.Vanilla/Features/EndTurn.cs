using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;

namespace CardWars.BattleEngine.Vanilla.Features;

public class EndPhaseEvent() : IEvent
{
	public required TurnState TurnState;
	public bool PhaseChanged = false;
}

public class EndTurnEvent() : IEvent
{
	public required TurnState TurnState;
	public bool PhaseChanged = false;
}

public class EndPhaseEventHandler : IEventHandler<EndPhaseEvent>
{
	public void Handle(Transaction context, EndPhaseEvent request)
	{
		Console.WriteLine("End PHASE");
	}
}

public class EndTurnEventHandler : IEventHandler<EndTurnEvent>
{
	public void Handle(Transaction context, EndTurnEvent request)
	{
		var batch = new BlockBatch([]);
		Console.WriteLine("End TURN");
		batch.Blocks.Add(new UpdateTurnStateBlock(request.TurnState));
		context.ApplyBlockBatch(batch);
	}
}