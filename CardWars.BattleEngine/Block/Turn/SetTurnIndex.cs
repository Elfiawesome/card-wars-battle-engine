using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Turn;

public record struct SetTurnIndexBlock(
	int TurnIndex,
	TurnPhase Phase,
	bool IsPhaseChanged = false // This is for in case the client needs animation for "new phase entered"
) : IBlock;

public class SetTurnIndexBlockHandler : IBlockHandler<SetTurnIndexBlock>
{
	public bool Handle(IServiceContainer service, SetTurnIndexBlock request)
	{
		service.State.TurnIndex = request.TurnIndex;
		service.State.TurnPhase = request.Phase;
		return true;
	}
}