using CardWars.BattleEngine.Core.Actions.ActionHandlers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public class CreateBattlefieldResolver(BattlefieldId battlefieldId, PlayerId playerId) : Resolver
{
	public BattlefieldId BattlefieldId = battlefieldId;
	public PlayerId PlayerId = playerId;// Used so that he can any abilities or something

	public override void Resolve(GameState state)
	{
		// Create unit slots
		for (int i = 0; i < 4; i++)
		{
			UnitSlotId usid = new(state.NewId);
			AddActionBatch(new([
				new InstantiateUnitSlotData(usid),
				new AttachUnitSlotToBattlefieldData(usid, BattlefieldId),
			]));
		}
		CommitResolve();
	}
}