using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionDatas.Creations;

public class CreateUnitActionData(UnitId unitId) : ActionData
{
	public UnitId UnitId { get; } = unitId;
}