using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.State;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class ModifyLayoutBlock(
	[property: DataTag] LayoutState Layout
) : IBlock;

public class ModifyLayoutBlockHandler : IBlockHandler<ModifyLayoutBlock>
{
	public void Handle(GameState context, ModifyLayoutBlock request)
	{
		context.Ext.GetOrCreateExtension<CardWarsBattleState>().Layout = request.Layout.Copy();
	}
}
