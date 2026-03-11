using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiatePlayerBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiatePlayerBlockHandler : IBlockHandler<InstantiatePlayerBlock>
{
	public void Handle(GameState context, InstantiatePlayerBlock request)
	{
		if (context.Get(request.Id) != null) return;
		context.Add(new Player(request.Id));
	}
}