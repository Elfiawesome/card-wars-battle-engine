using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Data;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class InstantiateCardBlock(
	EntityId Id,
	CompoundTag? Data = null
) : IBlock;

public class InstantiateCardBlockHandler : IBlockHandler<InstantiateCardBlock>
{
	public void Handle(GameState context, InstantiateCardBlock request)
	{
		if (context.Get(request.Id) != null) return;
		var card = new GenericCard(request.Id);
		if (request.Data != null)
			card.Data = (CompoundTag)request.Data.Clone();
		context.Add(card);
	}
}