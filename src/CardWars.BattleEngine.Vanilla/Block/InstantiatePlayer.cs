using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiatePlayerBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiatePlayerBlockHandler : IBlockHandler<InstantiatePlayerBlock>
{
	public void Handle(GameState context, InstantiatePlayerBlock request)
	{
		if (context.Get(request.Id) != null)
		{
			Logger.Warn($"Entity [{request.Id}] already exists, skipping InstantiatePlayerBlock"); return;
		}
		context.Add(new Player(request.Id));
	}
}