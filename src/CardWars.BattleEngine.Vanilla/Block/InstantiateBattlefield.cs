using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiateBattlefieldBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiateBattlefieldBlockHandler : IBlockHandler<InstantiateBattlefieldBlock>
{
	public void Handle(GameState context, InstantiateBattlefieldBlock request)
	{
		if (context.Get(request.Id) != null)
		{
			Logger.Warn($"Entity [{request.Id}] already exists, skipping InstantiateBattlefield"); return;
		}
		var battlefield = new Battlefield(request.Id);
		context.Add(battlefield);
	}
}