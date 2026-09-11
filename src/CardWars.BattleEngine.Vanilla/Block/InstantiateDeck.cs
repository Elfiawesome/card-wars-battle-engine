using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiateDeckBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiateDeckBlockHandler : IBlockHandler<InstantiateDeckBlock>
{
	public void Handle(GameState context, InstantiateDeckBlock request)
	{
		if (context.Get(request.Id) != null)
		{
			Log.Warn($"Entity [{request.Id}] already exists, skipping InstantiateDeckBlock"); return;
		}
		context.Add(new Deck(request.Id));
	}
}