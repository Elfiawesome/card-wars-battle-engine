using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public record struct PlayerJoinedRequestInput(
	[property: DataTag] EntityId Id,
	[property: DataTag] int RequestedTeam = 1
) : IInput;

public class PlayerJoinedRequestInputHandler : IInputHandler<PlayerJoinedRequestInput>
{
	public void Handle(InputContext context, PlayerJoinedRequestInput request)
	{
		if (context.Transaction.State.Get(request.Id) != null) { Log.Warn($"Player [{request.Id}] already exists, ignoring join request"); return; }
		BlockBatch batch = new([]);
		batch.Blocks.Add(new InstantiatePlayerBlock(request.Id));
		batch.Blocks.Add(new ModifyPlayerTeamBlock(request.Id, request.RequestedTeam));

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