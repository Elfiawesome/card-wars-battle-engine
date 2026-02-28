using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Features;

public record struct EndTurnRequestInput(
) : IInput;

public class EndTurnRequestInputHandler : IInputHandler<EndTurnRequestInput>
{
	public void Handle(InputContext context, EndTurnRequestInput request)
	{
		context.Transaction.QueueEvent(new EndTurnRequestEvent());
	}
}

public class EndTurnRequestEvent() : IEvent
{
	public bool IsCancelled = false;
}

public class EndTurnRequestEventHandler : IEventHandler<EndTurnRequestEvent>
{
	public void Handle(Transaction context, EndTurnRequestEvent request)
	{
		if (request.IsCancelled) { return; }
		var newTurnState = context.State.Turn.Copy();
		newTurnState.TurnIndex += 1;

		bool isPhasedChanged = false;
		if (newTurnState.TurnIndex >= newTurnState.TurnOrder.Count)
		{
			newTurnState.TurnIndex = 0;
			newTurnState.Phase = (newTurnState.Phase == TurnPhase.Setup)
							? TurnPhase.Attacking
							: TurnPhase.Setup;
			isPhasedChanged = true;

			context.QueueEvent(new EndPhaseEvent() { TurnState = newTurnState, PhaseChanged = isPhasedChanged });
		}

		EntityId? currentPlayerId = newTurnState.TurnOrder.ElementAtOrDefault(newTurnState.TurnIndex);
		if (currentPlayerId.Value.IsNone)
		{
			// Something wrong probably happened, so we advance to the next turn
			context.QueueEvent(new EndTurnRequestEvent());
		}
		else
		{
			newTurnState.AllowedPlayerInputs.Clear();
			newTurnState.AllowedPlayerInputs.Add((EntityId)currentPlayerId);
			context.QueueEvent(new EndTurnEvent() { TurnState = newTurnState, PhaseChanged = isPhasedChanged });
		}
	}
}

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