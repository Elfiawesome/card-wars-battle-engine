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
		RegisterHandler(new AttachUnitCardToPlayerBlockHandler());
		RegisterHandler(new AttachUnitSlotToBattlefieldBlockHandler());
		
		RegisterHandler(new InstantiateDeckBlockHandler());
		RegisterHandler(new InstantiatePlayerBlockHandler());
		RegisterHandler(new InstantiateBattlefieldBlockHandler());
		RegisterHandler(new InstantiateUnitCardBlockHandler());
		RegisterHandler(new InstantiateUnitSlotBlockHandler());

		RegisterHandler(new ModifyDeckAddBlockHandler());
		RegisterHandler(new ModifyDeckRemoveBlockHandler());
		RegisterHandler(new ModifyUnitCardSetBlockHandler());
		

		RegisterHandler(new AddAllowedPlayerInputsBlockHandler());
		RegisterHandler(new AddTurnOrderBlockHandler());
		RegisterHandler(new SetTurnIndexBlockHandler());
	}
}