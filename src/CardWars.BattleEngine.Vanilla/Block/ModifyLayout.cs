using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class ModifyLayoutBlock(
	[property: DataTag] LayoutState Layout
) : IBlock;

public class ModifyLayoutBlockHandler : IBlockHandler<ModifyLayoutBlock>
{
	public void Handle(GameState context, ModifyLayoutBlock request)
	{
		context.Layout = request.Layout.Copy();
	}
}
