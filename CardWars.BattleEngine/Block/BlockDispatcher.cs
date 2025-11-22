using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Block.Turn;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public class BlockDispatcher : RequestDispatcher<BattleEngine, IBlock, bool>
{
	public override void Register()
	{
		RegisterHandler(new AttachBattlefieldToPlayerBlockHandler());
		RegisterHandler(new AttachDeckToPlayerBlockHandler());
		RegisterHandler(new AttachUnitSlotToBattlefieldBlockHandler());
		RegisterHandler(new InstantiatePlayerBlockHandler());
		RegisterHandler(new InstantiateDeckBlockHandler());
		RegisterHandler(new InstantiateBattlefieldBlockHandler());
		RegisterHandler(new InstantiateUnitSlotBlockHandler());
		
		RegisterHandler(new AddAllowedPlayerInputsBlockHandler());
		RegisterHandler(new AddTurnOrderBlockHandler());
		RegisterHandler(new SetTurnIndexBlockHandler());

		RegisterHandler(new ModifyDeckAddBlockHandler());
	}
}