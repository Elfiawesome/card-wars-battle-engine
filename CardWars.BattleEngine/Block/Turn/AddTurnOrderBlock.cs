using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Turn;

public record AddTurnOrderBlock(
	PlayerId PlayerId
) : IBlock;

public class AddTurnOrderBlockHandler : IBlockHandler<AddTurnOrderBlock>
{
	public bool Handle(BattleEngine context, AddTurnOrderBlock request)
	{
		context.TurnService.AddPlayer(request.PlayerId);
		return true;
	}
}