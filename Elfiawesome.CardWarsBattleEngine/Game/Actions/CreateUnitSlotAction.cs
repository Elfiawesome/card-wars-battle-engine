using Elfiawesome.CardWarsBattleEngine.Game.Entities;

namespace Elfiawesome.CardWarsBattleEngine.Game.Actions;

public class CreateUnitSlotAction2(BattlefieldId battlefieldId, UnitSlotPos position) : GameAction
{
	public BattlefieldId TargetBattlefieldId { get; set; } = battlefieldId;
	public UnitSlotPos TargetUnitSlotPosition { get; set; } = position;

	public override void Execute(CardWarsBattleEngine engine)
	{
		if (engine._battlefields.TryGetValue(TargetBattlefieldId, out var battlefield))
		{
			battlefield.AddUnitSlot(TargetUnitSlotPosition);
		}
		else
		{
			InvalidateAction();
		}
	}
}