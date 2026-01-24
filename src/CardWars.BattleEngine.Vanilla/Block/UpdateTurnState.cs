using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class UpdateTurnStateBlock(
	TurnState NewState
) : IBlock;

public class UpdateTurnStateBlockHandler : IBlockHandler<UpdateTurnStateBlock>
{
	public void Handle(GameState context, UpdateTurnStateBlock request)
	{
		context.Turn = request.NewState;
	}
}