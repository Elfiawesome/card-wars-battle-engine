using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class UpdateTurnStateBlock(
	[property: DataTag] TurnState NewState
) : IBlock;

public class UpdateTurnStateBlockHandler : IBlockHandler<UpdateTurnStateBlock>
{
	public void Handle(GameState context, UpdateTurnStateBlock request)
	{
		context.Turn = request.NewState;
	}
}