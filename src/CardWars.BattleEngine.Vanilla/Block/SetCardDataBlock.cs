using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class SetCardDataBlock(
	[property: DataTag] EntityId TargetId,
	[property: DataTag] string Path,
	[property: DataTag] DataTag Value
) : IBlock;

public class SetCardDataBlockHandler : IBlockHandler<SetCardDataBlock>
{
	public void Handle(GameState context, SetCardDataBlock request)
	{
		if (context.Require<GenericCard>(request.TargetId) is not { } card) return;
		card.Data.SetByPath(request.Path, request.Value);
	}
}
