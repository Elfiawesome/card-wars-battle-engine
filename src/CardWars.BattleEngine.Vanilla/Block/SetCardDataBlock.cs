using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class SetCardDataBlock(
	EntityId TargetId,
	string Path,
	DataTag Value
) : IBlock;

public class SetCardDataBlockHandler : IBlockHandler<SetCardDataBlock>
{
	public void Handle(GameState context, SetCardDataBlock request)
	{
		if (context.Get(request.TargetId) is not GenericCard card) return;
		card.Data.SetByPath(request.Path, request.Value);
	}
}
