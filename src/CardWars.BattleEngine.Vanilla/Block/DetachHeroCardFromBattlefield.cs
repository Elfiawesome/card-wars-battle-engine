using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachHeroCardFromBattlefieldBlock(
	[property: DataTag] EntityId BattlefieldId,
	[property: DataTag] EntityId CardId
) : IBlock;

public class DetachHeroCardFromBattlefieldBlockHandler : IBlockHandler<DetachHeroCardFromBattlefieldBlock>
{
	public void Handle(GameState context, DetachHeroCardFromBattlefieldBlock request)
	{
		if (context.Require<Battlefield>(request.BattlefieldId) is not { } battlefield) return;
		if (context.Require<GenericCard>(request.CardId) is not { } card) return;

		if (battlefield.HeroCardIds.Remove(request.CardId) && card.OwnerBattlefieldId == request.BattlefieldId)
			card.OwnerBattlefieldId = null;
	}
}
