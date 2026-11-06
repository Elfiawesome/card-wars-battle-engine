using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachHeroCardToBattlefieldBlock(
	[property: DataTag] EntityId BattlefieldId,
	[property: DataTag] EntityId CardId
) : IBlock;

public class AttachHeroCardToBattlefieldBlockHandler : IBlockHandler<AttachHeroCardToBattlefieldBlock>
{
	public void Handle(GameState context, AttachHeroCardToBattlefieldBlock request)
	{
		if (context.Require<Battlefield>(request.BattlefieldId) is not { } battlefield) return;
		if (context.Require<GenericCard>(request.CardId) is not { } card) return;
		if (!card.IsHero) return;

		card.OwnerBattlefieldId = battlefield.Id;
		if (!battlefield.HeroCardIds.Contains(card.Id))
			battlefield.HeroCardIds.Add(card.Id);

		Log.Debug($"Attached hero [{card.Id}] to battlefield [{battlefield.Id}]");
	}
}
