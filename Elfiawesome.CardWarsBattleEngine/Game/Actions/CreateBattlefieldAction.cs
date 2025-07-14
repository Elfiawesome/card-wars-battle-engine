using Elfiawesome.CardWarsBattleEngine.Game.Entities;

namespace Elfiawesome.CardWarsBattleEngine.Game.Actions;

public class CreateBattlefieldAction(BattlefieldId battlefieldId, List<UnitSlotPos> unitSlots) : GameAction
{
	public BattlefieldId BattlefieldId { get; set; } = battlefieldId;
	public List<UnitSlotPos> UnitSlots { get; set; } = unitSlots;

	public override void Execute(CardWarsBattleEngine engine)
	{
		var battlefield = engine.AddBattlefield(BattlefieldId);
		foreach (var unitSlotPos in UnitSlots)
		{
			battlefield.AddUnitSlot(unitSlotPos);
		}
	}
}