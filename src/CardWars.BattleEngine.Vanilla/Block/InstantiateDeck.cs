using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiateDeckBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiateDeckBlockHandler : IBlockHandler<InstantiateDeckBlock>
{
	public void Handle(GameState context, InstantiateDeckBlock request)
	{
		if (context.Get(request.Id) != null) return;
		context.Add(new Deck(request.Id));
	}
}