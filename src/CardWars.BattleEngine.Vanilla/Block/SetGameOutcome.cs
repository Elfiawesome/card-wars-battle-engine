using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class SetGameOutcomeBlock(
	[property: DataTag] bool IsGameOver,
	[property: DataTag] int? WinningTeam,
	[property: DataTag] bool IsDraw
) : IBlock;

public class SetGameOutcomeBlockHandler : IBlockHandler<SetGameOutcomeBlock>
{
	public void Handle(GameState context, SetGameOutcomeBlock request)
	{
		context.IsGameOver = request.IsGameOver;
		context.WinningTeam = request.WinningTeam;
		context.IsDraw = request.IsDraw;
	}
}
