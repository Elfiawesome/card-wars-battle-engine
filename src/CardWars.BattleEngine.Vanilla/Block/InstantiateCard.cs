using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Data;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class InstantiateCardBlock(
	EntityId Id,
	CardDefinition? Definition = null
) : IBlock;

public class InstantiateCardBlockHandler : IBlockHandler<InstantiateCardBlock>
{
	public void Handle(GameState context, InstantiateCardBlock request)
	{
		if (context.Get(request.Id) != null) return;
		GenericCard newCard = request.Definition == null
			? new(request.Id)
			: new(request.Id)
			{
				Name = request.Definition.Name,
				Hp = request.Definition.Hp,
				Atk = request.Definition.Atk,
				Pt = request.Definition.Pt,
				Abilities = request.Definition.Abilities ?? [],
			};
		context.Add(newCard);
	}
}