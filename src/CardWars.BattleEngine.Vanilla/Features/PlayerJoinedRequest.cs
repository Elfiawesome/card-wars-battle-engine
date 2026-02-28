using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Features;

public record struct PlayerJoinedRequestInput(
	EntityId Id
) : IInput;

public class PlayerJoinedRequestInputHandler : IInputHandler<PlayerJoinedRequestInput>
{
	public void Handle(InputContext context, PlayerJoinedRequestInput request)
	{
		if (context.Transaction.State.Get(request.Id) != null) return;
		BlockBatch batch = new([]);
		batch.Blocks.Add(new InstantiatePlayerBlock(request.Id));

		var turnState = context.Transaction.State.Turn.Copy();
		turnState.TurnOrder.Add(request.Id);

		if (!context.Transaction.State.All.Any((t) => t is Player))
		{
			turnState.TurnIndex = 0;
			turnState.AllowedPlayerInputs = [request.Id];
		}

		batch.Blocks.Add(new UpdateTurnStateBlock(turnState));
		context.Transaction.ApplyBlockBatch(batch);

		context.Transaction.QueueEvent(new PlayerJoinedEvent() { PlayerId = request.Id });
	}
}