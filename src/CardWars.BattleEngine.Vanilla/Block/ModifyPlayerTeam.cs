using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class ModifyPlayerTeamBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] int Team
) : IBlock;

public class ModifyPlayerTeamBlockHandler : IBlockHandler<ModifyPlayerTeamBlock>
{
	public void Handle(GameState context, ModifyPlayerTeamBlock request)
	{
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		player.Team = request.Team;
	}
}
