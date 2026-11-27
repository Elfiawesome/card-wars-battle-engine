using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public record struct PlayerLeftRequestInput(
	[property: DataTag] EntityId PlayerId
) : IInput;

public class PlayerLeftRequestInputHandler : IInputHandler<PlayerLeftRequestInput>
{
	public void Handle(InputContext context, PlayerLeftRequestInput request)
	{
		var state = context.Transaction.State;
		if (state.Get<Player>(request.PlayerId) is not { IsDefeated: false } player) return;

		context.Transaction.ApplyBlockBatch(new BlockBatch([
			new MarkPlayerDefeatedBlock(player.Id)
		]));

		context.Transaction.QueueEvent(new PlayerDefeatedEvent
		{
			PlayerId = player.Id,
			Team = player.Team
		});
		context.Transaction.QueueEvent(new WinConditionCheckEvent
		{
			TurnState = state.Ext.GetOrCreate<CardWarsBattleState>().Turn.Clone()
		});
	}
}
