using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class RemoveEntityBlock( // TODO: Create the individual entity craetion instead?
	[property: DataTag] EntityId EntityId
) : IBlock;

public class RemoveEntityBlockHandler : IBlockHandler<RemoveEntityBlock>
{
	public void Handle(GameState context, RemoveEntityBlock request)
	{
		if (context.Remove(request.EntityId)) return;
		Log.Warn($"Entity [{request.EntityId}] was not found for removal");
	}
}
