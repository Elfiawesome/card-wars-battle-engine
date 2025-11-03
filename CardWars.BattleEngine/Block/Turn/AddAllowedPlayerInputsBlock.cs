using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Turn;

public record AddAllowedPlayerInputsBlock(
	PlayerId PlayerId,
	bool ClearPrev = true
) : IBlock;

public class AddAllowedPlayerInputsBlockHandler : IBlockHandler<AddAllowedPlayerInputsBlock>
{
	public bool Handle(BattleEngine context, AddAllowedPlayerInputsBlock request)
	{
		if (request.ClearPrev){ context.TurnService.AllowedPlayerInput.Clear(); }
		context.TurnService.AllowedPlayerInput.Add(request.PlayerId);
		return true;
	}
}