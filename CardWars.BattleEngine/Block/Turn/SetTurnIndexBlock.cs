namespace CardWars.BattleEngine.Block.Turn;

public record SetTurnIndexBlock(
	int TurnIndex,
	TurnService.PhaseType Phase,
	bool IsPhaseChanged = false // This is for in case the client needs animation for "new phase entered"
) : IBlock;

public class SetTurnIndexBlockHandler : IBlockHandler<SetTurnIndexBlock>
{
	public bool Handle(BattleEngine context, SetTurnIndexBlock request)
	{
		context.TurnService.TurnNumber = request.TurnIndex;
		context.TurnService.Phase = request.Phase;
		return true;
	}
}