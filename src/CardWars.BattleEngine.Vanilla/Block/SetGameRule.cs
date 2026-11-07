using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class SetGameRuleBlock(
	[property: DataTag] ResourceId RuleId
) : IBlock;

public class SetGameRuleBlockHandler : IBlockHandler<SetGameRuleBlock>
{
	public void Handle(GameState context, SetGameRuleBlock request)
	{
		context.RuleId = request.RuleId;
	}
}
