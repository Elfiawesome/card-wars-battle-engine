using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiateCardBlock(
	[property: DataTag] EntityId Id,
	[property: DataTag] CompoundTag? Data = null
) : IBlock;

public class InstantiateCardBlockHandler : IBlockHandler<InstantiateCardBlock>
{
	public void Handle(GameState context, InstantiateCardBlock request)
	{
		if (context.Get(request.Id) is not null)
		{
			Log.Warn($"Entity [{request.Id}] already exists, skipping InstantiateCardBlock"); return;
		}

		var card = new GenericCard(request.Id);
		if (request.Data is { } data)
			card.Data = (CompoundTag)data.Clone();
		context.Add(card);
	}
}