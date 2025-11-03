using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Block.Turn;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public class BlockDispatcher : RequestDispatcher<BattleEngine, IBlock, bool>
{
	public override void Register()
	{
		RegisterHandler(new InstantiatePlayerBlockHandler());
		
		RegisterHandler(new AddAllowedPlayerInputsBlockHandler());
		RegisterHandler(new AddTurnOrderBlockHandler());
		RegisterHandler(new SetTurnIndexBlockHandler());
	}
}