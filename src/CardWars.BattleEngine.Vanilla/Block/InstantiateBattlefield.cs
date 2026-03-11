using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiateBattlefieldBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiateBattlefieldBlockHandler : IBlockHandler<InstantiateBattlefieldBlock>
{
	public void Handle(GameState context, InstantiateBattlefieldBlock request)
	{
		if (context.Get(request.Id) != null) return;
		var battlefield = new Battlefield(request.Id);
		context.Add(battlefield);
	}
}