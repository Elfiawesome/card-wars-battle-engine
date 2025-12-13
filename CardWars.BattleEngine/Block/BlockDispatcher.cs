using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Block.Turn;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Block;

public class BlockDispatcher : RequestDispatcher<IServiceContainer, IBlock, bool>
{
	public override void Register()
	{
		// Entity
		RegisterHandler(new AttachDeckToPlayerBlockHandler());
		RegisterHandler(new AttachBattlefieldToPlayerBlockHandler());
		RegisterHandler(new AttachUnitSlotToBattlefieldBlockHandler());
		RegisterHandler(new InstantiateBattlefieldBlockHandler());
		RegisterHandler(new InstantiateDeckBlockHandler());
		RegisterHandler(new InstantiatePlayerBlockHandler());
		RegisterHandler(new InstantiateUnitSlotBlockHandler());

		RegisterHandler(new ModifyDeckAddBlockHandler());
		RegisterHandler(new ModifyDeckRemoveBlockHandler());

		// Turn
		RegisterHandler(new AddAllowedPlayerInputsBlockHandler());
		RegisterHandler(new AddTurnOrderBlockHandler());
		// RegisterHandler(new RemoveTurnOrderBlockHandler());
		RegisterHandler(new SetTurnIndexBlockHandler());

	}

	public void Handle(IServiceContainer serviceContainer, BlockBatch blockBatch)
	{
		foreach (var block in blockBatch.Blocks)
		{
			Handle(serviceContainer, block);
		}
	}
}