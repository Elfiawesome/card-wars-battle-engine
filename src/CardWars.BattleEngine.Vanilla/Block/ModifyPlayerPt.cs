using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class ModifyPlayerPtBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] int Amount
) : IBlock;

public class ModifyPlayerPtBlockHandler : IBlockHandler<ModifyPlayerPtBlock>
{
	public void Handle(GameState context, ModifyPlayerPtBlock request)
	{
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		player.Pt = Math.Max(0, player.Pt + request.Amount);
	}
}
