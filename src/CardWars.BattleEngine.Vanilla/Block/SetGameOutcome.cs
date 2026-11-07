using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class SetGameOutcomeBlock(
	[property: DataTag] GameOutcome Outcome
) : IBlock;

public class SetGameOutcomeBlockHandler : IBlockHandler<SetGameOutcomeBlock>
{
	public void Handle(GameState context, SetGameOutcomeBlock request)
	{
		context.Outcome = request.Outcome;
	}
}
