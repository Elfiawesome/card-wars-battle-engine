using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Turn;

public record struct AddTurnOrderBlock(
	PlayerId PlayerId
) : IBlock;

public class AddTurnOrderBlockHandler : IBlockHandler<AddTurnOrderBlock>
{
	public bool Handle(IServiceContainer service, AddTurnOrderBlock request)
	{
		service.State.TurnOrder.Add(request.PlayerId);
		return true;
	}
}