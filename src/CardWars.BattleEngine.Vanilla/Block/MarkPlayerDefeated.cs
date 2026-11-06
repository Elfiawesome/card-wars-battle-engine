using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class MarkHeroDefeatedBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId HeroCardId
) : IBlock;

public class MarkHeroDefeatedBlockHandler : IBlockHandler<MarkHeroDefeatedBlock>
{
	public void Handle(GameState context, MarkHeroDefeatedBlock request)
	{
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		player.DefeatedHeroIds.Add(request.HeroCardId);
	}
}

[DataTagType()]
public record class MarkPlayerDefeatedBlock(
	[property: DataTag] EntityId PlayerId
) : IBlock;

public class MarkPlayerDefeatedBlockHandler : IBlockHandler<MarkPlayerDefeatedBlock>
{
	public void Handle(GameState context, MarkPlayerDefeatedBlock request)
	{
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		player.IsDefeated = true;
	}
}
