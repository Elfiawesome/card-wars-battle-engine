using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public class BlockDispatcher : RequestDispatcher<object, IBlock, object>
{
	public override void Register()
	{
	}
}